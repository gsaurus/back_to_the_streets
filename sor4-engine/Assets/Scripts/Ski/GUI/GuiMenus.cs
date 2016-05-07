using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;

public class GuiMenus : MonoBehaviour
{
	enum MenuState {
		enterNickname,
		matchmaking,
		inGame
	};

	public static string defaultNickname = "Guest_";

	private static float canvasFadeTime = 0.3f;
	private static float connectionFadeTime = 0.25f;


	// waiting time for more players, in seconds
	private static float minWaitingTimeInOnline = 5.0f;
	private static float maxWaitingTimeInOnline = 15.0f;
	// On offline mode, don't wait that long
	private static float minWaitingTimeInOffline = 2.5f;
	private static float maxWaitingTimeInOffline = 5.0f;


	public GameObject background;
	public GameObject nicknameParent;
	public GameObject matchmakingParent;
	public GameObject inGameParent;

	public GameObject matchmakingProgressParent;

	// controls menu flow
	private MenuState menuState = MenuState.enterNickname;

	// controls matchmaking progress bar
	private int matchMakingProgress;
	// fake matchmaking progress intervals
	private float[] matchMakingIntervals;
	private float matchmakingStartTime;

	private string nickname;


	public void OnNicknameChange(string newNickname){
		nickname = newNickname;
	}


	void OnEnable(){
		AudioListener.volume = 0;
		NetworkMaster.Instance.RefreshServersList();
	}

	public void StartMatchmaking(){
		// Setup player data
		if (string.IsNullOrEmpty(nickname)) {
			nickname = defaultNickname + UnityEngine.Random.Range(0, int.MaxValue);
		}
		NetworkPlayerData playerData = new NetworkPlayerData(SystemInfo.deviceUniqueIdentifier + "::" + UnityEngine.Random.Range(0, int.MaxValue), nickname);
		NetworkCenter.Instance.SetPlayerData(playerData);

		this.enabled = true;
		FadeToState(MenuState.matchmaking);

		// Try to connect to a server
		bool connected = TryToConnectToAvailableServer();
		if (connected) {
			PostStartMatchmaking(true);
		} else {
			StartCoroutine(DelayedStartMatchmaking());
		}

	}

	IEnumerator DelayedStartMatchmaking(){
		NetworkMaster.Instance.RefreshServersList();
		yield return new WaitForSeconds(1.5f);
		bool connected = TryToConnectToAvailableServer() || TryToCreateNewServer();
		PostStartMatchmaking(connected);
	}


