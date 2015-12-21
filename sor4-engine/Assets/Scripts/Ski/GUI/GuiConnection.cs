using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;

public class GuiConnection : MonoBehaviour
{
	public GameObject mainParent;
	public GameObject goButton;
	public GameObject inGameParent;

	public UnityEngine.UI.Text infoText;

	private Vector2 scrollPosition;
	
	void Awake(){
	
		int randomId = Random.Range(0,int.MaxValue);
		NetworkCenter.Instance.SetPlayerData(new NetworkPlayerData(SystemInfo.deviceUniqueIdentifier + randomId, "guest_" + randomId));
	
		infoText.text = "Looking for Servers...";

		// Search for servers
		if (!TryToConnectToAvailableServer()){
			// try a few seconds later
			StartCoroutine(RetryToConnectToAvailableServer());
		}
	}
	

	void CreateNewServer(){
		// Setup game state
		WorldModel world = new WorldModel();
		
		StateManagerSetup setup = new StateManagerSetup(world);
		StateManager.Instance.Setup(setup);
		
		NetworkMaster.Instance.CreateServer(6);

		infoText.text = "Created New Server.\nWaiting for players";
	}


	bool TryToConnectToAvailableServer(){
		List<HostData> hosts = NetworkMaster.Instance.hosts;
		if (hosts != null){
			foreach (HostData host in hosts) {
				if (NetworkMaster.IsServerAvailable(host)){
					NetworkMaster.Instance.ConnectToServer(host);
					infoText.text = "Connected.\nWaiting for more players...";
					return true;
				}
			}
		}
		return false;
	}


	IEnumerator RetryToConnectToAvailableServer(){
		yield return new WaitForSeconds(3.0f);
		if (!TryToConnectToAvailableServer()){
			// Failed to connect, create new server
			CreateNewServer();
		}
	}



	void OnDestroy(){
		// unregister connection delegates
//		NetworkCenter.Instance.playerConnectedEvent -= OnPlayerConnectionConfirmed;
//		NetworkCenter.Instance.playerDisconnectedEvent -= OnPlayerDisconnectionConfirmed;
	}



	// Note: all this stuff should use the callbacks...


	void UpdateConnectionStatus(){

		List<string> readyPlayers = NetworkSync.Instance.GetReadyPlayerGuids();
		int numPlayersReady = readyPlayers == null ? 0 : readyPlayers.Count;

		// Automatically set clients ready
		if (Network.isClient && !NetworkSync.Instance.IsPlayerReady()){
			NetworkSync.Instance.SetReady(true);
		}

		// Show button for server to start game
		if (Network.isServer){
			goButton.SetActive(numPlayersReady > 0);
		}

		if (numPlayersReady > 0){
			float lagTime = NetworkSync.Instance.GetLagTime();
			uint framesLagged = (uint) Mathf.CeilToInt(NetworkSync.lagCompensationRate * lagTime / StateManager.Instance.UpdateRate);
			string newText = Network.isClient ? "Waiting for server to start\n" : "Press GO to start!!\n";
			newText += "#Connections: " + (numPlayersReady + 1) + "\ntt: " + (int)(lagTime * 1000) + "ms, frames: " + framesLagged;
			infoText.text = newText;
		}else {
			infoText.text = Network.isClient ? "Connected.\nWaiting for more players..." : "Created New Server.\nWaiting for players";
		}
	}

	void Update(){

		if (NetworkMaster.Instance.IsAnouncingServer || NetworkCenter.Instance.IsConnected()){
			UpdateConnectionStatus();
		}

		if (NetworkSync.Instance.IsEveryoneReady()) {
			// game starting, dismiss GUI
			this.enabled = false;
			mainParent.SetActive(false);
			inGameParent.SetActive(true);
		}

	}


	public void OnServerReadyButtonPressed(){
		NetworkSync.Instance.SetReady(true);
	}



//	void OnPlayerConnectionConfirmed(string guid) {
//		Debug.Log("On Player Connection Confirmed");
//		if (guid == Network.player.guid) {
//			enabled = false;
//		}
//	}
//	
//	
//	void OnPlayerDisconnectionConfirmed(string guid) {
//		Debug.Log("On Player Disconnection Confirmed");
//		if (guid == Network.player.guid) {
//			enabled = true;
//		}
//	}

}

