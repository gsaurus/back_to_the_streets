using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;

public class GuiMenus : SingletonMonoBehaviour<GuiMenus>
{
	enum MenuState {
		enterNickname,
		matchmaking,
		inGame
	};
		

	private static float canvasFadeTime = 0.3f;

	private static float hostsDiscoveryTimeout = 2.5f;

	public GameObject nicknameParent;
	public GameObject matchmakingParent;
	public GameObject inGameParent;


	// controls menu flow
	private MenuState menuState = MenuState.enterNickname;



	void OnEnable(){
		NetworkGame.Instance.onResumeEvent += OnGameResume;
	}


	private void SetupPlayerData(){
		NetworkPlayerData playerData = new NetworkPlayerData(SystemInfo.deviceUniqueIdentifier + "::" + UnityEngine.Random.Range(0, int.MaxValue), "" + UnityEngine.Random.Range(0, 99999999));
		NetworkCenter.Instance.SetPlayerData(playerData);
	}

	public void StartMatchmaking(){
		
		SetupPlayerData();

		this.enabled = true;

		FadeToState(MenuState.matchmaking);

		// Try to connect to a server
		NetworkSync.Instance.SetReady(false);
		StartCoroutine(HostsDiscovery());

	}


	void OnHostFound(HostData host){
		if (!NetworkCenter.Instance.IsConnected() && !NetworkMaster.Instance.IsAnouncingServer && NetworkMaster.IsServerAvailable(host)){
			NetworkConnectionError error = NetworkMaster.Instance.ConnectToServer(host);
			if (error == NetworkConnectionError.NoError) {
				NetworkMaster.Instance.hostFoundEvent -= OnHostFound;
			}
		}
	}


	IEnumerator HostsDiscovery(){
		NetworkMaster.Instance.hostFoundEvent += OnHostFound;
		NetworkMaster.Instance.RefreshServersList();
		yield return new WaitForSeconds(hostsDiscoveryTimeout);

		bool connected = NetworkCenter.Instance.IsConnected();
		if (!connected) {
			NetworkMaster.Instance.hostFoundEvent -= OnHostFound;
			connected = TryToCreateNewServer();
		}
		StartGame(connected);
	}
		



	void SetupGame(bool networked){
		RetroBread.Debug.Log("Setup " + (networked ? "Online" : "Offline") + " Game");

		WorldModel world = new WorldModel();

		StateManagerSetup setup = new StateManagerSetup(world, networked);
		StateManager.Instance.Setup(setup);	
	}
	

	bool TryToCreateNewServer(){
		NetworkConnectionError error = NetworkMaster.Instance.CreateServer((int)WorldModel.MaxPlayers);
		if (error == NetworkConnectionError.NoError){
			RetroBread.Debug.Log("Created New Server.\nWaiting for players");
			return true;
		} else {
			RetroBread.Debug.LogWarning("Couldn't create new server, error: " + error);
			return false;
		}
	}
		

	void StartGame(bool connected){
		SetupGame(connected);
		// game starting
		if (connected) {
			NetworkSync.Instance.SetReady(true);
		}
		RetroBread.Debug.Log("Starting game");
		FadeToState(MenuState.inGame);
		StartCoroutine(DelayedEnable(false, canvasFadeTime));
	}


	void Update(){
		// Nothing atm
	}



	void FadeToState(MenuState newState){
		if (menuState == newState) return;
//		winnerObj.SetActive(false);
//		secondObj.SetActive(false);
//		loserObj.SetActive(false);
		Vector2 awayPosition = new Vector2(0, Screen.height*2);
		switch (newState) {
			case MenuState.inGame:{
				StartCoroutine(FadeCanvasTo(nicknameParent, 0.0f, canvasFadeTime)); 
				StartCoroutine(FadeCanvasTo(matchmakingParent, 0.0f, canvasFadeTime)); 
				StartCoroutine(FadeCanvasTo(inGameParent, 1.0f, canvasFadeTime));
			}break;
			case MenuState.enterNickname:{
				StartCoroutine(FadeCanvasTo(nicknameParent, 1.0f, canvasFadeTime)); 
				StartCoroutine(FadeCanvasTo(matchmakingParent, 0.0f, canvasFadeTime)); 
				StartCoroutine(FadeCanvasTo(inGameParent, 0.0f, canvasFadeTime));
			}break;
			case MenuState.matchmaking:{
				StartCoroutine(FadeCanvasTo(nicknameParent, 0.0f, canvasFadeTime)); 
				StartCoroutine(FadeCanvasTo(matchmakingParent, 1.0f, canvasFadeTime)); 
				StartCoroutine(FadeCanvasTo(inGameParent, 0.0f, canvasFadeTime));
			}break;
		}
		menuState = newState;
	}

	IEnumerator DelayedEnable(bool enable, float time){
		yield return new WaitForSeconds(time);
		this.enabled = enabled;
	}
		

	IEnumerator FadeCanvasTo(GameObject obj, float aValue, float aTime){
		if (aValue > 0.25f) {
			obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			obj.SetActive(true);
		}
		CanvasGroup canvas = obj.GetComponent<CanvasGroup>();
		if (canvas != null && Math.Abs(canvas.alpha - aValue) > 0.05f){
			float alpha = canvas.alpha;
			for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime){
				canvas.alpha = Mathf.Lerp(alpha, aValue, t);
				yield return null;
			}
			canvas.alpha = aValue;
		}
		if (aValue < 0.25f) {
			obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Screen.height*2);
			//obj.SetActive(false);
		}
	}

	IEnumerator FadeImageColorTo(UnityEngine.UI.Image img, Color color, float aTime){
		Color originalColor = img.color;
		if (originalColor.Equals (color)) yield break;
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime){
			img.color = Color.Lerp(originalColor, color, t);
			yield return null;
		}
		img.color = color;
	}





	void OnDisable(){
		
		if (NetworkCenter.Instance.IsConnected()) {
			// Some logging..	
			List<string> readyPlayers = NetworkSync.Instance.GetReadyPlayerGuids();
			int numPlayersReady = readyPlayers == null ? 0 : readyPlayers.Count;
			float lagTime = NetworkSync.Instance.GetLagTime();
			uint framesLagged = (uint)Mathf.CeilToInt(NetworkSync.lagCompensationRate * lagTime / StateManager.Instance.UpdateRate);
			string infoText = "#Connections: " + (numPlayersReady + 1) + "\ntt: " + (int)(lagTime * 1000) + "ms, frames: " + framesLagged;
			RetroBread.Debug.Log(infoText);
		}

		NetworkGame.Instance.onResumeEvent -= OnGameResume;

	}




	private void OnGameResume(State newState, State oldestState, float timeToStart){
		// game starting
		FadeToState(MenuState.inGame);
		StartCoroutine(DelayedEnable(false, canvasFadeTime));
	}



	public void GameOver(){
		enabled = true;
	}


	IEnumerator RestartGameAfterSeconds(float seconds){
		yield return new WaitForSeconds(seconds);
		NetworkCenter.Instance.Disconnect();
		//StateManager.Instance.Setup(null);
		SetupGame(false);
		FadeToState(MenuState.enterNickname);
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



//	void OnDestroy(){
//		// unregister connection delegates
//		NetworkCenter.Instance.playerConnectedEvent -= OnPlayerConnectionConfirmed;
//		NetworkCenter.Instance.playerDisconnectedEvent -= OnPlayerDisconnectionConfirmed;
//	}



// Note: stuff should use the callbacks...

}

