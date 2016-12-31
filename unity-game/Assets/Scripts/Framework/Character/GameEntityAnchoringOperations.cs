using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{


	// Anchoring operations
	public static class GameEntityAnchoringOperations{


		#region affectors (events)


		// Anchor a model to it, obtained from a selector, and optionally move the parent relative to the entity being anchored (e.g. move back a little when grabbing)
		public static void AnchorEntity(GameEntityModel model, GameEntityReferenceDelegator anchoringRefDelegator, int anchorId, FixedVector3 deltaPosRelativeToAnched){
			GameEntityModel modelToBeAnchored = GameEntityController.GetEntityFromDelegator(anchoringRefDelegator, model);
			if (modelToBeAnchored != null){
				AnchorEntity(model, modelToBeAnchored, anchorId, deltaPosRelativeToAnched);
			}
		}

		// Anchor a model to it, and optionally move the parent relative to the entity being anchored (e.g. move back a little when grabbing)
		public static void AnchorEntity(GameEntityModel model, GameEntityModel modelToBeAnchored, int anchorId, FixedVector3 deltaPosRelativeToAnched){
			AnchorEntity(model, modelToBeAnchored, anchorId);
			PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
			PhysicPointModel anchoredPointModel = GameEntityController.GetPointModel(modelToBeAnchored);
			if (pointModel != null && anchoredPointModel != null){
				PhysicPointController pointController = pointModel.Controller() as PhysicPointController;
				if (pointController != null){
					if (!model.IsFacingRight()) deltaPosRelativeToAnched.X *= -1;
					FixedVector3 deltaPos = (anchoredPointModel.position + deltaPosRelativeToAnched) - pointModel.position;
					pointController.SetVelocityAffector(pointModel, PhysicPointController.setPositionffectorName, deltaPos);
				}
			}
		}


		// Anchor a model to it, obtained from a selector
		public static void AnchorEntity(GameEntityModel model, GameEntityReferenceDelegator anchoringRefDelegator, int anchorId){
			GameEntityModel modelToBeAnchored = GameEntityController.GetEntityFromDelegator(anchoringRefDelegator, model);
			if (modelToBeAnchored != null){
				AnchorEntity(model, modelToBeAnchored, anchorId);
			}
		}

		// Anchor a model to it
		public static void AnchorEntity(GameEntityModel model, GameEntityModel modelToBeAnchored, int anchorId){
			if (IsAnchored(modelToBeAnchored)){
				Debug.LogWarning("Trying to anchor an entity that is already anchored");
				return;
			}
			if (model.parentEntity != null && model.parentEntity == modelToBeAnchored.Index){
				Debug.LogWarning("Cyclic anchoring attempt");
				return;
			}
			if (model.anchoredEntities == null) model.anchoredEntities = new List<ModelReference>(anchorId);
			while (model.anchoredEntities.Count <= anchorId) {
				model.anchoredEntities.Add(null);
			}
			if (model.anchoredEntities[anchorId] != null && model.anchoredEntities[anchorId] != ModelReference.InvalidModelIndex){
				Debug.LogWarning("Trying to anchor an entity to a busy anchor");
				return;
			}
			model.anchoredEntities[anchorId] = modelToBeAnchored.Index;
			modelToBeAnchored.parentEntity = model.Index;
			modelToBeAnchored.positionRelativeToParent = FixedVector3.Zero;
			PhysicPointModel pointModel = GameEntityController.GetPointModel(modelToBeAnchored);
			if (pointModel != null){
				pointModel.isActive = false;
			}
		}


		// Release all anchored entities
		public static void ReleaseAllAnchoredEntities(GameEntityModel model){
			if (model.anchoredEntities == null) return;
			for (int i = 0 ; i < model.anchoredEntities.Count ; ++i){
				ReleaseAnchoredEntity(model, i);
			}
		}


		// Release event may be accompained by a set animation event and set anchored position.
		// It safely releases at the relative position to parent, taking physics in consideration
		public static void ReleaseAnchoredEntity(GameEntityModel model, int anchorId){
			if (model.anchoredEntities == null || model.anchoredEntities.Count <= anchorId) return;
			GameEntityModel anchoredEntityModel = StateManager.state.GetModel(model.anchoredEntities[anchorId]) as GameEntityModel;
			if (anchoredEntityModel != null){
				anchoredEntityModel.parentEntity = new ModelReference();
				PhysicPointModel pointModel = GameEntityController.GetPointModel(anchoredEntityModel);
				if (pointModel != null){
					pointModel.isActive = true;
					PhysicPointModel parentPointModel = GameEntityController.GetPointModel(model);
					PhysicPointController pointController = pointModel.Controller() as PhysicPointController;
					if (pointController != null && parentPointModel != null){
						// Set position directly
						pointModel.position = parentPointModel.position;
						if (!model.IsFacingRight()) anchoredEntityModel.positionRelativeToParent.X *= -1;
						pointController.SetVelocityAffector(pointModel, PhysicPointController.setPositionffectorName, anchoredEntityModel.positionRelativeToParent);
						anchoredEntityModel.positionRelativeToParent = FixedVector3.Zero;
					}
				}
			}
			model.anchoredEntities[anchorId] = new ModelReference();
		}


		// Set anchored entity position relatively to it's parent
		public static void SetAnchoredEntityRelativePosition(GameEntityModel model, int anchorId, FixedVector3 relativePosition){
			if (model.anchoredEntities == null || model.anchoredEntities.Count <= anchorId) return;
			GameEntityModel anchoredEntityModel = StateManager.state.GetModel(model.anchoredEntities[anchorId]) as GameEntityModel;
			if (anchoredEntityModel != null){
				anchoredEntityModel.positionRelativeToParent = relativePosition;
			}
		}
			


		// Forces the animation of an anchored entity, so that it can't be messed with it's current animation events
		public static void SetAnchoredEntityAnimation(GameEntityModel model, int anchorId, string animationName){
			if (model.anchoredEntities == null || model.anchoredEntities.Count <= anchorId) return;
			GameEntityModel anchoredEntityModel = StateManager.state.GetModel(model.anchoredEntities[anchorId]) as GameEntityModel;
			if (anchoredEntityModel != null){
				AnimationModel anchoredAnimationModel = GameEntityController.GetAnimationModel(anchoredEntityModel);
				AnimationController anchoredAnimController = anchoredAnimationModel.Controller() as AnimationController;
				if (anchoredAnimController != null){
					// Force animation, so that it ignores any desired transition from a previous animation update 
					anchoredAnimController.ForceAnimation(anchoredAnimationModel, animationName);
				}
			}
		}


		// Take ownership of a model, obtained from a selector
		public static void OwnEntity(GameEntityModel model, GameEntityReferenceDelegator ownedRefDelegator){
			GameEntityModel modelToBeOwned = GameEntityController.GetEntityFromDelegator(ownedRefDelegator, model);
			if (modelToBeOwned != null){
				modelToBeOwned.ownerEntity = model.Index;
				model.ownedEntities.Add(modelToBeOwned.Index);
			}
		}


		// Get freedom from owner entity
		public static void ReleaseOwnership(GameEntityModel model){
			if (model.ownerEntity != null && model.ownerEntity != ModelReference.InvalidModelIndex) {
				GameEntityModel owner = StateManager.state.GetModel(model.ownerEntity) as GameEntityModel;
				owner.ownedEntities.Remove(model.Index);
				model.ownerEntity = new ModelReference(ModelReference.InvalidModelIndex);
			}
		}
			

		#endregion




		#region getters (conditions)


		// Parent name (who's anchoring the entity)
		public static string ParentEntityName(GameEntityModel model){
			GameEntityModel parentEntityModel = StateManager.state.GetModel(model.parentEntity) as GameEntityModel;
			if (parentEntityModel == null) return null;
			AnimationModel animModel = GameEntityController.GetAnimationModel(parentEntityModel);
			if (animModel == null) return null;
			return animModel.characterName;
		}

		// Anchored name
		public static string AnchoredEntityName(GameEntityModel model, int anchorId){
			if (model.anchoredEntities == null || model.anchoredEntities.Count <= anchorId || model.anchoredEntities[anchorId] == null) return null;
			GameEntityModel anchoredEntityModel = StateManager.state.GetModel(model.anchoredEntities[anchorId]) as GameEntityModel;
			if (anchoredEntityModel == null) return null;
			AnimationModel animModel = GameEntityController.GetAnimationModel(anchoredEntityModel);
			if (animModel == null) return null;
			return animModel.characterName;
		}

		// Anchored animation name
		public static string AnchoredEntityAnimation(GameEntityModel model, int anchorId){
			if (model.anchoredEntities == null || model.anchoredEntities.Count <= anchorId || model.anchoredEntities[anchorId] == null) return null;
			GameEntityModel anchoredEntityModel = StateManager.state.GetModel(model.anchoredEntities[anchorId]) as GameEntityModel;
			if (anchoredEntityModel == null) return null;
			AnimationModel animModel = GameEntityController.GetAnimationModel(anchoredEntityModel);
			if (animModel == null) return null;
			return animModel.animationName;
		}

		// Is anchored
		public static bool IsAnchored(GameEntityModel model){
			return model.parentEntity != null && model.parentEntity.index != ModelReference.InvalidModelIndex;
		}

		// Is anchoring something
		public static bool IsAnchoring(GameEntityModel model, int anchorId){
			if (model.anchoredEntities == null || model.anchoredEntities.Count <= anchorId) return false;
			return model.anchoredEntities[anchorId] != null && model.anchoredEntities[anchorId] != ModelReference.InvalidModelIndex;
		}


		#endregion


	
	}


}

