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

	public static string defaultNickname = "Guest_";

	private static float canvasFadeTime = 0.3f;
	private static float connectionFadeTime = 0.25f;


	// waiting time for more players, in seconds -- server
	private static float minWaitingTimeInOnline = 1.0f; //12.0f;
	private static float maxWaitingTimeInOnline = 1.0f; //18.0f;
	// waiting time for more players, in seconds -- client
	private static float maxWaitingTimeClient = 20.0f;
	// On offline mode, don't wait that long
	private static float minWaitingTimeInOffline = 2.5f;
	private static float maxWaitingTimeInOffline = 5.0f;

	private static float hostsDiscoveryTimeout = 2.25f;


	public GameObject background;
	public GameObject nicknameParent;
	public GameObject matchmakingParent;
	public GameObject inGameParent;

	public GameObject matchmakingProgressParent;

	public GameObject leaderboardObject;

	// controls menu flow
	private MenuState menuState = MenuState.enterNickname;

	// controls matchmaking progress bar
	private int matchMakingProgress;
	// fake matchmaking progress intervals
	private float[] matchMakingIntervals;
	private float matchmakingStartTime;

	public string nickname;

	private bool isMarkedToRestart;


	public void OnNicknameChange(string newNickname){
		nickname = newNickname;
	}


	public void ToggleVolume(bool value){
		AudioListener.volume = value ? 1 : 0;
	}


	void OnEnable(){
		NetworkGame.Instance.onResumeEvent += OnGameResume;
	}


	private void SetupPlayerData(){
		NetworkPlayerData playerData = new NetworkPlayerData(SystemInfo.deviceUniqueIdentifier + "::" + UnityEngine.Random.Range(0, int.MaxValue), nickname);
		NetworkCenter.Instance.SetPlayerData(playerData);
	}

	public void StartMatchmaking(){
		// Setup player data
		if (string.IsNullOrEmpty(nickname)) {
			nickname = defaultNickname + UnityEngine.Random.Range(0, 129999999);
		}
		SetupPlayerData();

		this.enabled = true;
		matchMakingProgress = 0;
		matchmakingStartTime = 0;
		matchMakingIntervals = null;
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
				PostStartMatchmaking(true);
			}
		}
	}


	IEnumerator HostsDiscovery(){
		NetworkMaster.Instance.hostFoundEvent += OnHostFound;
		NetworkMaster.Instance.RefreshServersList();
		yield return new WaitForSeconds(hostsDiscoveryTimeout);
		if (matchMakingProgress == 0) {
			NetworkMaster.Instance.hostFoundEvent -= OnHostFound;
			bool connected = TryToCreateNewServer();
			PostStartMatchmaking(connected);
		}
	}


	void PostStartMatchmaking(bool connected){

		// Try to connect to a server, or to create a new server
		// Setup game with connection status
		SetupGame(connected);

		// start fake matchmaking 
		matchMakingProgress = 1;
		// setup fake matchmaking
		float minTime;
		float maxTime;
		if (connected){
			if (UnityEngine.Network.isServer){
				minTime = minWaitingTimeInOnline;
				maxTime = maxWaitingTimeInOnline;
			}else{
				minTime = maxTime = maxWaitingTimeClient;
			}
		}else{
			minTime = minWaitingTimeInOffline;
			maxTime = maxWaitingTimeInOffline;
		}
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
		



	void UpdateConnectionStatus(){

		List<string> readyPlayers = NetworkSync.Instance.GetReadyPlayerGuids();
		int numPlayersReady = readyPlayers == null ? 0 : readyPlayers.Count;
		matchMakingProgress = Math.Max(matchMakingProgress, numPlayersReady + 1);

		// Automatically set clients ready
		if (!NetworkSync.Instance.IsPlayerReady()) {
			if (Network.isClient) {
				// clients automatically set ready
				NetworkSync.Instance.SetReady(true);
			} else {
				RetroBread.Debug.Log("looks like I'm a server");
				// If server, set ready if there are enough players, or if enough time passed
				if (matchMakingProgress >= WorldModel.MaxPlayers) {
					if (numPlayersReady > 0) {
						NetworkSync.Instance.SetReady(true);
					}
				}
			}
		}else{
			RetroBread.Debug.Log("already ready");
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
					NetworkMaster.Instance.CancelServer();
					if (readyPlayers == null || readyPlayers.Count == 0) {
						// only me, let's go offline
						SetupGame(false);
					}
				}
				if (!NetworkCenter.Instance.IsConnected() && StateManager.Instance.IsNetworked) {
					// Something went wrong while connecting, go offline
					RetroBread.Debug.LogWarning("Connection problems, going offline");
					NetworkCenter.Instance.Disconnect();
					SetupGame(false);
				}
				// game starting
				RetroBread.Debug.Log("Starting game" + (NetworkSync.Instance.IsEveryoneReady() ? " everyone is ready" : "but not everyone is ready"));
				FadeToState(MenuState.inGame);
				StartCoroutine(DelayedEnable(false, canvasFadeTime));
			}
		
		}else if (menuState == MenuState.inGame
			&& StateManager.Instance.IsNetworked
			&& (!NetworkCenter.Instance.IsConnected() || NetworkCenter.Instance.GetNumPlayersOnline() <= 1)
			&& StateManager.state != null && StateManager.state.Keyframe > 0
		){
			RetroBread.Debug.Log("No enough players connected, switching to offline mode");
			StateManager.Instance.SetOffline();

			State state = StateManager.state;
			int oldPlayerId = NetworkCenter.Instance.GetPlayerNumber();
			if (state == null || state.MainModel == null || oldPlayerId < 0) {
				SetupGame(false);
			} else {
				WorldModel world = state.MainModel as WorldModel;
				DebugWorldView view = world.View() as DebugWorldView;
				// switch own skier model
				if (world.skiers.Length > oldPlayerId) {
					SkierModel mySkier = world.skiers[oldPlayerId];
					SkierModel firstSkier = world.skiers[0];
					world.skiers[0] = mySkier;
					world.skiers[oldPlayerId] = firstSkier;
					PlayerInputModel playerInput = StateManager.state.GetModel(mySkier.inputModelRef) as PlayerInputModel;
					if (playerInput != null) {
						playerInput.playerId = 0;
					}
					playerInput = StateManager.state.GetModel(firstSkier.inputModelRef) as PlayerInputModel;
					if (playerInput != null) {
						playerInput.playerId = (uint)oldPlayerId;
					}

					// switch views
					if (view != null) {
						view.SwitchPlayers(oldPlayerId, 0);
					}
				}
			}
			NetworkCenter.Instance.Disconnect();
		}

		if (isMarkedToRestart) {
			isMarkedToRestart = false;
			StartCoroutine(RestartGameAfterSeconds(6.0f));
		}

	}


	private void UpdateMatchmakingProgress(){
		Color disabledColor = new Color(0.5f, 0.5f, 0.5f);
		Color enabledColor = new Color(0.25f, 1.0f, 0.25f);
		UnityEngine.UI.Image image;
		if (matchMakingIntervals == null){
			for (int i = 0 ; i < matchmakingProgressParent.transform.childCount ; ++i) {
				image = matchmakingProgressParent.transform.GetChild(i).gameObject.GetComponent<UnityEngine.UI.Image>();
				if (image != null) {
					image.color = disabledColor;
				}
			}
			return;
		}
		if (matchMakingProgress > 0 && matchMakingProgress <= matchMakingIntervals.Length) {
			float deltaTime = Time.unscaledTime - matchmakingStartTime;
			if (deltaTime > matchMakingIntervals[matchMakingProgress-1]) {
				++matchMakingProgress;
			}
		}
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
			obj.SetActive(false);
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



	public void MarkToRestart(){
		isMarkedToRestart = true;
		enabled = true;
	}


	IEnumerator RestartGameAfterSeconds(float seconds){
		yield return new WaitForSeconds(seconds);
		NetworkCenter.Instance.Disconnect();
		WorldObjects.Reset();
		StateManager.Instance.Setup(null);
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

