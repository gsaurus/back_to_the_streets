using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



namespace RetroBread{
	namespace Network{


		// Stores all players information
		// Handles connections, reconnections and server promotions
		public sealed class NetworkCenter: SingletonMonoBehaviour<NetworkCenter>{
			
			
		#region Fields

			// Serializer to encode/decode player info into/from network messages
			public Serializer serializer = Serializer.defaultSerializer;


			// All information about all connected players is here, by guid
			private SerializableDictionary<string, NetworkPlayerData> players = new SerializableDictionary<string, NetworkPlayerData>();

			// Matching between the players uniqueId and their number in the game
			// If a player reconnects he's assigned to the same player number, if available
			// Player numbers are tentaptively kept even if a player goes offline
			private SerializableDictionary<string, uint> playerNumbers = new SerializableDictionary<string, uint>();

			// If locked, new players won't overtake disconnected players place
			// If unlocked, new players take the first free slot
			public bool playersLocked = false;

			// Events for when players connect / disconnect 
			public delegate void OnPlayerConnectionEvent(string guid);
			public event OnPlayerConnectionEvent playerConnectedEvent;
			public event OnPlayerConnectionEvent playerDisconnectedEvent;

		#endregion



		#region Assessors

			// Set our own player data
			public void SetPlayerData(NetworkPlayerData data) {
						players[UnityEngine.Network.player.guid] = data;
			}

			// Get a player's data
			public T GetPlayerData<T>(NetworkPlayer player) where T:NetworkPlayerData{
				NetworkPlayerData data;
				players.TryGetValue(player.guid, out data);
				return data as T;
			}

			// Get our own player data
			public T GetPlayerData<T>() where T:NetworkPlayerData{
				return GetPlayerData<T>(UnityEngine.Network.player);
			}

			// Get a player's data
			public NetworkPlayerData GetPlayerData(NetworkPlayer player){
				return GetPlayerData(player.guid);
			}

			// Get a player's data from it's network guid
			public NetworkPlayerData GetPlayerData(string guid){
				if (guid == "" || guid == "0") guid = UnityEngine.Network.player.guid;
				NetworkPlayerData data;
				players.TryGetValue(guid, out data);
				return data;
			}
			
			// Get our own player data
			public NetworkPlayerData GetPlayerData(){
				return GetPlayerData(UnityEngine.Network.player);
			}


			private string GetPlayerUniqueId(uint playerNumber){
				foreach(KeyValuePair<string, uint> pair in playerNumbers){
					if (pair.Value == playerNumber){
						return pair.Key;
					}
				}
				return null;
			}

			public NetworkPlayerData GetPlayerData(uint playerNumber){
				string uniqueId = GetPlayerUniqueId(playerNumber);
				if (uniqueId == null) return null;
				foreach(NetworkPlayerData playerData in players.Values){
					if (playerData.uniqueId == uniqueId){
						return playerData;
					}
				}
				return null;
			}

			// Get the number of the player given it's network player data
			public int GetPlayerNumber(NetworkPlayerData playerData){
				if (playerData == null) return -1;
				uint playerNumber;
				if (playerNumbers.TryGetValue(playerData.uniqueId, out playerNumber)){
					return (int) playerNumber;
				}
				return -1;
			}
			
			// Get the number of the player given it's network instance
			public int GetPlayerNumber(NetworkPlayer player) {
				NetworkPlayerData data = GetPlayerData(player);
				return GetPlayerNumber(data);
			}

			// Get the number of the player given it's network guid
			public int GetPlayerNumber(string playerGuid) {
				NetworkPlayerData data = GetPlayerData(playerGuid);
				return GetPlayerNumber(data);
			}

			// Get our player number
			public int GetPlayerNumber(){
				return GetPlayerNumber(UnityEngine.Network.player);
			}

			// Number of connected players
			public int GetNumPlayersOnline(){
				return players.Count;
			}

			// Are we connected with someone?
			public bool IsConnected(){
				return UnityEngine.Network.connections.Length > 0;
			}

			public List<uint> GetAllNumbersOfConnectedPlayers(){
				List<uint> res = new List<uint>();
				foreach(NetworkPlayerData playerData in players.Values){
					res.Add((uint)GetPlayerNumber(playerData));
				}
				return res;
			}


