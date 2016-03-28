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
		// -----------
		// Hit/Hurt information is used by animation controllers to react to hits
		private List<HitInformation> lastHits = new List<HitInformation>();
		private List<HitInformation> lastHurts = new List<HitInformation>();
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
			PhysicPointModel pointModel = GetPointModel(model);
			PhysicPointModel otherPointModel = GetPointModel(otherModel);
			HitInformation hitInformation = animController.HitCollisionCheck(
				animModel, pointModel.position, model.isFacingRight,
				otherAnimModel, otherPointModel.position, otherModel.isFacingRight
			);
			if (hitInformation != null) {
				// Both entities get knowing one hit the other
				GameEntityController otherController = otherModel.Controller() as GameEntityController;
				otherController.lastHurts.Add(hitInformation.HitWithEntity(model.Index));
				lastHits.Add(hitInformation.HitWithEntity(otherModel.Index));
				//Debug.Log("HIT detected");
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
					if (--model.pauseTimer == 0 && model.parentEntity == null) {
						pointModel.isActive = true;
					}
				}
			}

			// Built in combo timer
			// If times up, combo counter is reset automatically
			if (model.comboTimer > 0) {
				if (--model.comboTimer == 0) {
					model.comboCounter = 0;
				}
			}

			// TODO: Update custom timers??

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


		// Pause for a given number of frames (used for attack pause delay effect)
		public static void PausePhysics(GameEntityModel model, int numFrames){
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel == null) return;
			pointModel.isActive = false;
			model.pauseTimer = numFrames;
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

		// Add 1 to combo counter
		public static void IncrementCombo(GameEntityModel model, int comboTimer){
			++model.comboCounter;
			model.comboTimer = comboTimer;
		}

		// Reset combo counter
		public static void ResetCombo(GameEntityModel model){
			model.comboCounter = 0;
			model.comboTimer = 0;
		}

		// Anchor a model to it
		public static void AnchorEntity(GameEntityModel model, GameEntityModel modelToBeAnchored, int anchorId){
			if (IsAnchored(modelToBeAnchored)){
				Debug.LogWarning("Trying to anchor an entity that is already anchored");
				return;
			}
			while (model.anchoredEntities.Count <= anchorId) {
				model.anchoredEntities.Add(null);
			}
			if (model.anchoredEntities[anchorId] != null){
				Debug.LogWarning("Trying to anchor an entity to a busy anchor");
				return;
			}
			model.anchoredEntities[anchorId] = modelToBeAnchored.Index;
			modelToBeAnchored.parentEntity = model.Index;
			PhysicPointModel pointModel = GetPointModel(modelToBeAnchored);
			if (pointModel != null){
				pointModel.isActive = false;
			}
		}

		// Release event may be accompained by a set animation event
		// TODO: no need to receive release position if using original relative position, can be setted before release
		public static void ReleaseAnchoredEntity(GameEntityModel model, int anchorId, FixedVector3 releasePosition){
			if (model.anchoredEntities.Count <= anchorId) return;
			GameEntityModel anchoredEntityModel = StateManager.state.GetModel(model.anchoredEntities[anchorId]) as GameEntityModel;
			if (anchoredEntityModel != null){
				anchoredEntityModel.parentEntity = null;
				PhysicPointModel pointModel = GetPointModel(anchoredEntityModel);
				if (pointModel != null){
					pointModel.isActive = true;
					PhysicPointModel parentPointModel = GetPointModel(model);
					PhysicPointController pointController = pointModel.Controller() as PhysicPointController;
					if (pointController != null && parentPointModel != null){
						// Set position directly
						pointModel.position = parentPointModel.position;
						if (!model.isFacingRight) releasePosition.X *= -1;
						pointController.SetVelocityAffector(pointModel, PhysicPointController.setPositionffectorName, releasePosition);
					}
				}
			}
			model.anchoredEntities[anchorId] = null;
		}


		// Safelly set position relative to self (e.g. vault), taking physics collisions in consideration
		public static void MoveEntity(GameEntityModel model, FixedVector3 relativePosition){
			if (!model.isFacingRight) relativePosition.X *= -1;
			PhysicPointModel pointModel = GetPointModel(model);
			if (pointModel != null){
				PhysicPointController pointController = pointModel.Controller() as PhysicPointController;
				if (pointController != null){
					pointController.SetVelocityAffector(pointModel, PhysicPointController.setPositionffectorName, relativePosition);
				}
			}
		}


		// Force anchored entity's position (e.g. for an attack move while grabbed, or to accompain while walking)
		public static void SetAnchoredEntityPosition(GameEntityModel model, int anchorId, FixedVector3 relativePosition){
			////TODO: hold it as an offset and apply on update, so no need to set every frame
//			if (model.anchoredEntities.Count <= anchorId) return;
//			GameEntityModel anchoredEntityModel = StateManager.state.GetModel(model.anchoredEntities[anchorId]) as GameEntityModel;
//			if (anchoredEntityModel != null){
//				PhysicPointModel pointModel = GetPointModel(anchoredEntityModel);
//				PhysicPointModel parentPointModel = GetPointModel(model);
//				if (pointModel != null && parentPointModel != null){
//					if (!model.isFacingRight) relativePosition.X *= -1;
//					pointModel.position = parentPointModel.position + relativePosition;
//				}
//			}
		}

		// TODO: probably better as new class like public class AnimationTransitionEvent: AnimationEvent{,
		// TODO: warning.. what happens if two animationTransitionEvents happen at once?.. maybe setAnchored thing should force it
		//public static void SetAnchoredEntityAnimation(GameEntityModel model, int anchorId, ...){


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


		// ----------------------
		// Combo counter & timer

		// Combo counter value
		public static int ComboCounter(GameEntityModel model){
			return model.comboCounter;
		}

		// Combo timer
		public static int ComboTimer(GameEntityModel model){
			return model.comboTimer;
		}

		// Parent name (who's anchoring the entity)
		public static string ParentEntityName(GameEntityModel model){
			GameEntityModel parentEntityModel = StateManager.state.GetModel(model.parentEntity) as GameEntityModel;
			if (parentEntityModel == null) return null;
			AnimationModel animModel = GetAnimationModel(parentEntityModel);
			if (animModel == null) return null;
			return animModel.characterName;
		}

		// Anchored name
		public static string AnchoredEntityName(GameEntityModel model, int anchorId){
			if (model.anchoredEntities.Count <= anchorId || model.anchoredEntities[anchorId] == null) return null;
			GameEntityModel anchoredEntityModel = StateManager.state.GetModel(model.anchoredEntities[anchorId]) as GameEntityModel;
			if (anchoredEntityModel == null) return null;
			AnimationModel animModel = GetAnimationModel(anchoredEntityModel);
			if (animModel == null) return null;
			return animModel.characterName;
		}

		// Is anchored
		public static bool IsAnchored(GameEntityModel model){
			return model.parentEntity != null && model.parentEntity.index != ModelReference.InvalidModelIndex;
		}

		// Is anchoring something
		public static bool IsAnchoring(GameEntityModel model, int anchorId){
			if (model.anchoredEntities.Count <= anchorId) return false;
			return model.anchoredEntities[anchorId] != null;
		}


	#endregion
		

	}



}
