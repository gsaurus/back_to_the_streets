using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;

public class GuiServersList : MonoBehaviour
{

	private Vector2 scrollPosition;

	void Awake(){
//		Debug.Log(FixedFloat.Create( 145103 >> FixedFloat.SHIFT_AMOUNT, false ));
//		Debug.Log(FixedFloat.Create( 599880 >> FixedFloat.SHIFT_AMOUNT, false ));
//		Debug.Log(FixedFloat.Create( 1420468 >> FixedFloat.SHIFT_AMOUNT, false ));
//		Debug.Log(FixedFloat.Create( 3592413 >> FixedFloat.SHIFT_AMOUNT, false ));
//		Debug.Log(FixedFloat.Create( 26353447 >> FixedFloat.SHIFT_AMOUNT, false ));
//		Debug.Log("------------------");
//		Debug.Log(FixedFloat.Create(0.008648812770843505859375));
//		Debug.Log(FixedFloat.Create(0.035755634307861328125));
//		Debug.Log(FixedFloat.Create(0.0846664905548095703125));
//		Debug.Log(FixedFloat.Create(0.214124500751495361328125));
//		Debug.Log(FixedFloat.Create(1.570787847042083740234375));
		int randomId = Random.Range(0,int.MaxValue);
		NetworkCenter.Instance.SetPlayerData(new NetworkPlayerData(SystemInfo.deviceUniqueIdentifier + randomId, "guest_" + randomId));
		NetworkCenter.Instance.playersLocked = true;
//		NetworkCenter.Instance.playerConnectedEvent += OnPlayerConnectionConfirmed;
//		NetworkCenter.Instance.playerDisconnectedEvent += OnPlayerDisconnectionConfirmed;
	}

	void OnDestroy(){
		// unregister connection delegates
//		NetworkCenter.Instance.playerConnectedEvent -= OnPlayerConnectionConfirmed;
//		NetworkCenter.Instance.playerDisconnectedEvent -= OnPlayerDisconnectionConfirmed;
	}

	void OnGUI(){

		if (NetworkCenter.Instance.IsConnected() || NetworkMaster.Instance.IsAnouncingServer){
			return; // already connected, so forget this screen
		}

		List<HostData> hosts = NetworkMaster.Instance.hosts;
		NetworkPlayerData myPlayerData = NetworkCenter.Instance.GetPlayerData();

		if (myPlayerData == null) {
			Debug.Log("My player is null!!");
			return;
		}

		GUILayout.BeginHorizontal(GUILayout.Width(300));

		// Player name
		myPlayerData.playerName = GUILayout.TextField(myPlayerData.playerName, GUILayout.Width(120));

		// Create server button
		if (GUILayout.Button("Create New Server")){
			NetworkMaster.Instance.CreateServer(5);
		}
		// Create refresh servers button
		if (GUILayout.Button("Refresh Master Servers List")){
			NetworkMaster.Instance.RefreshServersList();
		}
		GUILayout.EndHorizontal();

		// List of servers
		GUILayout.Space(25);
		scrollPosition = GUILayout.BeginScrollView(scrollPosition,
		                                           GUILayout.Width(300),
		                                           GUILayout.Height(Screen.height - 50)
		                                          );
		foreach (HostData host in hosts) {
			if (NetworkMaster.IsServerAvailable(host)){
				if (GUILayout.Button(host.gameName + " " + host.connectedPlayers + "/" + host.playerLimit)){
					NetworkMaster.Instance.ConnectToServer(host);
				}
			}
		}
		GUILayout.EndScrollView();


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