			public static bool TryGetNetworkPlayerForGuid(string guid, out NetworkPlayer player){
				foreach (NetworkPlayer p in UnityEngine.Network.connections){
					if (guid == p.guid){
						player = p;
						return true;
					}
				}
				player = new NetworkPlayer();
				return false;
			}


		#endregion


		#region Server

			// When server is initialized, he's connected
			void OnServerInitialized() {
				NetworkPlayerData playerData = GetPlayerData();
				playerNumbers[playerData.uniqueId] = FindSlotForPlayer(playerData);
				if (playerConnectedEvent != null) {
					playerConnectedEvent(UnityEngine.Network.player.guid);
				}
			}
			
			// Server received a new connection.
			// Wait for client to anounce his player data
			void OnPlayerConnected(NetworkPlayer player) {
				//Debug.Log("Player connected from: " + player.ipAddress + ", guid: " + player.guid);
				StartCoroutine(WaitForPlayerData(player));
			}

			// If after a few seconds the player wasn't added, close it's connection
			IEnumerator WaitForPlayerData(NetworkPlayer player){
				//Debug.Log("wait");
				yield return new WaitForSeconds(5);
				if (GetPlayerData(player) == null) {
					//Debug.Log("wait ended, kill!");
					TryCloseConnection(player);
				}
			}


			// Server receives client player data
			[RPC]
			void OnClientPlayerDataAnounced(byte[] data, NetworkMessageInfo info) {

				// ignore if not server
				if (!UnityEngine.Network.isServer) return;

				// Deserialize data
				NetworkPlayerData playerData = serializer.Deserialize(data) as NetworkPlayerData;
				if (playerData == null) {
					// Woops, invalid player data! Disconnect it
					Debug.LogWarning("Invalid player data from " + info.sender.ipAddress);
					TryCloseConnection(info.sender);
					return;
				}

				// Then see if we have room for this user
				uint playerNumber = FindSlotForPlayer(playerData);
				if (playerNumber > UnityEngine.Network.maxConnections) {
					// No room for it
					Debug.Log("No room in server for " + playerData.playerName);
					TryCloseConnection(info.sender);
					return;
				}

				// Everything went well
				byte[] allPlayersData = serializer.Serialize(players);
				byte[] allPlayerNumsData = serializer.Serialize(playerNumbers);

				// send current players to the newcomer
				GetComponent<NetworkView>().RPC("SetAllPlayersData", info.sender, allPlayersData, allPlayerNumsData);

				// and notify all players about the newcomer as well
				GetComponent<NetworkView>().RPC("AddPlayerData", RPCMode.All, (int)playerNumber, data, info.sender);
			}


			// Find a suitable player number for the given player
			uint FindSlotForPlayer(NetworkPlayerData playerData){

				// Get all slots in use
				List<uint> slotsInUse;

				uint index;
				if (playerNumbers.TryGetValue(playerData.uniqueId, out index)){
					// Theres a slot reserved to this player
					return index;
				}

				if (playersLocked) {
					// include those that aren't connected
					slotsInUse = new List<uint>(playerNumbers.Count);
					foreach(uint val in playerNumbers.Values) {
						slotsInUse.Add(val);
					}
				}else {
					// filter those in use only
					slotsInUse = new List<uint>(players.Count);
					uint val;
					foreach (NetworkPlayerData otherPlayerData in players.Values){
						if (playerNumbers.TryGetValue(otherPlayerData.uniqueId, out val)){
							slotsInUse.Add(val);
						}
					}
				}
				// Find first slot available
				slotsInUse.Sort();
				index = 0;
				while (index < slotsInUse.Count && slotsInUse[(int)index] == index) {
					++index;
				}

				return index;
			}



			// Server lost a client
			// Remove his player data, but keep the slot available for reconnection
			void OnPlayerDisconnected(NetworkPlayer player) {
				Debug.Log("Player disconnected: " + player.ipAddress);
				// notify all clients
				GetComponent<NetworkView>().RPC("RemovePlayerData", RPCMode.All, player);
			}


