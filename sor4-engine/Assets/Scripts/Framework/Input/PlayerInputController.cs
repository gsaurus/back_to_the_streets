
using System;
using System.Collections.Generic;


// Check input events and traduce them into input state
public class PlayerInputController:Controller<PlayerInputModel>{

	public PlayerInputController(){

	}
	

	// Update
	public override void Update(PlayerInputModel model){

		// Update action coolers
		for (int i = 0 ; i < model.actionCoolers.Length ; ++i) {
			if (model.actionCoolers[i] > 0){
				--model.actionCoolers[i];
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
					model.axis = axisEvent.axis;
				}else {
					buttonEvent = e as ButtonInputEvent;
					if (buttonEvent != null){
						if (buttonEvent.isPressed && !model.actionsHolded[buttonEvent.button]){
							model.actionsHolded[buttonEvent.button] = true;
							model.actionCoolers[buttonEvent.button] = PlayerInputModel.ActionCoolerFrames;
						}else if (!buttonEvent.isPressed && model.actionsHolded[buttonEvent.button]){
							model.actionsHolded[buttonEvent.button] = false;
						} // buttonEvent.isPressed
					} // buttonEvent != null
				} // axisEvent != null

			} // foreach event

		} // playerEvents != null

	} // Update


} // PlayerInputController

