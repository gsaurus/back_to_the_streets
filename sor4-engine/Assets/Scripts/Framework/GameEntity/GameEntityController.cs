using UnityEngine;
using System.Collections;

// 
public class GameEntityController: Controller<GameEntityModel> {

	public static readonly string animVelocityAffector = "anim_vel_affector";



	public PhysicPointModel GetPointModel(GameEntityModel model){
		return StateManager.state.GetModel(model.physicsModelId) as PhysicPointModel;
	}

	public AnimationModel GetAnimationModel(GameEntityModel model){
		return StateManager.state.GetModel(model.animationModelId) as AnimationModel;
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

	public void Flip(GameEntityModel model){
		model.isFacingRight = !model.isFacingRight;
	}


#endregion


#region getters (conditions)


	public bool IsFacingRight(GameEntityModel model){
		return model.isFacingRight;
	}


	// ----------------------
	// Ground and Wall checks
	// TODO: do I need those or I can access directly?

	public bool IsGrounded(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return false;
		return pointModel.collisionFlags.ground;
	}

	public bool IsHittingLeftWall(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return false;
		return pointModel.collisionFlags.left;
	}

	public bool IsHittingRighttWall(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return false;
		return pointModel.collisionFlags.right;
	}

	public bool IsHittingNearWall(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return false;
		return pointModel.collisionFlags.near;
	}

	public bool IsHittingFarWall(GameEntityModel model){
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel == null) return false;
		return pointModel.collisionFlags.far;
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
