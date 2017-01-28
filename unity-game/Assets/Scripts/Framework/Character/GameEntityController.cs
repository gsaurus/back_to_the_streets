using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{

	// Interface for objects that provides an entity reference for events
	public interface GameEntityReferenceDelegator{
		ModelReference GetEntityReference(GameEntityModel model);
	}

	public static class GameEntityModelExtensions{
	
		public static bool IsFacingRight(this GameEntityModel model){
			if (model.parentEntity != null && model.parentEntity != ModelReference.InvalidModelIndex) {
				GameEntityModel parentModel = StateManager.state.GetModel(model.parentEntity) as GameEntityModel;
				return parentModel.mIsFacingRight;
			}
			return model.mIsFacingRight;
		}
			
	}


	// Controls mostly hits, flips, and any other specific things of entities
	public class GameEntityController: Controller<GameEntityModel> {

		// Animations can setup this velocity affector
		public static readonly string animVelocityAffector = "anim_vel_affector";

		// Input axis is automatically traduced to input depending on then inputVelocityFactor of the model
		public static readonly string inputVelocityAffector = "input_vel_affector";


		public enum InclusionType{
			none		= 0,
			contains	= 1,
			only		= 2,
			except		= 3
		};


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
				return parentPosition + new FixedVector3((parentModel.IsFacingRight() ? 1 : -1) * model.positionRelativeToParent.X, model.positionRelativeToParent.Y, model.positionRelativeToParent.Z);
			}
		}


		// Check collision against other entity
		public bool CollisionCollisionCheck(GameEntityModel model, GameEntityModel otherModel){
			AnimationModel animModel = GetAnimationModel(model);
			AnimationModel otherAnimModel = GetAnimationModel(otherModel);
			AnimationController animController = animModel.Controller() as AnimationController;
			FixedVector3 position = GetRealPosition(model);
			FixedVector3 otherPosition = GetRealPosition(otherModel);
			if (animController.CollisionCollisionCheck(animModel, position, model.IsFacingRight(), otherAnimModel, otherPosition, otherModel.IsFacingRight())) {
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
				animModel, position, model.IsFacingRight(),
				otherAnimModel, otherPosition, otherModel.IsFacingRight()
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
				if (inputVelocity.X != 0  && (inputVelocity.X > 0 != model.IsFacingRight())){
					Flip(model);
				}
			}
		}
			


	#region affectors (events)



		// Flip the character on the X axis
		public static void Flip(GameEntityModel model){
			model.mIsFacingRight = !model.mIsFacingRight;
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
						model.mIsFacingRight = pointModel.position.X < hitterPoint.position.X;
						if (oppositeFacing) {
							model.mIsFacingRight = !model.mIsFacingRight;
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
				model.mIsFacingRight = hitter.IsFacingRight();
				if (oppositeFacing) {
					model.mIsFacingRight = !model.IsFacingRight();
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


		// Return true if cycle must stop, output variable does the comparison and must be used as result
		private static bool InclusionCheck(InclusionType inclusionType, int original, int other, out bool inclusion){
			switch (inclusionType) {
				case InclusionType.none:{
					inclusion = true;
					return true;
				}
				case InclusionType.contains:{
					inclusion = original == other;
					if (inclusion) return true;
				} break;
				case InclusionType.except:{
					inclusion = original != other;
					if (!inclusion) return true;
				} break;
				case InclusionType.only:{
					inclusion = original == other;
					if (!inclusion) return true;
				} break;
				default:{
					inclusion = false;
					break;
				}
			}
			return false;
		}

		// Generic entity-entity collision with team check
		public static bool IsCollidingWithTeam(GameEntityModel model, InclusionType teamInclusionType, int teamId){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller.lastCollisionEntityId == ModelReference.InvalidModelIndex) return false;
			bool isValid;
			InclusionCheck(teamInclusionType, WorldUtils.GetEntityTeam(controller.lastCollisionEntityId), teamId, out isValid);
			return isValid;
		}

		// Successful hits a team
		public static bool HitTeam(GameEntityModel model, InclusionType teamInclusionType, int teamId, InclusionType typeInclusionType, int type){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller.lastHits.Count == 0) return false;
			bool isValid = false;
			// Check team
			foreach (HitInformation hit in controller.lastHits) {
				if (InclusionCheck(teamInclusionType, WorldUtils.GetEntityTeam(hit.entityId), teamId, out isValid)) {
					break;
				}
			}
			if (!isValid) return false;

			// Check type
			foreach (HitInformation hit in controller.lastHits) {
				if (InclusionCheck(typeInclusionType, (int) hit.hitData.type, type, out isValid)) {
					break;
				}
			}
			return isValid;
		}

		// Hitters count
		public static bool HurtFromTeam(GameEntityModel model, int teamId){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller.lastHurts.Count == 0) return false;
			foreach (HitInformation hurt in controller.lastHurts) {
				if (WorldUtils.GetEntityTeam(hurt.entityId) == teamId) {
					return true;
				}
			}
			return false;
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
			if (model.IsFacingRight()) isFrontal = hitterPoint.position.X >= modelPoint.position.X;
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
