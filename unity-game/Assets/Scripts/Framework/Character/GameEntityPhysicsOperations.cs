using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{

	// TODO: remove this class?
	// Entity physics operations (velocity, impulse..)
	public static class GameEntityPhysicsOperations{


		#region affectors (events)

		// Set the velocity affector bound to animation velocity control
		public static void SetAnimationVelocity(GameEntityModel model, FixedVector3 velocity){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return;
			if (!model.IsFacingRight()){
				velocity.X *= -1;
			}
			pointModel.velocityAffectors[GameEntityController.animVelocityAffector] = velocity;
		}

		// Apply a force on the physics velocity affector
		public static void AddImpulse(GameEntityModel model, FixedVector3 impulse){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return;
			if (!model.IsFacingRight()){
				impulse.X *= -1;
			}
			pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] += impulse;
	//			pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] += new FixedVector3(0, impulse.Y, 0);
	//			pointModel.velocityAffectors[GameEntityController.animVelocityAffector] += new FixedVector3(impulse.X, 0, impulse.Z);
		}


		// Reset X and Z force components on the physics velocity affector
		public static void ResetPlanarImpulse(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return;
			FixedVector3 originalImpulse;
			pointModel.velocityAffectors.TryGetValue(PhysicPointModel.defaultVelocityAffectorName, out originalImpulse);
			pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] =
				new FixedVector3(0, originalImpulse.Y, 0)
			;
		}


		// Set the maximum velocity applied by input axis
		public static void SetMaxInputVelocity(GameEntityModel model, FixedVector3 maxVel){
			model.maxInputVelocity = maxVel;
		}



		// Safelly set position relative to self (e.g. vault), taking physics collisions in consideration
		public static void MoveEntity(GameEntityModel model, FixedVector3 relativePosition){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel != null){
				PhysicPointController pointController = pointModel.Controller() as PhysicPointController;
				if (pointController != null){
					pointController.SetVelocityAffector(pointModel, PhysicPointController.setPositionffectorName, relativePosition);
				}
			}
		}
			

		#endregion


		#region getters (conditions)


		public static bool IsFacingRight(GameEntityModel model){
			return model.mIsFacingRight;
		}


		// ----------------------
		// Ground and Wall checks

		public static bool IsGrounded(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return false;
			return PhysicPointController.IsGrounded(pointModel);
		}

		public static bool IsHittingLeftWall(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return false;
			return pointModel.collisionInpact.X < 0;
		}

		public static bool IsHittingRightWall(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return false;
			return pointModel.collisionInpact.X > 0;
		}

		public static bool IsHittingNearWall(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return false;
			return pointModel.collisionInpact.Z < 0;
		}

		public static bool IsHittingFarWall(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return false;
			return pointModel.collisionInpact.Z > 0;
		}

		public static FixedFloat CollisionVerticalForce(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return FixedFloat.Zero;
			return FixedFloat.Abs(pointModel.collisionInpact.Y);
		}

		public static FixedFloat CollisionHorizontalForce(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return FixedFloat.Zero;
			return FixedFloat.Abs(pointModel.collisionInpact.X);
		}

		public static FixedFloat CollisionZForce(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return FixedFloat.Zero;
			return FixedFloat.Abs(pointModel.collisionInpact.Z);
		}


		// Apply a force on the physics velocity affector
		public static FixedFloat GetVerticalImpulse(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return FixedFloat.Zero;
			return pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName].Y;
		}



		#endregion



	}


}

