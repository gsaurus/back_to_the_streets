using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{


	// Entity physics operations (velocity, impulse..)
	public static class GameEntityPhysicsOperations{


		#region affectors (events)

		// Set the velocity affector bound to animation velocity control
		public static void SetAnimationVelocity(GameEntityModel model, FixedVector3 velocity){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return;
			if (!model.isFacingRight){
				velocity.X *= -1;
			}
			pointModel.velocityAffectors[GameEntityController.animVelocityAffector] = velocity;
		}

		// Apply a force on the physics velocity affector
		public static void AddImpulse(GameEntityModel model, FixedVector3 impulse){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return;
			if (!model.isFacingRight){
				impulse.X *= -1;
			}
			//			FixedVector3 collisionVelocity;
			//			pointModel.velocityAffectors.TryGetValue(PhysicPointController.collisionVelocityAffectorName, out collisionVelocity);
			pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] += impulse; //collisionVelocity + impulse;
		}


		// Reset X and Z force components on the physics velocity affector
		public static void ResetPlanarImpulse(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return;
			pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] =
				new FixedVector3(0, pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName].Y, 0)
			;
		}


		// Apply a force on the physics velocity affector of a referenced entity
		// Note: there could be generic templated methods receiving delegate methods... 
		public static void AddImpulse(GameEntityModel model, GameEntityReferenceDelegator refDelegator, FixedVector3 impulse){
			if (!model.isFacingRight) impulse.X *= -1;
			GameEntityModel refModel = GameEntityController.GetEntityFromDelegator(refDelegator, model);
			if (refModel != null){
				AddImpulse(refModel, impulse);
			}
		}


		// Set the maximum velocity applied by input axis
		public static void SetMaxInputVelocity(GameEntityModel model, FixedVector3 maxVel){
			model.maxInputVelocity = maxVel;
		}



		// Safelly set position relative to self (e.g. vault), taking physics collisions in consideration
		public static void MoveEntity(GameEntityModel model, FixedVector3 relativePosition){
			if (!model.isFacingRight) relativePosition.X *= -1;
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
			return model.isFacingRight;
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
			return pointModel.collisionInpact.Y;
		}

		public static FixedFloat CollisionHorizontalForce(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return FixedFloat.Zero;
			return pointModel.collisionInpact.X;
		}

		public static FixedFloat CollisionZForce(GameEntityModel model){
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel == null) return FixedFloat.Zero;
			return pointModel.collisionInpact.Z;
		}

	

		#endregion


	
	}


}

