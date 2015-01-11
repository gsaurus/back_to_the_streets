using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

	

// information about each peer state
sealed class NetworkSyncState{
	// pings count only matters 
	private int pingsCount;
	public float TravelTime { get; private set; }
	public bool isReady;

	public void Update(float tt){
		float integrationRatio = 1f / (pingsCount + 1);
		if (integrationRatio > NetworkSync.lagIntegrationRate) {
			++pingsCount;
		}else {
			integrationRatio = NetworkSync.lagIntegrationRate;
		}
		TravelTime = (TravelTime * (1 - integrationRatio)) + (tt * integrationRatio);
		//Debug.Log("Ping response: " + tt + ", integrated: " + TravelTime + ", ratio: " + integrationRatio + ", count: " + pingsCount);
	}

}


// Synchronize clock in the network
// Note that we don't use Unity's Network.ping because outgoing and ingoing travel
// times may be different, so we compute our own outgoing tts
public sealed class NetworkSync: SingletonMonoBehaviour<NetworkSync>{

	// Minimum rate in which lag accomodates to network latency changes
	public static float lagIntegrationRate = 0.1f;

	// How much of the latency we use in our game input
	public static float lagCompensationRate = 1.0f; 

	// how often to ping peers
	public static float pingRate = 2.0f;

	// Synchronization state between this peer and all others (key = guid)
	private Dictionary<string, NetworkSyncState> syncStates;

	// Flag telling whether we're ready to play or not
	// This can be used to prevent game from proceeding while we're loading assets for instance
	private bool isReady;


	// Events for when players get ready (most useful for server)
	// It can be used to start the game when everyone is ready, or 
	// to pause the game when someone went unready, or to send
	// the current game state to a player that got ready
	public delegate void OnPlayerReadyOrNotEvent(string guid);
	public event OnPlayerReadyOrNotEvent playerReadyEvent;
	public event OnPlayerReadyOrNotEvent playerNotReadyEvent;
	

	// On awake we register delegates
	void Awake(){
		// register connection delegates
		NetworkCenter.Instance.playerConnectedEvent += OnPlayerConnectionConfirmed;
		NetworkCenter.Instance.playerDisconnectedEvent += OnPlayerDisconnectionConfirmed;
	}

	// On destroy we unregister delegates
	void OnDestroy(){
		// unregister connection delegates
		NetworkCenter.Instance.playerConnectedEvent -= OnPlayerConnectionConfirmed;
		NetworkCenter.Instance.playerDisconnectedEvent -= OnPlayerDisconnectionConfirmed;
	}
	

	// Get the maximum lag with all connected peers
	public float GetLagTime(){
		if (syncStates == null) return 0;
		float maxLag = 0;
		foreach (NetworkSyncState syncState in syncStates.Values){
			if (syncState.TravelTime > maxLag){
				maxLag = syncState.TravelTime;
			}
		}
		return maxLag;
	}

	// Get the lag to some other player
	public float GetLagTime(string guid){
		if (syncStates == null) return 0;
		NetworkSyncState state;
		if (syncStates.TryGetValue(guid, out state)){
			return state.TravelTime;
		}
		return 0;
	}


#region Ping - Pong

	// Ping peers every now and then
	IEnumerator PingPeers(){
		// wait just a litle bit before start pinging, avoid initial unstability
		yield return new WaitForSeconds(0.1f);
		if (syncStates == null || !NetworkCenter.Instance.IsConnected()) yield break;
		while(syncStates.Count > 0){
			networkView.RPC("PingRequest", RPCMode.Others);
			yield return new WaitForSeconds(pingRate);
		}
		Debug.Log("Coroutine finished");
	}


	// When we get a ping request, we send the travel time back to sender
	[RPC]
	void PingRequest(NetworkMessageInfo messageInfo){
		float travelTime = (float) (Network.time - messageInfo.timestamp);
		// We explicitly send our guid in the RPC because of Unity bug not seting sender guid correctly
		networkView.RPC("PingResponse", messageInfo.sender, travelTime, Network.player.guid);
	}

	// On receiving the ping response we integrate the travel time
	[RPC]
	void PingResponse(float travelTime, string senderGuid){
		if (syncStates == null) return;
		NetworkSyncState syncState;
		if (!syncStates.TryGetValue(senderGuid, out syncState)) {
			Debug.LogWarning("Got a ping response from an unknown player " + senderGuid);
			return;
		}
		syncState.Update (travelTime);
	}

#endregion



#region Ready

	// Mark this player as ready
	public void SetReady(bool ready){
		if (ready != isReady){
			networkView.RPC("SetPlayerReady", RPCMode.All, Network.player.guid, ready);
		}
	}

	[RPC]
	void SetPlayerReady(string guid, bool ready){
		if (syncStates == null) return;

		if (guid == Network.player.guid){
			isReady = ready;
		}else {
			syncStates[guid].isReady = ready;
		}

		// dispatch events about players being ready or not
		if (ready){
			if (playerReadyEvent != null){
				playerReadyEvent(guid);
			}
		}else {
			if (playerNotReadyEvent != null){
				playerNotReadyEvent(guid);
			}
		}
	}

	// Check if everyone is ready, including ourselves
	public bool IsEveryoneReady(){
		if (syncStates == null) return false;
		if (!isReady) return false;
		foreach (NetworkSyncState syncState in syncStates.Values){
			if (!syncState.isReady){
				return false;
			}
		}
		return true;
	}
	
	// Check if a specific player is ready
	public bool IsPlayerReady(string guid){
		if (guid == Network.player.guid) {
			return isReady;
		}
		if (syncStates == null) return false;
		NetworkSyncState state;
		if (syncStates.TryGetValue(guid, out state)){
			return state.isReady;
		}
		return false;
	}
	
	// Is our player ready?
	public bool IsPlayerReady(){
		return isReady;
	}

	// Get a list of the guis of all ready players
	public List<string> GetReadyPlayerGuids(){
		if (syncStates == null) return null;
		List<string> readyPlayers = new List<string>(syncStates.Count);
		if (isReady) readyPlayers.Add(Network.player.guid);

		foreach(KeyValuePair<string, NetworkSyncState> entry in syncStates){
			if (entry.Value.isReady){
				readyPlayers.Add(entry.Key);
			}
		}
		return readyPlayers;
	}

#endregion
	
	// When the player first connects with someone else we start the ping coroutine
	void OnPlayerConnectionConfirmed(string guid) {
		if (guid != Network.player.guid) {
			NetworkSyncState syncState = new NetworkSyncState();
			if (syncStates == null) {
				syncStates = new Dictionary<string, NetworkSyncState>();
			}
			syncStates.Add(guid, syncState);
			if (syncStates.Count == 1) {
				StopCoroutine(PingPeers());
				StartCoroutine(PingPeers());
			}
		}
	}
	

	// When a player discommects we remove it's state
	// If it's the player itself we stop the ping coroutine
	void OnPlayerDisconnectionConfirmed(string guid) {
		if (guid == Network.player.guid) {
			StopCoroutine(PingPeers());
			isReady = false;
		}else if (syncStates != null){
			syncStates.Remove(guid);
		}
	}


}