	void PostStartMatchmaking(bool connected){

		// Setup player data
		if (string.IsNullOrEmpty(nickname)) {
			nickname = defaultNickname + UnityEngine.Random.Range(0, int.MaxValue);
		}
		NetworkPlayerData playerData = new NetworkPlayerData(SystemInfo.deviceUniqueIdentifier, nickname);
		NetworkCenter.Instance.SetPlayerData(playerData);

		// Try to connect to a server, or to create a new server
		// Setup game with connection status
		SetupGame(connected);

		// start fake matchmaking 
		matchMakingProgress = 1;
		// setup fake matchmaking
		float minTime = connected ? minWaitingTimeInOnline : minWaitingTimeInOffline;
		float maxTime = connected ? maxWaitingTimeInOnline : maxWaitingTimeInOffline;
		maxTime = UnityEngine.Random.Range(minTime, maxTime);
		matchMakingIntervals = new float[WorldModel.MaxPlayers-1];
		for (int i = 0; i < WorldModel.MaxPlayers-1; ++i) {
			matchMakingIntervals[i] = UnityEngine.Random.Range(0.5f, maxTime);
		}
		Array.Sort(matchMakingIntervals);
		matchmakingStartTime = Time.unscaledTime;
	
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


	bool TryToConnectToAvailableServer(){
		List<HostData> hosts = NetworkMaster.Instance.hosts;
		int errorRetries = 4;
		if (hosts != null){
			foreach (HostData host in hosts) {
				if (NetworkMaster.IsServerAvailable(host)){
					NetworkConnectionError error = NetworkMaster.Instance.ConnectToServer(host);
					if (error == NetworkConnectionError.NoError) {
						RetroBread.Debug.Log("Connected.\nWaiting for players...");
						return true;
					} else {
						RetroBread.Debug.LogWarning("Failed to connect to server with error: " + error);
						if (--errorRetries == 0) {
							return false;
						}
					}
				}
			}
		}
		return false;
	}
		



	void UpdateConnectionStatus(){

		List<string> readyPlayers = NetworkSync.Instance.GetReadyPlayerGuids();
		int numPlayersReady = readyPlayers == null ? 0 : readyPlayers.Count;
		matchMakingProgress = Math.Max(matchMakingProgress, numPlayersReady + 1);

		// Automatically set clients ready
		if (!NetworkSync.Instance.IsPlayerReady ()) {
			if (Network.isClient) {
				// clients automatically set ready
				NetworkSync.Instance.SetReady(true);
			} else {
				// If server, set ready if there are enough players, or if enough time passed
				if (matchMakingProgress >= WorldModel.MaxPlayers) {
					if (numPlayersReady > 0) {
						NetworkSync.Instance.SetReady(true);
					}
				}
			}
		}
			
	}

	void Update(){

		if (menuState == MenuState.matchmaking) {
			if (NetworkMaster.Instance.IsAnouncingServer || NetworkCenter.Instance.IsConnected()) {
				UpdateConnectionStatus();
			}

			UpdateMatchmakingProgress();

			if (NetworkSync.Instance.IsEveryoneReady() || (!NetworkCenter.Instance.IsConnected() && matchMakingProgress >= WorldModel.MaxPlayers)) {

				if (NetworkMaster.Instance.IsAnouncingServer) {
					List<string> readyPlayers = NetworkSync.Instance.GetReadyPlayerGuids();
					if (readyPlayers == null || readyPlayers.Count == 0) {
						// only me, let's go offline
						NetworkMaster.Instance.CancelServer();
						SetupGame(false);
					}
				}
				// game starting
				FadeToState(MenuState.inGame);
				StartCoroutine(DelayedEnable(false, canvasFadeTime));
			}
		
		}

	}


	private void UpdateMatchmakingProgress(){
		if (matchMakingIntervals == null) return;
		if (matchMakingProgress <= matchMakingIntervals.Length) {
			float deltaTime = Time.unscaledTime - matchmakingStartTime;
			if (deltaTime > matchMakingIntervals[matchMakingProgress-1]) {
				++matchMakingProgress;
			}
		}
		UnityEngine.UI.Image image;
		Color disabledColor = new Color(0.5f, 0.5f, 0.5f);
		Color enabledColor = new Color(0.25f, 1.0f, 0.25f);
		Color color;
		for (int i = 0 ; i < matchmakingProgressParent.transform.childCount ; ++i) {
			image = matchmakingProgressParent.transform.GetChild(i).gameObject.GetComponent<UnityEngine.UI.Image>();
			if (image != null) {
				color = i < matchMakingProgress ? enabledColor : disabledColor;
				StartCoroutine(FadeImageColorTo(image, color, connectionFadeTime));
			}
		}
	}



	void FadeToState(MenuState newState){
		if (menuState == newState) return;
		switch (newState) {
			case MenuState.inGame:{
				StartCoroutine(FadeCanvasTo(background, 0.0f, canvasFadeTime));
				StartCoroutine(FadeCanvasTo(nicknameParent, 0.0f, canvasFadeTime)); 
				StartCoroutine(FadeCanvasTo(matchmakingParent, 0.0f, canvasFadeTime)); 
				StartCoroutine(FadeCanvasTo(inGameParent, 1.0f, canvasFadeTime));
			}break;
			case MenuState.enterNickname:{
				StartCoroutine(FadeCanvasTo(background, 1.0f, canvasFadeTime));
				StartCoroutine(FadeCanvasTo(nicknameParent, 1.0f, canvasFadeTime)); 
				StartCoroutine(FadeCanvasTo(matchmakingParent, 0.0f, canvasFadeTime)); 
				StartCoroutine(FadeCanvasTo(inGameParent, 0.0f, canvasFadeTime));
			}break;
			case MenuState.matchmaking:{
				StartCoroutine(FadeCanvasTo(background, 1.0f, canvasFadeTime));
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
			obj.SetActive (true);
		}
		CanvasGroup canvas = obj.GetComponent<CanvasGroup>();
		if (canvas == null || Math.Abs(canvas.alpha - aValue) < 0.05f) yield break;
		float alpha = canvas.alpha;
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime){
			canvas.alpha = Mathf.Lerp(alpha, aValue, t);
			yield return null;
		}
		canvas.alpha = aValue;
		if (aValue < 0.25f) {
			obj.SetActive (false);
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

