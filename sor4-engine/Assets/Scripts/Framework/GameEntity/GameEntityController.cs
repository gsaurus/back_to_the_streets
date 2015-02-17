using UnityEngine;
using System.Collections;

// 
public class GameEntityController: Controller<GameEntityModel> {

	// Animations can setup this velocity affector
	public static readonly string animVelocityAffector = "anim_vel_affector";

	// Input axis is automatically traduced to input depending on then inputVelocityFactor of the model
	public static readonly string inputVelocityAffector = "input_vel_affector";


	// Get PhysicPointModel
	public PhysicPointModel GetPointModel(GameEntityModel model){
		return StateManager.state.GetModel(model.physicsModelId) as PhysicPointModel;
	}

	// Get AnimationModel
	public AnimationModel GetAnimationModel(GameEntityModel model){
		return StateManager.state.GetModel(model.animationModelId) as AnimationModel;
	}

	// Get InputProvider
	public Model GetInputProvider(GameEntityModel model){
		return StateManager.state.GetModel(model.inputModelId);
	}


	// Update automated stuff
	public override void Update(GameEntityModel model){
		// first update the input velocity
		UpdateInputVelocityAffector(model);
		// if input velocity goes against current direction, flip
		CheckAutomaticFlip(model);
	}


	// Update the input velocity affector multiplying input axis and max input velocity
	private void UpdateInputVelocityAffector(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return;
		FixedVector3 newInputVel = FixedVector3.Zero;
		if (model.maxInputVelocity.X != 0 || model.maxInputVelocity.Y != 0){
			Model inputModel = GetInputProvider(model);
			if (inputModel == null) return;
			GameEntityInputProvider inputController = inputModel.Controller() as GameEntityInputProvider;
			if (inputController == null) return;
			FixedVector3 axis = inputController.GetInputAxis(inputModel);
			newInputVel = new FixedVector3(axis.X*model.maxInputVelocity.X, axis.Y*model.maxInputVelocity.Y, axis.Z*model.maxInputVelocity.Z);
		}
		pointModel.velocityAffectors[inputVelocityAffector] = newInputVel;
	}


	// If automatic flip is on, flip automatically if input velocity is going against entity direction
	private void CheckAutomaticFlip(GameEntityModel model){
		if (model.automaticFlip){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return;
			FixedVector3 inputVelocity = pointModel.velocityAffectors[inputVelocityAffector];
			if (inputVelocity.X != 0  && (inputVelocity.X > 0 != model.isFacingRight)){
				Flip(model);
			}
		}
	}


#region affectors (events)

	// Set the velocity affector bound to animation velocity control
	public void SetAnimationVelocity(GameEntityModel model, FixedVector3 velocity){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return;
		pointModel.velocityAffectors[animVelocityAffector] = velocity;
	}

	// Apply a force on the physics velocity affector
	public void AddImpulse(GameEntityModel model, FixedVector3 impulse){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return;
		pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] += impulse;
	}

	// Flip the character on the X axis
	public void Flip(GameEntityModel model){
		model.isFacingRight = !model.isFacingRight;
	}

	// Set automatic flip (to flip automatically based on animation velocity)
	public void SetAutomaticFlip(GameEntityModel model, bool automaticFlip){
		model.automaticFlip = automaticFlip;
	}

	// Set the maximum velocity applied by input axis
	public void SetMaxInputVelocity(GameEntityModel Model, FixedVector3 maxVel){
		Model.maxInputVelocity = maxVel;
	}


#endregion


#region getters (conditions)


	public bool IsFacingRight(GameEntityModel model){
		return model.isFacingRight;
	}


	// ----------------------
	// Ground and Wall checks

	public bool IsGrounded(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return false;
		return pointModel.collisionInpact.Y < 0;
	}

	public bool IsHittingLeftWall(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return false;
		return pointModel.collisionInpact.X < 0;
	}

	public bool IsHittingRighttWall(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return false;
		return pointModel.collisionInpact.X > 0;
	}

	public bool IsHittingNearWall(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return false;
		return pointModel.collisionInpact.Z < 0;
	}

	public bool IsHittingFarWall(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return false;
		return pointModel.collisionInpact.Z > 0;
	}

	public FixedFloat CollisionVerticalForce(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return FixedFloat.Zero;
		return pointModel.collisionInpact.Y;
	}
	
	public FixedFloat CollisionHorizontalForce(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return FixedFloat.Zero;
		return pointModel.collisionInpact.X;
	}
	
	public FixedFloat CollisionZForce(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return FixedFloat.Zero;
		return pointModel.collisionInpact.Z;
	}



#endregion

// err.. we can create the conditions elsewhere
//#region condition builders
//
//	public BoolCondition ConditionIsFacingRight(bool value){
//		return new BoolCondition(IsFacingRight, value);
//	}
//
//	// Ground and Wall checks
//	public BoolCondition ConditionIsGrounded(bool value){
//		return new BoolCondition(IsGrounded, value);
//	}
//	public BoolCondition ConditionIsHittingLeftWall(bool value){
//		return new BoolCondition(IsHittingLeftWall, value);
//	}
//	public BoolCondition ConditionIsHittingRightWall(bool value){
//		return new BoolCondition(IsHittingRighttWall, value);
//	}
//	public BoolCondition ConditionIsHittingNearWall(bool value){
//		return new BoolCondition(IsHittingNearWall, value);
//	}
//	public BoolCondition ConditionIsHittingFarWall(bool value){
//		return new BoolCondition(IsHittingFarWall, value);
//	}
//
//
//
//#endregion

}
