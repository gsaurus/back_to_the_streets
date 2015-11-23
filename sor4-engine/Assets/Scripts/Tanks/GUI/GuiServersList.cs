using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;

public class GuiServersList : MonoBehaviour
{

	private Vector2 scrollPosition;

	private bool selectingServerOptions;

	private uint levelId = 1;
	private uint tankEnergy = 1;
	private uint bulletEnery = 1;
	private uint numBullets = 2;
	private float tankVel = 0.04f;
	private float bulletVel = 0.12f;
	private float tankRotation = 0.07f;
	private float turretRotation = 0.06f;
	
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
		selectingServerOptions= false;
//		NetworkCenter.Instance.playersLocked = true;
//		NetworkCenter.Instance.playerConnectedEvent += OnPlayerConnectionConfirmed;
//		NetworkCenter.Instance.playerDisconnectedEvent += OnPlayerDisconnectionConfirmed;
	}

	void OnDestroy(){
		// unregister connection delegates
//		NetworkCenter.Instance.playerConnectedEvent -= OnPlayerConnectionConfirmed;
//		NetworkCenter.Instance.playerDisconnectedEvent -= OnPlayerDisconnectionConfirmed;
	}



	int[] GetMapForId(uint mapId){

		int[] res = null;

		switch (mapId){

			case 2:
				res = new int[]{
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0,
					0, 9, 0, 9, 0, 9, 1, 9, 0, 9, 0, 9, 0,
					0, 9, 0, 9, 0, 0, 0, 0, 0, 9, 0, 9, 0,
					0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					1, 0, 9, 9, 0, 9, 0, 9, 0, 9, 9, 0, 1,
					0, 0, 0, 0, 0, 9, 9, 9, 0, 0, 0, 0, 0,
					0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0,
					0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0,
					0, 9, 0, 9, 0, 0, 0, 0, 0, 9, 0, 9, 0,
					0, 9, 0, 9, 0, 9, 9, 9, 0, 9, 0, 9, 0,
					0, 0, 0, 0, 0, 9, 0, 9, 0, 0, 0, 0, 0
				};
				break;
			case 3:
				res = new int[]{
					0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0,
					0, 9, 0, 1, 0, 0, 0, 9, 0, 9, 0, 9, 0,
					0, 9, 0, 0, 0, 0, 9, 9, 0, 9, 1, 9, 0,
					0, 0, 0, 9, 0, 0, 0, 0, 0, 1, 0, 0, 0,
					0, 0, 0, 9, 0, 0, 1, 0, 0, 9, 3, 9, 1,
					0, 0, 0, 0, 0, 9, 0, 0, 1, 0, 3, 0, 0,
					0, 9, 9, 9, 3, 3, 3, 1, 0, 0, 3, 9, 0,
					0, 0, 0, 1, 3, 9, 0, 9, 0, 9, 0, 9, 0,
					1, 9, 0, 1, 0, 9, 0, 9, 0, 0, 0, 9, 0,
					0, 9, 0, 9, 0, 9, 9, 9, 0, 9, 1, 9, 0,
					0, 9, 0, 9, 0, 9, 9, 9, 0, 0, 0, 0, 0,
					0, 9, 0, 0, 0, 0, 0, 0, 0, 9, 0, 9, 0,
					0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 9, 9, 0
				};
				break;
			case 4:
				res = new int[]{
					0, 0, 9, 0, 0, 9, 0, 9, 0, 9, 0, 0, 0,
					3, 9, 9, 9, 0, 9, 0, 1, 0, 9, 9, 0, 0,
					3, 3, 3, 0, 0, 9, 0, 9, 0, 9, 0, 9, 0,
					3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2,
					0, 9, 0, 0, 0, 9, 9, 9, 0, 0, 0, 0, 0,
					0, 0, 9, 0, 0, 9, 9, 9, 9, 9, 9, 1, 1,
					9, 9, 0, 9, 0, 9, 9, 9, 3, 9, 1, 1, 9,
					0, 0, 0, 1, 0, 1, 0, 3, 3, 3, 3, 0, 0,
					2, 2, 0, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2,
					3, 3, 0, 9, 0, 0, 9, 9, 0, 0, 0, 0, 0,
					3, 3, 9, 0, 9, 0, 0, 9, 0, 1, 9, 9, 0,
					3, 1, 9, 0, 9, 0, 0, 0, 0, 9, 0, 9, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, 0
				};
				break;
			case 5:
				res = new int[]{
					0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0,
					0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0,
					0, 0, 1, 0, 0, 0, 3, 0, 1, 1, 1, 0, 0,
					0, 1, 0, 0, 0, 3, 1, 0, 0, 0, 1, 0, 0,
					0, 0, 0, 0, 3, 1, 1, 0, 0, 0, 1, 1, 0,
					0, 1, 0, 3, 1, 1, 1, 0, 1, 0, 0, 0, 0,
					0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0,
					1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0,
					0, 0, 1, 0, 0, 0, 1, 1, 3, 0, 0, 1, 0,
					0, 1, 0, 0, 0, 0, 1, 3, 0, 0, 1, 1, 0,
					0, 1, 1, 1, 0, 0, 3, 0, 0, 1, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1,
					1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
				};
				break;
			case 1:
				res =  new int[]{
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 9, 9, 0, 0, 3, 3, 3, 0, 2, 0, 0, 0,
					0, 9, 9, 0, 0, 3, 3, 3, 0, 0, 0, 0, 0,
					0, 9, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 9, 9, 1, 1, 2, 0, 2, 3, 1, 3, 3, 3,
					0, 9, 9, 9, 1, 2, 0, 2, 3, 3, 3, 3, 3,
					0, 9, 9, 1, 1, 2, 2, 2, 0, 0, 0, 3, 3,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, 3,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, 9,
					9, 2, 2, 0, 0, 0, 0, 1, 1, 0, 0, 9, 9,
					0, 0, 2, 9, 0, 0, 0, 1, 1, 0, 0, 2, 2,
					0, 0, 9, 9, 0, 3, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 9, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0
				};
				break;
			default:
				res = new int[]{
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
				};
			break;
		}
		int[] sortedRes = new int[WorldModel.MaxWidth * WorldModel.MaxHeight];
		for (int i = 0 ; i < WorldModel.MaxHeight ; ++i){
			for (int j = 0 ; j < WorldModel.MaxWidth ; ++j){
				sortedRes[i*WorldModel.MaxWidth + j] = res[(WorldModel.MaxHeight-1-i)*WorldModel.MaxWidth + j];
			}
		}
		return sortedRes;
	}


	void OnServerOptionsGUI(){
	
		GUILayout.BeginHorizontal(GUILayout.Width(150));
		GUILayout.Label("levelId: ");
		uint.TryParse(GUILayout.TextField(levelId + "", GUILayout.Width(50)), out levelId);
		GUILayout.EndHorizontal();
		GUILayout.Space(10);

		GUILayout.BeginHorizontal(GUILayout.Width(150));
		GUILayout.Label("tankEnergy: ");
		uint.TryParse(GUILayout.TextField(tankEnergy + "", GUILayout.Width(50)), out tankEnergy);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(GUILayout.Width(150));
		GUILayout.Label("bulletEnery: ");
		uint.TryParse(GUILayout.TextField(bulletEnery + "", GUILayout.Width(50)), out bulletEnery);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(GUILayout.Width(150));
		GUILayout.Label("numBullets: ");
		uint.TryParse(GUILayout.TextField(numBullets + "", GUILayout.Width(50)), out numBullets);
		GUILayout.EndHorizontal();
		GUILayout.Space(10);

		GUILayout.BeginHorizontal(GUILayout.Width(150));
		GUILayout.Label("tankVel: ");
		float.TryParse(GUILayout.TextField(tankVel + "", GUILayout.Width(50)), out tankVel);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(GUILayout.Width(150));
		GUILayout.Label("bulletVel: ");
		float.TryParse(GUILayout.TextField(bulletVel + "", GUILayout.Width(50)), out bulletVel);
		GUILayout.EndHorizontal();
		GUILayout.Space(10);

		GUILayout.BeginHorizontal(GUILayout.Width(150));
		GUILayout.Label("tankRotation: ");
		float.TryParse(GUILayout.TextField(tankRotation + "", GUILayout.Width(50)), out tankRotation);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(GUILayout.Width(150));
		GUILayout.Label("turretRotation: ");
		float.TryParse(GUILayout.TextField(turretRotation + "", GUILayout.Width(50)), out turretRotation);
		GUILayout.EndHorizontal();
		
		// Create server button
		if (GUILayout.Button("Create")){
			selectingServerOptions = false;

			// Setup game state
			int[] theMap = GetMapForId(levelId);

			WorldModel world = new WorldModel(
				theMap,
				tankEnergy,
				bulletEnery,
				numBullets,
				tankVel,
				bulletVel,
				tankRotation,
				turretRotation
			);
			
			StateManagerSetup setup = new StateManagerSetup(world);
			StateManager.Instance.Setup(setup);

			NetworkMaster.Instance.CreateServer(5);
		}

	}



	void OnGUI(){

		if (NetworkCenter.Instance.IsConnected() || NetworkMaster.Instance.IsAnouncingServer){
			return; // already connected, so forget this screen
		}

		if (selectingServerOptions){
			OnServerOptionsGUI();
			return;
		}

		List<HostData> hosts = NetworkMaster.Instance.hosts;
		NetworkPlayerData myPlayerData = NetworkCenter.Instance.GetPlayerData();

		if (myPlayerData == null) {
			RetroBread.Debug.Log("My player is null!!");
			return;
		}

		GUILayout.BeginHorizontal(GUILayout.Width(300));

		// Player name
		myPlayerData.playerName = GUILayout.TextField(myPlayerData.playerName, GUILayout.Width(120));

		// Create server button
		if (GUILayout.Button("Create New Server")){
			selectingServerOptions = true;
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

