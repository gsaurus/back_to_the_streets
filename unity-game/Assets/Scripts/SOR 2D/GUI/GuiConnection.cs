using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;

public class GuiConnection : MonoBehaviour
{
	public GameObject mainParent;
	public GameObject goButton;
	public GameObject offlineButton;
	public GameObject inGameParent;

	public UnityEngine.UI.Text infoText;

	private Vector2 scrollPosition;
	
	void Awake(){
	
		goButton.GetComponent<Button>().interactable = false;

		int randomId = Random.Range(0,int.MaxValue);
		NetworkCenter.Instance.SetPlayerData(new NetworkSorPlayerData(SystemInfo.deviceUniqueIdentifier + randomId, "guest_" + randomId, Random.Range(1,4)));
	
		infoText.text = "Looking for Servers...";

		// Search for servers
		if (!TryToConnectToAvailableServer()){
			// try a few seconds later
			StartCoroutine(RetryToConnectToAvailableServer());
		}
	}



	void SetupGame(bool networked){
		WorldModel world = new WorldModel();

		StateManagerSetup setup = new StateManagerSetup(world, networked);
		StateManager.Instance.Setup(setup);	
	}



	void CreateNewServer(){
		
		// Setup game state
		SetupGame(true);
		
		NetworkConnectionError error = NetworkMaster.Instance.CreateServer(6);
		if (error == NetworkConnectionError.NoError){
			infoText.text = "Created New Server.\nWaiting for players";
		} else {
			infoText.text = "Couldn't create new server, error: " + error;
		}
	}


	bool TryToConnectToAvailableServer(){
		List<HostData> hosts = NetworkMaster.Instance.hosts;
		if (hosts != null){
			foreach (HostData host in hosts) {
				if (NetworkMaster.IsServerAvailable(host)){
					SetupGame(true);
					NetworkMaster.Instance.ConnectToServer(host);
					infoText.text = "Connected.\nWaiting for more players...";
					UnityEngine.Debug.Log(infoText.text);
					return true;
				}
			}
		}
		return false;
	}


	IEnumerator RetryToConnectToAvailableServer(){
		for (int retries = 0 ; retries < 3 ; ++retries) {
			yield return new WaitForSeconds(1.0f);
			if (TryToConnectToAvailableServer()) {
				// success
				yield break;
			}
		}
		CreateNewServer();
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
			goButton.GetComponent<Button>().interactable = numPlayersReady > 0;
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

	public void OnOfflineModeButtonPressed(){

		// Cancel connection retries
		StopAllCoroutines();

		// Cancel communications..
		if (NetworkMaster.Instance.IsAnouncingServer){
			NetworkMaster.Instance.CancelServer();
		}

		NetworkCenter.Instance.Disconnect();

		// Start offline mode
		SetupGame(false);

		// game starting, dismiss GUI
		this.enabled = false;
		mainParent.SetActive(false);
		inGameParent.SetActive(true);
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

