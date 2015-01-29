using UnityEngine;
using System.Collections;

// Game entity used as base for characters and other game elements
public class GameEntityModel: Model<GameEntityModel> {

	// Keep track of physics and animation models
	public uint physicsModelId;
	public uint animationModelId;

	public bool isFacingRight;


	protected override View<GameEntityModel> CreateView(){
		return new GameEntityView();
	}
	
	
	protected override Controller<GameEntityModel> CreateController(){
		return new GameEntityController();
	}
	
	public override void OnDestroy(){
		// Cleanup dependant states
		Model model = StateManager.state.GetModel(physicsModelId);
		if (model != null){
			StateManager.state.RemoveModel(model);
		}
		model = StateManager.state.GetModel(animationModelId);
		if (model != null){
			StateManager.state.RemoveModel(model);
		}
	}

}
