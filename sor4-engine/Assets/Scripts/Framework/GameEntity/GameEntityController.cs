using UnityEngine;
using System.Collections;


namespace RetroBread{


	// 
	public class GameEntityController: Controller<GameEntityModel> {

		public const uint groundFramesTolerance = 2;

		// Animations can setup this velocity affector
		public static readonly string animVelocityAffector = "anim_vel_affector";

		// Input axis is automatically traduced to input depending on then inputVelocityFactor of the model
		public static readonly string inputVelocityAffector = "input_vel_affector";



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
			pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] += impulse;
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
		public static void SetMaxInputVelocity(GameEntityModel Model, FixedVector3 maxVel){
			Model.maxInputVelocity = maxVel;
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
			return pointModel.collisionInpact.Y != 0 || pointModel.framesSinceLastTimeGrounded <= groundFramesTolerance;
		}

		public static bool IsHittingLeftWall(GameEntityModel model){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return false;
			return pointModel.collisionInpact.X < 0;
		}

		public static bool IsHittingRighttWall(GameEntityModel model){
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



	#endregion
		

	}



}
