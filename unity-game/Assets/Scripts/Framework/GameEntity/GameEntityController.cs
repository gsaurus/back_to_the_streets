using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{

	// Interface for objects that provides an entity reference for events
	public interface GameEntityReferenceDelegator{
		ModelReference GetEntityReference(GameEntityModel model);
	}


	// Controls mostly hits, flips, and any other specific things of entities
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
		// -----------
		// Hit/Hurt information is used by animation controllers to react to hits
		public List<HitInformation> lastHits = new List<HitInformation>();
		public List<HitInformation> lastHurts = new List<HitInformation>();
		// For collision we won't need information about everyone
		// To simplify, one can only collide with only other entity at the same time
		public ModelReference lastCollisionEntityId;

		private int nextPauseTimer;


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

		// Get an entity from an entity provider delegate
		public static GameEntityModel GetEntityFromDelegator(GameEntityReferenceDelegator refDelegator, GameEntityModel model){
			ModelReference reference = refDelegator.GetEntityReference(model);
			if (reference != null){
				return StateManager.state.GetModel(reference) as GameEntityModel;
			}
			return null;
		}

		// Get Position used in collision checks
		public static FixedVector3 GetRealPosition(GameEntityModel model){
			if (model.parentEntity == null || model.parentEntity == ModelReference.InvalidModelIndex){
				// Normal position
				PhysicPointModel pointModel = StateManager.state.GetModel(model.physicsModelId) as PhysicPointModel;
				return pointModel.position;
			}else {
				// Anchored, use parent position
				GameEntityModel parentModel = StateManager.state.GetModel(model.parentEntity) as GameEntityModel;
				FixedVector3 parentPosition = GetRealPosition(parentModel);
//				Debug.Log("Model old position: " + (StateManager.state.GetModel(model.physicsModelId) as PhysicPointModel).position
//					+ "\nModel parent position: " + parentPosition
//					+ "\nModel new position: " + parentPosition + " + " + model.positionRelativeToParent + " = " + (parentPosition + model.positionRelativeToParent)
//				);
				return parentPosition + model.positionRelativeToParent;
			}

		}


		// Check collision against other entity
		public bool CollisionCollisionCheck(GameEntityModel model, GameEntityModel otherModel){
			AnimationModel animModel = GetAnimationModel(model);
			AnimationModel otherAnimModel = GetAnimationModel(otherModel);
			AnimationController animController = animModel.Controller() as AnimationController;
			FixedVector3 position = GetRealPosition(model);
			FixedVector3 otherPosition = GetRealPosition(otherModel);
			if (animController.CollisionCollisionCheck(animModel, position, model.isFacingRight, otherAnimModel, otherPosition, otherModel.isFacingRight)) {
				// Both entities get knowing they hit each other
				GameEntityController otherController = otherModel.Controller() as GameEntityController;
				otherController.lastCollisionEntityId = model.Index;
				lastCollisionEntityId = otherModel.Index;
//				Debug.Log("Collision detected");
				return true;
			}
			return false;
		}


		// Check Hit against other entity
		public void HitCollisionCheck(GameEntityModel model, GameEntityModel otherModel){
			AnimationModel animModel = GetAnimationModel(model);
			AnimationModel otherAnimModel = GetAnimationModel(otherModel);
			AnimationController animController = animModel.Controller() as AnimationController;
			FixedVector3 position = GetRealPosition(model);
			FixedVector3 otherPosition = GetRealPosition(otherModel);
			HitInformation hitInformation = animController.HitCollisionCheck(
				animModel, position, model.isFacingRight,
				otherAnimModel, otherPosition, otherModel.isFacingRight
			);
			if (hitInformation != null) {
				// Both entities get knowing one hit the other
				GameEntityController otherController = otherModel.Controller() as GameEntityController;
				otherController.lastHurts.Add(hitInformation.HitWithEntity(model.Index));
				lastHits.Add(hitInformation.HitWithEntity(otherModel.Index));
				// Debug.Log(model.Index + " hit " + otherModel.Index);
			}
		}


		// Clear temporary information
		// Called from teams manager before any hit/collision checks
		public void ClearHitsInformation(){
			lastHits.Clear();
			lastHurts.Clear();
			lastCollisionEntityId = new ModelReference(ModelReference.InvalidModelIndex);
		}


		// Update automated stuff
		protected override void Update(GameEntityModel model){

			// first update the input velocity
			UpdateInputVelocityAffector(model);

			// if input velocity goes against current direction, flip
			CheckAutomaticFlip(model);

			// Built in pause timer
			if (model.pauseTimer > 0){
				PhysicPointModel pointModel = GetPointModel(model);
				if (pointModel != null) {
					if (--model.pauseTimer == 0 && (model.parentEntity == null || model.parentEntity == ModelReference.InvalidModelIndex)) {
						pointModel.isActive = true;
					}
				}
			}

			// Update all custom timers
			Dictionary<string, int> newTimerValues = new Dictionary<string, int>();
			foreach (KeyValuePair<string, int> timer in model.customTimers){
				if (timer.Value > 0){
					int newTimer = timer.Value - 1;
					// if there is a corresponding variable, automatically reset it
					// TODO: is this the intended behaviour?...
					if (newTimer == 0 && model.customVariables.ContainsKey(timer.Key)){
						model.customVariables[timer.Key] = 0;
					}
					newTimerValues[timer.Key] = newTimer;
				}
			}
			// consolidation
			foreach (KeyValuePair<string, int> timer in newTimerValues) {
				model.customTimers[timer.Key] = timer.Value;
			}

		}


		protected override void PostUpdate(GameEntityModel model){
			if (nextPauseTimer > 0) {
				// Pause physics and animation
				PhysicPointModel pointModel = GetPointModel(model);
				if (pointModel != null) {
					pointModel.isActive = false;
					model.pauseTimer = nextPauseTimer;
				}
				nextPauseTimer = 0;
			}
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



		// Flip the character on the X axis
		public static void Flip(GameEntityModel model){
			model.isFacingRight = !model.isFacingRight;
		}

		// Set automatic flip (to flip automatically based on animation velocity)
		public static void SetAutomaticFlip(GameEntityModel model, bool automaticFlip){
			model.automaticFlip = automaticFlip;
		}


		// Pause for a given number of frames (used for attack pause delay effect)
		public static void PausePhysics(GameEntityModel model, int numFrames){
			GameEntityController controller = model.Controller() as GameEntityController;
			controller.nextPauseTimer = numFrames;
		}


		// Face to hitter's location
		public static void FaceToHitterLocation(GameEntityModel model, bool oppositeFacing){
			GameEntityController controller = model.Controller() as GameEntityController;
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel != null && controller.lastHurts.Count > 0) {
				HitInformation info = controller.lastHurts[0];
				GameEntityModel hitter = StateManager.state.GetModel(info.entityId) as GameEntityModel;
				if (hitter != null) {
					PhysicPointModel hitterPoint = GameEntityController.GetPointModel(hitter);
					if (hitterPoint != null) {
						model.isFacingRight = pointModel.position.X < hitterPoint.position.X;
						if (oppositeFacing) {
							model.isFacingRight = !model.isFacingRight;
						}
					}
				}
			}
		}

		// Face to same direction as hitter is facing
		public static void FaceToHitterDirection(GameEntityModel model, bool oppositeFacing){
			GameEntityController controller = model.Controller() as GameEntityController;
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			if (pointModel != null && controller.lastHurts.Count > 0) {
				HitInformation info = controller.lastHurts[0];
				GameEntityModel hitter = StateManager.state.GetModel(info.entityId) as GameEntityModel;
				model.isFacingRight = hitter.isFacingRight;
				if (oppositeFacing) {
					model.isFacingRight = !model.isFacingRight;
				}
			}
		}

		public static void HurtBasedOnFacingOptions(GameEntityModel model, HitData.HitFacing facingOptions, FixedFloat damagePercentage){

			// Facing
			if (facingOptions == HitData.HitFacing.hitterLocation || facingOptions == HitData.HitFacing.inverseHitterLocation) {
				FaceToHitterLocation(model, facingOptions == HitData.HitFacing.inverseHitterLocation);
			} else if (facingOptions == HitData.HitFacing.hitterOrientation || facingOptions == HitData.HitFacing.inverseHitterOrientation) {
				FaceToHitterDirection(model, facingOptions == HitData.HitFacing.inverseHitterOrientation);
			} else {
				// None, nothing to do
			}

			// Damage!
			if (damagePercentage != 0 && model.customVariables.ContainsKey("energy")) {
				GameEntityController controller = model.Controller() as GameEntityController;
				foreach (HitInformation hitInfo in controller.lastHurts) {
					int damageValue = (int)(hitInfo.hitData.damage * damagePercentage);
					if (damageValue == 0) damageValue = 1;
					model.customVariables["energy"] -= damageValue;
				}
			}

		}

		public static void HurtBasedOnHitData(GameEntityModel model, FixedFloat damagePercentage){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller.lastHurts.Count > 0) {
				HitData hitData = controller.lastHurts[0].hitData;
				HurtBasedOnFacingOptions(model, hitData.facingOptions, damagePercentage);
			}
		}


	#endregion


	#region getters (conditions)



		// ----------------------
		// Hits / Hurts

		// Generic entity-entity collision
		public static bool IsCollidingWithOthers(GameEntityModel model){
			GameEntityController controller = model.Controller() as GameEntityController;
			return controller.lastCollisionEntityId != ModelReference.InvalidModelIndex;
		}

		// Successful hits count
		public static int HitTargetsCount(GameEntityModel model){
			GameEntityController controller = model.Controller() as GameEntityController;
			return controller.lastHits.Count; 
		}
		
		// Hitters count
		public static int HurtSourcesCount(GameEntityModel model){
			GameEntityController controller = model.Controller() as GameEntityController;
			return controller.lastHurts.Count; 
		}

		public static bool HurtContainsType(GameEntityModel model, HitData.HitType type){
			GameEntityController controller = model.Controller() as GameEntityController;
			foreach (HitInformation info in controller.lastHurts) {
				if (info.hitData.type == type) return true;
			}
			return false;
		}

		public static bool IsHurtFrontal(GameEntityModel model, bool frontal){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller.lastHurts.Count == 0) return false;
			GameEntityModel hitter = StateManager.state.GetModel(controller.lastHurts[0].entityId) as GameEntityModel;
			PhysicPointModel modelPoint = GameEntityController.GetPointModel(model);
			PhysicPointModel hitterPoint = GameEntityController.GetPointModel(hitter);
			bool isFrontal;
			if (model.isFacingRight) isFrontal = hitterPoint.position.X >= modelPoint.position.X;
			else isFrontal = hitterPoint.position.X <= modelPoint.position.X;
			return isFrontal == frontal;
		}

		// Hurt at a certain collisionID
		public static bool HurtsContainCollisionId(GameEntityModel model, int collisionId){
			GameEntityController controller = model.Controller() as GameEntityController;
			foreach (HitInformation hitInformation in controller.lastHurts) {
				if (hitInformation.collisionId == collisionId) {
					return true;
				}
			}
			return false;
		}

	#endregion
		

	}



}