			// Add all received players to our dictionaries
			[RPC]
			void SetAllPlayersData(byte[] playersData, byte[] playerNumsData){
				players = serializer.Deserialize(playersData) as SerializableDictionary<string, NetworkPlayerData>;
				playerNumbers = serializer.Deserialize(playerNumsData) as SerializableDictionary<string, uint>;

				// Send one notification about each received player
				if (playerConnectedEvent != null){
					foreach (string guid in players.Keys){
						playerConnectedEvent(guid);
					}
				}
			}

			// Add the received player
			[RPC]
			void AddPlayerData(int playerNumber, byte[] data, NetworkPlayer sender){

				NetworkPlayerData playerData = serializer.Deserialize(data) as NetworkPlayerData;
				if (playerData == null) {
					// Woops, invalid player data!
					Debug.LogWarning("Invalid player data from " + sender.ipAddress);
					return;
				}

				playerNumbers[playerData.uniqueId] = (uint)playerNumber;
				players[sender.guid] = playerData;

				// Send notification
				if (playerConnectedEvent != null) {
					playerConnectedEvent(sender.guid);
				}

			}

			// Remove a player
			[RPC]
			void RemovePlayerData(NetworkPlayer player, NetworkMessageInfo info) {
				// Send event notification
				if (playerDisconnectedEvent != null) {
					playerDisconnectedEvent(player.guid);
				}

				// Remove only after notification so others can still access it's data
				UnityEngine.Network.RemoveRPCs(player);
				UnityEngine.Network.DestroyPlayerObjects(player);
				players.Remove(player.guid);
			}


		#endregion


		#region Client

			// Connected
			// When the client connects to the server, we send our player data to it
			void OnConnectedToServer(){

				// Send our data to server
				NetworkPlayerData playerData = GetPlayerData();
				if (playerData == null) {
					// Wops, we can't play without data
					Debug.LogWarning("No Player Data to anounce to server");
					UnityEngine.Network.Disconnect();
					return;
				}
				byte[] data = serializer.Serialize(playerData);
				GetComponent<NetworkView>().RPC("OnClientPlayerDataAnounced", RPCMode.Server, data);

				// wait for server response
				StartCoroutine(WaitForServerData());
			}

			// If after a few seconds the server didn't accept this player, close connection
			IEnumerator WaitForServerData(){
				yield return new WaitForSeconds(5);
				if (GetNumPlayersOnline() <= 1) {
					UnityEngine.Network.Disconnect();
				}
			}

			
		//	// Disconnected
		//	void OnDisconnectedFromServer(NetworkDisconnection reason){
		//		if (Network.isServer) {
		//			Debug.Log("Server disconnected!");
		//		}else {
		//			switch (reason){
		//				case NetworkDisconnection.LostConnection:{
		//					Debug.Log("Lost connection to the server");
		//				}break;
		//				case NetworkDisconnection.Disconnected: {
		//					Debug.Log("Gracefully disconnected from the server");
		//				} break;
		//				default: {
		//					Debug.Log("Disconnected for unknown reason");
		//				}break;
		//			}
		//		}
		//	}
		//	
		//	// Failed to connect
		//	void OnFailedToConnect(NetworkConnectionError error) {
		//		Debug.Log("Could not connect to server: " + error);
		//	}

		#endregion



		#region Server Promotion

			//TODO: I can do this LATER

		#endregion


			void TryCloseConnection(NetworkPlayer player){
				foreach (NetworkPlayer p in UnityEngine.Network.connections){
					if (p.guid == player.guid){
						Debug.Log("Closing client: " + player.ipAddress + ", guid: " + player.guid);
						UnityEngine.Network.CloseConnection(p, true);
					}
				}
			}


			void OnApplicationQuit() {
				// TODO: store info so we can restore the networked game
				UnityEngine.Network.Disconnect();
			}






			//	void DebugListPlayersOnChat(string intro){
			//		if (Network.connections.Length == 0) return;
			//		NetworkChat chat = GetComponent<NetworkChat>();
			//		chat.SendTextMessage(Network.player.guid + " Listing due to: " + intro);
			//		foreach (NetworkPlayerData playerData in players.Values) {
			//			chat.SendTextMessage(Network.player.guid + " " + playerData.playerName);
			//		}	
			//	}
			//	void DebugToChat(string info){
			//		if (Network.connections.Length == 0) return;
			//		NetworkChat chat = GetComponent<NetworkChat>();
			//		chat.SendTextMessage(Network.player.guid + " " + info);
			//	}

		}



	}
}
