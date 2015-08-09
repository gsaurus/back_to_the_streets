
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
		public FixedVector3 GetInputAxis(Model model){
			PlayerInputModel playerModel = model as PlayerInputModel;
			if (playerModel == null) return FixedVector3.Zero;
			return playerModel.axis[0];
		}

		// Button pressed
		public bool IsButtonPressed(Model model, uint buttonId){
			PlayerInputModel playerModel = model as PlayerInputModel;
			if (playerModel == null) return false;
			return playerModel.actionPressedCoolers[buttonId] > 0;
		}

		// Button hold
		public bool IsButtonHold(Model model, uint buttonId){
			PlayerInputModel playerModel = model as PlayerInputModel;
			if (playerModel == null) return false;
			return playerModel.actionsHold[buttonId];
		}

		// Button released
		public bool IsButtonReleased(Model model, uint buttonId){
			PlayerInputModel playerModel = model as PlayerInputModel;
			if (playerModel == null) return false;
			return playerModel.actionPressedCoolers[buttonId] > 0;
		}

		

		// Update coolers and input state based on input events
		protected override void Update(PlayerInputModel model){

			// Update action coolers
			for (int i = 0 ; i < model.actionPressedCoolers.Length ; ++i){
				if (model.actionPressedCoolers[i] > 0){
					--model.actionPressedCoolers[i];
				}
				if (model.actionReleasedCoolers[i] > 0){
					--model.actionReleasedCoolers[i];
				}
			}


			// Update model input state based on player events
			List<Event> playerEvents = StateManager.Instance.GetEventsForPlayer(model.playerId);
			if (playerEvents != null){
				ButtonInputEvent buttonEvent;
				AxisInputEvent axisEvent;
				foreach (Event e in playerEvents){
					axisEvent = e as AxisInputEvent;
					if (axisEvent != null){
						model.axis[axisEvent.axisId] = axisEvent.value;
					}else {
						buttonEvent = e as ButtonInputEvent;
						if (buttonEvent != null && buttonEvent.buttonId < model.actionPressedCoolers.Length){
							if (buttonEvent.isPressed && !model.actionsHold[buttonEvent.buttonId]){
								model.actionsHold[buttonEvent.buttonId] = true;
								model.actionPressedCoolers[buttonEvent.buttonId] = ActionPressedCoolerFrames;
							}else if (!buttonEvent.isPressed && model.actionsHold[buttonEvent.buttonId]){
								model.actionsHold[buttonEvent.buttonId] = false;
								model.actionReleasedCoolers[buttonEvent.buttonId] = ActionReleasedCoolerFrames;
							} // buttonEvent.isPressed
						} // buttonEvent != null
					} // axisEvent != null

				} // foreach event

			} // playerEvents != null

		} // Update


	} // PlayerInputController



}
