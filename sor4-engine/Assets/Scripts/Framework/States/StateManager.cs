using System;
using System.Collections.Generic;
using RetroBread.Network;


namespace RetroBread{


	// Used to initialize StateManager
	public class StateManagerSetup{
		public Model initialModel;			// first model of the game
		public float updateRate;			// logics update in frames per second
		public uint saveStateFrequency;		// how often to save state, in frames
		public bool isNetworked;
		// TODO: other setup options?

		public StateManagerSetup(Model initialModel, bool isNetworked = false){
			this.initialModel = initialModel;
			updateRate = 0.0166666667f;	// default: 60fps
			saveStateFrequency = 3;	// default: clone every 3 frames
			this.isNetworked = isNetworked;
		}
	}


	// StateManager is the intermediate between the game state, and all other
	// entities that interact with it - such as network, states history, input and event loggers
	public sealed class StateManager{

		#region Singleton
		private static readonly StateManager instance = new StateManager();
		public static StateManager Instance { get{ return instance; } }
		public static State state { get{ return instance.currentState; } }
		private StateManager(){}
		#endregion

		// If received input keyframe differs this number of frames from current keyframe, sync time
		private const int clockSinkToleranceFrames = 2; 
		// how much clock is sinked on each frame
		private const float clockSinkPercentagePerFrame = 0.025f;

		// Buffers are typically used on Netplay only
		private StatesBuffer statesBuffer;
		private EventsBuffer eventsBuffer;

		private uint saveStateFrequency; 		// every how many frames the state should be cached?
		private uint latestKeyframeBuffered;	// what was the keyframe of the last cached state?

		// TODO: Loggers in/out

		// The current game state
		private InternalState currentState;

		// logics update frequency
		public float UpdateRate { get; private set; }
		// how much remaining time is kept for next update?
		private float latestUpdateDeltatimeRemainder;	
		// extra time added to update to sync clocks smootly
		private float clockSynkTime;

		private StateManagerSetup delayedSetup;

		// Control if controllers are updated, or only views
		public bool IsPaused { get; private set; }

		public bool IsNetworked { get ; private set; }


		// Initialize with stuff
		// TODO: what to setup here? Networked game or not, loggers, properties etc..
		public void Setup(StateManagerSetup setup){

			if (currentState != null && currentState.IsUpdating){
				// can't stop at the middle of an update
				delayedSetup = setup;
			}else {
				LateSetup(setup);
			}
		}

		private void LateSetup(StateManagerSetup setup){

			// TODO: init state with proper seed, either random or agreed with network
			if (this.currentState != null){
				currentState.Destroy();
			}
			if (setup == null){
				IsPaused = true;
				this.statesBuffer = null;
				this.eventsBuffer = null;
				return;
			}

			DefaultVCFactories.RegisterFactories();
			this.currentState = new InternalState(setup.initialModel, (long) new System.Random().Next(1, int.MaxValue));

			UpdateRate = setup.updateRate;
			this.latestUpdateDeltatimeRemainder = 0;

			// TODO: only create this if necessary, else set it to null
			this.statesBuffer = new StatesBufferBySerializing();
			//this.statesBuffer = new StatesBufferByCloning();
			this.eventsBuffer = new EventsBuffer();
			saveStateFrequency = setup.saveStateFrequency;
			latestKeyframeBuffered = 0;
			// buffer state zero imediately
			statesBuffer.SetState(currentState);

			// TODO: add listeners and pause only if on networked game
			IsNetworked = setup.isNetworked;
			if (IsNetworked) {
				NetworkGame.Instance.onEventsAddedEvent += OnEventsAdded;
				NetworkGame.Instance.onPauseEvent += OnPause;
				NetworkGame.Instance.onResumeEvent += OnResume;
				NetworkGame.Instance.stateCorrectionEvent += OnStateCorrection;
				IsPaused = true; // wait for server order to resume
			} else {
				// No need to wait for resume (synch)
				IsPaused = false;
			}
		}


		#region Game Events

