using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


// Controls the network flow of a networked game
public sealed class NetworkGame : SingletonMonoBehaviour<NetworkGame>{

	// If true, when a player is set to not ready, the game is paused
	// else, the game keep running as long as the server lives
	public bool pauseWhenPlayersNotReady = true;

	// Events when game events are added
	public delegate void OnEventsAddedDelegate(SerializableList<Event> events);
	public event OnEventsAddedDelegate onEventsAddedEvent;

	// Event for pause game
	public delegate void OnPause();
	public event OnPause onPauseEvent;

	// Event for unpause the game, with a delay to ensure synchronization
	public delegate void OnResume(State newState, State oldestState, float timeToStart);
	public event OnResume onResumeEvent;

	// TODO: state correction messages and events
	// Event for state correction
	public delegate void StateCorrection(State newState);
	public event StateCorrection stateCorrectionEvent;

	// We gatter input events here to flush out once per frame
	private SerializableList<Event> eventsBuffer = new SerializableList<Event>();
	
	// On awake, register delegates
	void Awake(){
		NetworkSync.Instance.playerReadyEvent += OnPlayerReadyEvent;
		NetworkSync.Instance.playerNotReadyEvent += OnPlayerNotReadyEvent;
		NetworkCenter.Instance.playerDisconnectedEvent += OnPlayerNotReadyEvent;
	}

	// On destroy, unregister delegates
	void OnDestroy(){
		NetworkSync.Instance.playerReadyEvent -= OnPlayerReadyEvent;
		NetworkSync.Instance.playerNotReadyEvent -= OnPlayerNotReadyEvent;
		NetworkCenter.Instance.playerDisconnectedEvent -= OnPlayerNotReadyEvent;
	}


#region Pause / Resume game


	// When a player gets ready, one of the following can happen
	// 1) game allow players to join on middle game: server send him current state
	// 2) game doesn't allow joining on middle game: if all ready, server send state to all
	void OnPlayerReadyEvent(string guid){
		if (Network.isServer && NetworkSync.Instance.IsPlayerReady()){
			State currentState = StateManager.state;
			State oldestState = StateManager.Instance.GetOldestBufferedState();

			if (pauseWhenPlayersNotReady){
				// if everyone is ready, we can resume the game
				if (NetworkSync.Instance.IsEveryoneReady()){
					SendResumeMessage(currentState, oldestState);
				}
			}else {
				if (guid == Network.player.guid) {
					// Server got ready, send resume to every ready client
					List<string> readyGuids = NetworkSync.Instance.GetReadyPlayerGuids();
					foreach (string readyGuid in readyGuids){
						SendResumeMessage(currentState, oldestState, readyGuid);
					}
				}else{
					// resume the game of the one that just went ready
					SendResumeMessage(currentState, oldestState, guid);
				}
			}
		}
	}


	// When a player isn't ready anymore, one of the following can happen
	// 1) game allow players to leave on middle game: nothing happens
	// 2) game doesn't allow leave on middle game: pause game to all until everyone is ready again
	void OnPlayerNotReadyEvent(string guid){
		if (pauseWhenPlayersNotReady){
			GetComponent<NetworkView>().RPC("PauseGame",RPCMode.All); 
		}
//		else {
//			NetworkPlayer player;
//			if (NetworkCenter.TryGetNetworkPlayerForGuid(guid, out player)){
//				networkView.RPC("PauseGame", player);
//			}
//		}
	}

	// Gather current state and oldest known state and send to client(s)
	void SendResumeMessage(State currentState, State oldestState, string targetGuid = null){

		byte[] currentStateData;
		byte[] oldestStateData;
		currentStateData = NetworkCenter.Instance.serializer.Serialize(currentState);
		oldestStateData = NetworkCenter.Instance.serializer.Serialize(oldestState);
		float timeToResume;
		if (targetGuid == null){
			// send to all
			timeToResume = NetworkSync.Instance.GetLagTime();
			GetComponent<NetworkView>().RPC("ResumeGame", RPCMode.All, currentStateData, oldestStateData, timeToResume);
		}else{
			NetworkPlayer player;
			if (NetworkCenter.TryGetNetworkPlayerForGuid(targetGuid, out player)){
				timeToResume = NetworkSync.Instance.GetLagTime(targetGuid);
				GetComponent<NetworkView>().RPC("ResumeGame", player, currentStateData, oldestStateData, timeToResume);
			}
		}
	}

	// Pause the game
	[RPC]
	void PauseGame(){
		if (onPauseEvent != null){
			onPauseEvent();
		}
	}

	// Receiving a resume request, we sync and make sure to resume at the expected time
	[RPC]
	void ResumeGame(byte[] newStateData, byte[] oldestStateData, float timeToResume, NetworkMessageInfo messageInfo){
		State newState = NetworkCenter.Instance.serializer.Deserialize(newStateData) as State;
		State oldestState = NetworkCenter.Instance.serializer.Deserialize(oldestStateData) as State;
		float travelTime = (float) (Network.time - messageInfo.timestamp);
		timeToResume -= travelTime;
		if (timeToResume > 0){
			// wait until we're ready to resume
			StartCoroutine(DelayedGameResume(newState, oldestState, timeToResume));
		}else {
			// we're already late, resume imediately
			if (onResumeEvent != null){
				onResumeEvent(newState,oldestState,-timeToResume);
			}
		}
	}

	// Wait until it's time to resume
	IEnumerator DelayedGameResume(State newState, State oldestState, float timeToWait){
		yield return new WaitForSeconds(timeToWait);
		if (onResumeEvent != null){
			onResumeEvent(newState,oldestState,0);
		}
	}



#endregion


#region Game Events

	// Apply lag compensation on the given events
	private void ApplyLagCompensationOnEvent(Event e){
		uint currentKeyframe = StateManager.state.Keyframe;
		float lagTime = NetworkSync.Instance.GetLagTime();
		uint laggedKeyframe = (uint) Math.Ceiling(NetworkSync.lagCompensationRate * lagTime / StateManager.Instance.UpdateRate);
		laggedKeyframe += currentKeyframe;
		if (e.Keyframe < laggedKeyframe){
			e.Keyframe = laggedKeyframe;
		}
	}

	// Set all events to be owned by current player number
	private void ApplyNetworkPlayerOnEvent(Event e){
		int playerNumber = NetworkCenter.Instance.GetPlayerNumber();
		if (playerNumber >= 0) {
			e.PlayerId = (uint)playerNumber;
		}
	}

	// Add events to be sent through the network
	public void AddEvent(Event e){
		ApplyNetworkPlayerOnEvent(e);
		ApplyLagCompensationOnEvent(e);
		eventsBuffer.Add(e);
	}

	public void FlushEvents(){
		if (eventsBuffer.Count > 0){
			byte[] eventsData = NetworkCenter.Instance.serializer.Serialize(eventsBuffer);
			GetComponent<NetworkView>().RPC("OnEventsAdded", RPCMode.All, eventsData);
			eventsBuffer.Clear();
		}
	}

	// When receiving game events from the network, we forward them to listeners
	[RPC]
	void OnEventsAdded(byte[] eventsData){
		if (onEventsAddedEvent != null){
			SerializableList<Event> events;
			events = NetworkCenter.Instance.serializer.Deserialize(eventsData) as SerializableList<Event>;
			if (events != null){
				onEventsAddedEvent(events);
			}
		}
	}
	
#endregion



}

