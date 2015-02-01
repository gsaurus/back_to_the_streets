using UnityEngine;
using System.Collections;

// Game entity used as base for characters and other game elements
public class GameEntityModel: Model<GameEntityModel> {

	// Physic model
	public uint physicsModelId;
	// Animation model
	public uint animationModelId;
	// Input model (we store input info when necessary, specially if controlled by AI)
	public uint inputModelId;

	// flag telling if the character is facing left or right
	public bool isFacingRight;


	protected override View<GameEntityModel> CreateView(){
		return new GameEntityView();
	}
	
	
	protected override Controller<GameEntityModel> CreateController(){
		return new GameEntityController();
	}
	
	// called when the game entity model is destroyed
	public override void OnDestroy(){
		// err.. don't cleanup dependant states, destroy may be called via resync,
		// the state may be out of the state manager
//		// Cleanup dependant states
//		StateManager.state.RemoveModel(physicsModelId);
//		StateManager.state.RemoveModel(animationModelId);
//		StateManager.state.RemoveModel(inputModelId);
	}

}
