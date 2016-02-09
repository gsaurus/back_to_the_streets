using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace RetroBread{
	namespace Network{


		// List available servers
		// Let player connect to an existing server
		// Let player create a new server
		public sealed class NetworkMaster: SingletonMonoBehaviour<NetworkMaster>{

//			public static string masterServerIP = "127.0.0.1"; //"192.168.7.68";


			// A host state indicating it's open to new connections
			public static readonly string networkHostAvailable = "open";

			// When new hosts are discovered we send OnHostFound events 
			public delegate void OnHostFound(HostData host);
			public event OnHostFound hostFoundEvent;

			// game unique identifier
			public string gameIdentifier = "Put Game Name here";

			public int port = 27886; // Use same as kaillera on this example..

			// list of hosts found so far
			public List<HostData> hosts { get; private set; }

			public bool IsAnouncingServer { get; private set; }



			// We start by refreshing servers list
			void OnEnable(){
				RefreshServersList();
				IsAnouncingServer = false;
			}


			public void RefreshServersList() {
//				MasterServer.ipAddress = masterServerIP;
//				MasterServer.port = 23466;
//				UnityEngine.Network.natFacilitatorIP = masterServerIP;
//				UnityEngine.Network.natFacilitatorPort = 50005;
				MasterServer.ClearHostList();
				hosts = new List<HostData>();
				MasterServer.RequestHostList(gameIdentifier);
			}


			public static bool IsServerAvailable(HostData host) {
				return host.comment == networkHostAvailable && host.connectedPlayers < host.playerLimit;
			}

			public static bool IsServerPasswordProtected(HostData host){
				return host.passwordProtected;
			}


			void Update() {
				// Check if we received any hosts so far
				if (MasterServer.PollHostList().Length != 0) {
					HostData[] receivedHosts = MasterServer.PollHostList();

					foreach (HostData hostData in receivedHosts){
						hosts.Add(hostData);
						// Notify listeners
						if (hostFoundEvent != null) {
							hostFoundEvent(hostData);
						}
					}

					MasterServer.ClearHostList();
				}
			}


			public void CreateServer(int maxPlayers, string password = null){
				UnityEngine.Network.incomingPassword = password;
				bool useNat = !UnityEngine.Network.HavePublicAddress();
				UnityEngine.Network.InitializeServer(maxPlayers-1, port, useNat);
			}

			public void CancelServer(){
				MasterServer.UnregisterHost();
				IsAnouncingServer = false;
				enabled = true;
			}


			void OnServerInitialized() {
				NetworkPlayerData myPlayerData = NetworkCenter.Instance.GetPlayerData();
				string gameName = myPlayerData != null ? myPlayerData.playerName : "guest";
				MasterServer.RegisterHost(gameIdentifier, gameName, networkHostAvailable);
				IsAnouncingServer = true;
			}


			public void ConnectToServer(HostData host, string password = null) {
				UnityEngine.Network.Connect(host, password);
			}

			void OnConnectedToServer() {
				// Once connected, disable NetworkMaster
				enabled = false;
			}

			// Let other components handle failure
		//	void OnFailedToConnect(NetworkConnectionError error) {
		//		//Debug.Log("Could not connect to server: " + error);
		//	}


			void OnDisable(){
				CancelServer();
			}

			void OnApplicationQuit() {
				CancelServer();
			}

		}



	}
}
