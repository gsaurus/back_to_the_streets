
using System;


// Handy static helper class, to get the input provider
public static class InputConditionsHelper{
	
	// Factor used to determine if X or Z movement is dominant over the overall axis control
	public static readonly FixedFloat AxisDominanceFactor = 0.475f;
	
	
	// Get input provider given the animation model
	public static Model GetInputModel(AnimationModel model){
		Model ownerModel = StateManager.state.GetModel(model.ownerId);
		if (ownerModel == null) return null;
		GameEntityModel entityModel = ownerModel as GameEntityModel;
		if (entityModel == null) return null;
		return StateManager.state.GetModel(entityModel.inputModelId);
	}
	
	// Get input axis given the animation model
	public static FixedVector3 GetInputAxis(AnimationModel model){
		Model inputModel = GetInputModel(model);
		if (inputModel == null) return FixedVector3.Zero;
		GameEntityInputProvider inputController = inputModel.Controller() as GameEntityInputProvider;
		if (inputController == null) return FixedVector3.Zero;
		return inputController.GetInputAxis(inputModel);
	}
	
	// Tell if the owner is facing right, given the animation model
	public static bool IsOwnerFacingRight(AnimationModel model){
		GameEntityModel ownerModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
		if (ownerModel == null) return false;
		return ownerModel.isFacingRight;
	}
	
}