		// Add an event to the game
		public void AddEvent(Event newEvent){
			if (state == null || IsPaused) {
				return;
			}
			// Setup events keyframe
			newEvent.Keyframe = state.Keyframe;

			if (IsNetworked){
				// Network will take care of adding lag compensation
				NetworkGame.Instance.AddEvent(newEvent);
			}else{
				eventsBuffer.AddEvent(newEvent);
			}
		}

		// When events arrive from the network we add them to the events buffer
		private void OnEventsAdded(List<Event> newEvents){
			uint oldestKeyframe = uint.MaxValue;
			foreach (Event e in newEvents){
				Debug.Log("Event applied for player " + e.PlayerId);
				eventsBuffer.AddEvent(e);
				if (e.Keyframe < oldestKeyframe){
					oldestKeyframe = e.Keyframe;
				}
			}
			// if frames are considerably different, delay or advance game time
			if (NetworkGame.Instance.enabled){
				uint lagFrames = NetworkGame.Instance.GetLagFrames();
				int frameDifference = (int)(oldestKeyframe - state.Keyframe);
				if (frameDifference < -clockSinkToleranceFrames || frameDifference > lagFrames + clockSinkToleranceFrames){
					clockSynkTime = frameDifference*UpdateRate;
					//UnityEngine.Debug.Log("remainder: " + frameDifference);
				}
			}
		}


		// Get event for a given player in the current state keyframe
		public List<Event> GetEventsForPlayer(uint playerId){
			return eventsBuffer.GetEvents(state.Keyframe, playerId);
		}


		#endregion

		#region Pause / Resume

		// Manually pause the game (to load resources for instance)
		public void SetPaused(bool paused){
			// For now we just pause right away.
			// When server resumes the game we may notice a state jump
			// TODO: improve it if necessary
			IsPaused = paused;
			if (IsNetworked) {
				NetworkSync.Instance.SetReady(!paused);
			}
		}

		// Game pause requested from network for instance
		private void OnPause(){
			// For now we just pause and don't adjust the current state
			// from the original pause momment, so we may notice a state jump on resume
			// TODO: improve it if necessary
			IsPaused = true;
		}

		// Game resumed from server, with new state
		private void OnResume(State newState, State oldestState, float timePassedSinceResume){
			UnityEngine.Debug.Log("Resume game, time: " + timePassedSinceResume);
			// When the game is resumed we override our state to the one from server
			OverrideState(newState, oldestState);

			if (timePassedSinceResume > 0){
				// We are already late! update game to get in sync
				UpdateLogics(timePassedSinceResume);
			}
			IsPaused = false;
		}

		// Game state changed, from server
		private void OverrideState(State newState, State oldestState = null){

			// override current state
			InternalState newCurrentState = newState as InternalState;

			// reuse as much as possible
			newCurrentState.ReuseVCFromOtherState(currentState);
			currentState = newCurrentState;

			// Reset states buffer with the sent states
			statesBuffer.Clear();
			if (oldestState != null){
				statesBuffer.SetState(oldestState);
			}
			statesBuffer.SetState(currentState);
			latestKeyframeBuffered = currentState.Keyframe;
		}


		// Game state changed from server
		private void OnStateCorrection(State newState){

			// override current state
			InternalState newCurrentState = newState as InternalState;

			// reuse as much as possible
			newCurrentState.ReuseVCFromOtherState(currentState);

			uint currentKeyframe = currentState.Keyframe;
			currentState = newCurrentState;
			statesBuffer.SetState(currentState);

			// redo game state since then
			while (currentState.Keyframe < currentKeyframe){

			}
		}


		#endregion



		#region Update Loop


		// Single logics tick
		private void UpdateLogicsTick(){
			currentState.UpdateControllers();
			// Buffer state every now and then
			if (currentState.Keyframe >= latestKeyframeBuffered + saveStateFrequency){
				statesBuffer.SetState(currentState);
				latestKeyframeBuffered = currentState.Keyframe;
			}
		}


