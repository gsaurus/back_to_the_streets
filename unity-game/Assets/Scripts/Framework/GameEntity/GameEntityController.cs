using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{


	// 
	public class GameEntityController: Controller<GameEntityModel> {

		// Animations can setup this velocity affector
		public static readonly string animVelocityAffector = "anim_vel_affector";

		// Input axis is automatically traduced to input depending on then inputVelocityFactor of the model
		public static readonly string inputVelocityAffector = "input_vel_affector";


		// Information carried during the update
		// Hits and hurts information
		// This information is updated through the teams manager and is consulted later by animations execution (conditions)
		// So the game entity controller doesn't directly manage them.
		// Hits & collisions are handled before physics and animations, to be consulted at that time
		private List<HitData> lastHits = new List<HitData>();
		private List<HitData> lastHurts = new List<HitData>();
		private List<ModelReference> lastHittenEntitiesIds = new List<ModelReference>();
		private List<ModelReference> lastHitterEntitiesIds = new List<ModelReference>();
		// For collision we won't need information about everyone
		// To simplify, one can only collide with only other entity at the same time
		private ModelReference lastCollisionEntityId;


		// Get PhysicPointModel
		public static PhysicPointModel GetPointModel(GameEntityModel model){
			return StateManager.state.GetModel(model.physicsModelId) as PhysicPointModel;
		}

		// Get AnimationModel
		public static AnimationModel GetAnimationModel(GameEntityModel model){
			return StateManager.state.GetModel(model.animationModelId) as AnimationModel;
		}

		// Get InputProvider
		public static Model GetInputProvider(GameEntityModel model){
			return StateManager.state.GetModel(model.inputModelId);
		}


		// Check collision against other entity
		public bool CollisionCollisionCheck(GameEntityModel model, GameEntityModel otherModel){
			AnimationModel animModel = GetAnimationModel(model);
			AnimationModel otherAnimModel = GetAnimationModel(otherModel);
			AnimationController animController = animModel.Controller() as AnimationController;
			PhysicPointModel pointModel = GetPointModel(model);
			PhysicPointModel otherPointModel = GetPointModel(otherModel);
			if (animController.CollisionCollisionCheck(animModel, pointModel.position, model.isFacingRight, otherAnimModel, otherPointModel.position, otherModel.isFacingRight)) {
				// Both entities get knowing they hit each other
				GameEntityController otherController = otherModel.Controller() as GameEntityController;
				otherController.lastCollisionEntityId = model.Index;
				lastCollisionEntityId = otherModel.Index;
				Debug.Log("Collision detected");
				return true;
			}
			return false;
		}


		// Check Hit against other entity
		public void HitCollisionCheck(GameEntityModel model, GameEntityModel otherModel){
			AnimationModel animModel = GetAnimationModel(model);
			AnimationModel otherAnimModel = GetAnimationModel(otherModel);
			AnimationController animController = animModel.Controller() as AnimationController;
			PhysicPointModel pointModel = GetPointModel(model);
			PhysicPointModel otherPointModel = GetPointModel(otherModel);
			HitData hitData = animController.HitCollisionCheck(animModel, pointModel.position, model.isFacingRight, otherAnimModel, otherPointModel.position, otherModel.isFacingRight);
			if (hitData != null) {
				// Both entities get knowing one hit the other
				GameEntityController otherController = otherModel.Controller() as GameEntityController;
				otherController.lastHurts.Add(hitData);
				lastHits.Add(hitData);
				Debug.Log("HIT detected");
			}
		}


		// Clear temporary information
		// Called from teams manager before any hit/collision checks
		public void ClearHitsInformation(){
			lastHits.Clear();
			lastHurts.Clear();
			lastHittenEntitiesIds.Clear();
			lastHitterEntitiesIds.Clear();
			lastCollisionEntityId = new ModelReference(ModelReference.InvalidModelIndex);
		}


		// Update automated stuff
		protected override void Update(GameEntityModel model){

			// first update the input velocity
			UpdateInputVelocityAffector(model);

			// if input velocity goes against current direction, flip
			CheckAutomaticFlip(model);

		}


		// Update the input velocity affector multiplying input axis and max input velocity
		private static void UpdateInputVelocityAffector(GameEntityModel model){
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
		private static void CheckAutomaticFlip(GameEntityModel model){
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
		public static void SetAnimationVelocity(GameEntityModel model, FixedVector3 velocity){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return;
			if (!model.isFacingRight){
				velocity.X *= -1;
			}
			pointModel.velocityAffectors[animVelocityAffector] = velocity;
		}

		// Apply a force on the physics velocity affector
		public static void AddImpulse(GameEntityModel model, FixedVector3 impulse){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return;
			if (!model.isFacingRight){
				impulse.X *= -1;
			}
//			FixedVector3 collisionVelocity;
//			pointModel.velocityAffectors.TryGetValue(PhysicPointController.collisionVelocityAffectorName, out collisionVelocity);
			pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] += impulse; //collisionVelocity + impulse;
		}

		// Flip the character on the X axis
		public static void Flip(GameEntityModel model){
			model.isFacingRight = !model.isFacingRight;
		}

		// Set automatic flip (to flip automatically based on animation velocity)
		public static void SetAutomaticFlip(GameEntityModel model, bool automaticFlip){
			model.automaticFlip = automaticFlip;
		}

		// Set the maximum velocity applied by input axis
		public static void SetMaxInputVelocity(GameEntityModel model, FixedVector3 maxVel){
			model.maxInputVelocity = maxVel;
		}


	#endregion


	#region getters (conditions)


		public static bool IsFacingRight(GameEntityModel model){
			return model.isFacingRight;
		}


		// ----------------------
		// Ground and Wall checks

		public static bool IsGrounded(GameEntityModel model){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return false;
			return PhysicPointController.IsGrounded(pointModel);
		}

		public static bool IsHittingLeftWall(GameEntityModel model){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return false;
			return pointModel.collisionInpact.X < 0;
		}

		public static bool IsHittingRightWall(GameEntityModel model){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return false;
			return pointModel.collisionInpact.X > 0;
		}

		public static bool IsHittingNearWall(GameEntityModel model){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return false;
			return pointModel.collisionInpact.Z < 0;
		}

		public static bool IsHittingFarWall(GameEntityModel model){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return false;
			return pointModel.collisionInpact.Z > 0;
		}

		public static FixedFloat CollisionVerticalForce(GameEntityModel model){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return FixedFloat.Zero;
			return pointModel.collisionInpact.Y;
		}
		
		public static FixedFloat CollisionHorizontalForce(GameEntityModel model){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return FixedFloat.Zero;
			return pointModel.collisionInpact.X;
		}
		
		public static FixedFloat CollisionZForce(GameEntityModel model){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return FixedFloat.Zero;
			return pointModel.collisionInpact.Z;
		}



		#region Hits / Collisions

			public static bool IsCollidingWithOthers(GameEntityModel model){
				GameEntityController controller = model.Controller() as GameEntityController;
				return controller.lastCollisionEntityId != ModelReference.InvalidModelIndex;
			}

			public static int HitTargetsCount(GameEntityModel model){
				GameEntityController controller = model.Controller() as GameEntityController;
				return controller.lastHits.Count; 
			}
			
			public static int HurtSourcesCount(GameEntityModel model){
				GameEntityController controller = model.Controller() as GameEntityController;
				return controller.lastHurts.Count; 
			}

		#endregion



	#endregion
		

	}



}
