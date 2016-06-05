
using System;
using System.Collections.Generic;



namespace RetroBread{


	// Check input events and traduce them into input state
	public class PlayerInputController:Controller<PlayerInputModel>, GameEntityInputProvider{

		private const int ActionPressedCoolerFrames = 12;
		private const int ActionReleasedCoolerFrames = 12;

		public PlayerInputController(){

		}
		
		// Axis
		public FixedFloat GetInputAxis(Model model){
			PlayerInputModel playerModel = model as PlayerInputModel;
			if (playerModel == null) return FixedFloat.Zero;
			return playerModel.axis;
		}

		// Update coolers and input state based on input events
		protected override void Update(State state, PlayerInputModel model){

			if (StateManager.mainState != state) return;
			// Update model input state based on player events
			List<Event> playerEvents = StateManager.Instance.GetEventsForPlayer(model.playerId);
			if (playerEvents != null){
				AxisInputEvent axisEvent;
				foreach (Event e in playerEvents){
					axisEvent = e as AxisInputEvent;
					if (axisEvent != null){
						model.axis = axisEvent.axis;
					} // axisEvent != null

				} // foreach event
				
			} // playerEvents != null

		} // Update


	} // PlayerInputController



}