		// logics update cycles
		private void UpdateLogics(float deltaTime){

			// Do as many cycles as possible, and store the remainder deltatime for next time
			deltaTime += latestUpdateDeltatimeRemainder;
			if (clockSynkTime != 0){
				float synkTimeFraction = clockSynkTime * clockSinkPercentagePerFrame;
				clockSynkTime -= synkTimeFraction;
				deltaTime += synkTimeFraction;
			}
			while (deltaTime > UpdateRate) {
				deltaTime -= UpdateRate;
				UpdateLogicsTick();
			}
			latestUpdateDeltatimeRemainder = deltaTime;
		}


		// Rewind and remake game state if there are old uncaught events
		private void RewindIfNecessary(){
			uint oldestEventKeyframe = eventsBuffer.GetOldestEventKeyframeSinceLastCheck();
			uint presentKeyframe = currentState.Keyframe;
			if (oldestEventKeyframe < presentKeyframe){
				//			UnityEngine.Debug.Log("Need to rewind " + (presentKeyframe - oldestEventKeyframe) + "frames");
				if (!LoadBufferedState(oldestEventKeyframe, false)) {
					UnityEngine.Debug.LogWarning("Can't rewind, no buffered state is old enough");
					// TODO: ask network server for a state update
					return;
				}
				// Old state restored, now redo the present state
				while (currentState.Keyframe < presentKeyframe){
					UpdateLogicsTick();
				}
			}
		}



		// visual update
		private void UpdateVisuals(float deltaTime){
			currentState.UpdateViews(deltaTime);
		}


		// Update loop, including logics and visualization update
		// It also automatically corrects the state when older input is detected
		public void Update(float deltaTime){

			if (currentState == null) {
				// Not yet initialized
				return;
			}

			// Flush network events
			if (IsNetworked){
				NetworkGame.Instance.FlushEvents();
			}

			if (!IsPaused) {

				// rewind the game state due to uncatched old events, if any
				RewindIfNecessary();

				// logics update cycles
				UpdateLogics(deltaTime);

			}

			// If the game was requested to restart during the previous update, do it now
			if (delayedSetup != null){
				LateSetup(delayedSetup);
				delayedSetup = null;
				return;
			}

			if (currentState.Keyframe > 0) {
				// temporarily set back to current frame
				// because views may want to access (last) current keyframe
				currentState.Keyframe--;

				// visual update
				UpdateVisuals(deltaTime);

				// In the end we move to next frame again
				currentState.Keyframe++;
			}else {
				// no updates took place yet. Still update visuals (perhaps a loading screen going on?)
				UpdateVisuals(deltaTime);
			}

		}


		#endregion



		#region Save / Load buffered states

		// Force buffering the current state
		public uint SaveBufferedState(){
			if (state.IsUpdating) return 0;
			statesBuffer.SetState(currentState);
			latestKeyframeBuffered = currentState.Keyframe;
			return latestKeyframeBuffered;
		}

		// Override current state with the buffered state at the given keyframe
		// If there is no buffered state at that frame, use the highest below it and
		// redo the state until the requested momment
		public bool LoadBufferedState(uint keyframe, bool discardNewerEvents = true){

			State latestState = statesBuffer.GetLatestState(keyframe);
			if (latestState == null) return false; // there is no buffered states before the requested keyframe

			// Discard newer, deprecated state
			statesBuffer.DiscardNewerStates(latestState.Keyframe);
			latestKeyframeBuffered = latestState.Keyframe;

			// override current state
			InternalState newCurrentState = latestState as InternalState;

			// reuse as much as possible
			newCurrentState.ReuseVCFromOtherState(currentState);
			currentState = newCurrentState;

			// restore any missing progress until the requested momment
			while (currentState.Keyframe < keyframe){
				UpdateLogicsTick();
			}

			if (discardNewerEvents){
				if (keyframe > 0) {
					eventsBuffer.DiscardNewerEvents(keyframe-1);
				}else {
					eventsBuffer.Clear();
				}
			}

			return true;
		}


		public State GetOldestBufferedState(){
			return statesBuffer.GetOldestState();
		}

		#endregion

	}


}

