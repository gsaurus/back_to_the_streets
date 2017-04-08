using System;
using System.Collections.Generic;
using System.Linq;

namespace RetroBread{


public static class CharacterSubjectsBuilder {

	public enum PredefinedSubjects {
		none			= -2,
		globalVar		= -1,
		self			= 0
	}

	private enum AnyOrAllOptions{
		anyOf = 0,
		allBut = 1
	}

	private enum OrientationOptions{
		any = 0,
		fromBack = 1,
		fromFront = 2
	}

	// Default subject delegation delegate
	private delegate void DefaultSubjectDelegatioDelegate(GameEntityModel model, List<GameEntityModel> subjects);
	
	// Subjects getter delegate builders indexed by type directly on array
	private delegate EventSubject<GameEntityModel>.GetSubjectsDelegate BuilderAction(Storage.GenericParameter param);
	private static BuilderAction[] builderActions = {
		// Default, always present
		GetSelf,
		// Custom, added if necessary, parameterised
		GetOwner,
		GetOwnerOrSelf,
		GetParent,
		GetGrabbed,
		GetHitters,
		GetHittens,
		GetColliding,
		GetAll
	};
		

	// The public builder method
	public static List<EventSubject<GameEntityModel>> Build(Storage.Character charData, int[] subjectIds){
		List<EventSubject<GameEntityModel>> subjects = new List<EventSubject<GameEntityModel>>(subjectIds == null ? 1 : subjectIds.Length);

		// Build default subjects
		subjects.Add(BuildDefaultSubject(GetSelf));
		if (subjectIds == null) return subjects;
		int numDefaultSubjects = subjects.Count;

		// Build custom subjects
		EventSubject<GameEntityModel> subject;
		foreach (int subjectId in subjectIds) {
			subject = BuildFromParameter(charData.genericParameters[subjectId], numDefaultSubjects);
			if (subject != null) {
				subjects.Add(subject);
			}
		}
		return subjects;
	}


	// Build a single subject
	private static EventSubject<GameEntityModel> BuildFromParameter(Storage.GenericParameter parameter, int initialIndex){
		EventSubject<GameEntityModel>.GetSubjectsDelegate getSubjectDelegate;
		EventSubject<GameEntityModel>.ReevaluateSubjectsDelegate reevaluateSubject;
		int callIndex = parameter.type;
		if (callIndex < builderActions.Length) {
			getSubjectDelegate = builderActions[initialIndex + callIndex](parameter);
			reevaluateSubject = GetReevaluateSubject(parameter);
			return new EventSubject<GameEntityModel>(getSubjectDelegate, reevaluateSubject);
		}
		Debug.Log("CharacterSubjectsBuilder: Unknown subject type: " + parameter.type);
		return null;
	}


	// Build the default subject for the given builder method
	private static EventSubject<GameEntityModel> BuildDefaultSubject(BuilderAction subjectsDefaultBuilderAction){
		return new EventSubject<GameEntityModel>(subjectsDefaultBuilderAction(null), GetReevaluateSubject(null));
	}

	// Get the reevaluate delegate
	private static EventSubject<GameEntityModel>.ReevaluateSubjectsDelegate GetReevaluateSubject(Storage.GenericParameter parameter){
		// Either return all or a single instance
		return delegate(List<GameEntityModel> subjectModels){
			if (parameter != null && parameter.SafeBool(0) && subjectModels != null && subjectModels.Count > 1){
				// Return one randomly
				return CreateSubjectsWithSubject(subjectModels[StateManager.state.Random.NextInt(0, subjectModels.Count - 1)]);
			} else{
				return subjectModels;
			}
		};
	}

	private static List<GameEntityModel> CreateSubjectsWithSubject(GameEntityModel subject){
		List<GameEntityModel> subjects = new List<GameEntityModel>(1);
		subjects.Add(subject);
		return subjects;
	}



	// Default Subject Delegation
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate DefaultDelegation(int subjectId, DefaultSubjectDelegatioDelegate subjectDelegation){
		// Create the delegate
		return delegate(GameEntityModel selfModel, List<GameEntityModel>[] subjectModels){
			List<GameEntityModel> models;
			if (subjectId == (int)PredefinedSubjects.self){
				models = new List<GameEntityModel>();
				models.Add(selfModel);
			}else {
				models = subjectModels[subjectId];
			}
			if (models == null || models.Count == 0) return null;
			List<GameEntityModel> subjects = new List<GameEntityModel>(models.Count);
			foreach (GameEntityModel model in models){
				subjectDelegation(model, subjects);
			}
			return subjects;
		};
	}




#region Default Subjects

	// 0: GetSelf
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate GetSelf(Storage.GenericParameter parameter){
		// Create the delegate
		return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
			return CreateSubjectsWithSubject(model);
		};
	}

	// 1: GetOwner
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate GetOwner(Storage.GenericParameter parameter){
		int subjectId = parameter.SafeInt(0);
		return DefaultDelegation(subjectId, delegate(GameEntityModel model, List<GameEntityModel> subjects){
			GameEntityModel ownerModel = StateManager.state.GetModel(model.ownerEntity) as GameEntityModel;
			if (ownerModel != null){
				subjects.Add(ownerModel);
			}
		});
	}

	// 2: GetOwnerOrSelf
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate GetOwnerOrSelf(Storage.GenericParameter parameter){
		int subjectId = parameter.SafeInt(0);
		return DefaultDelegation(subjectId, delegate(GameEntityModel model, List<GameEntityModel> subjects){
			GameEntityModel ownerOrSelfModel = StateManager.state.GetModel(model.ownerEntity) as GameEntityModel;
			if (ownerOrSelfModel == null){
				ownerOrSelfModel = model;
			}
			subjects.Add(ownerOrSelfModel);
		});
	}

	// 3: GetParent
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate GetParent(Storage.GenericParameter parameter){
		int subjectId = parameter.SafeInt(0);
		return DefaultDelegation(subjectId, delegate(GameEntityModel model, List<GameEntityModel> subjects){
			GameEntityModel parentModel = StateManager.state.GetModel(model.parentEntity) as GameEntityModel;
			if (parentModel != null){
				subjects.Add(parentModel);
			}
		});
	}

#endregion


#region Custom Subjects

	// 4: GetGrabbed
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate GetGrabbed(Storage.GenericParameter parameter){
		// Read anchor options, anchor IDs and if it's a single subject
		AnyOrAllOptions anchorOptions = (AnyOrAllOptions)parameter.SafeInt(1);
		int[] anchorIds = parameter.SafeIntsList(0);
		int subjectId = parameter.SafeInt(0);

		return DefaultDelegation(subjectId, delegate(GameEntityModel model, List<GameEntityModel> subjects){
			List<ModelReference> anchoredEntities = model.anchoredEntities;
			if (anchorOptions == AnyOrAllOptions.anyOf) {
				// Add all from given anchors
				foreach (int anchorId in anchorIds) {
					if (anchorId >= 0 && anchorId < anchoredEntities.Count) {
						subjects.Add(StateManager.state.GetModel(anchoredEntities[anchorId]) as GameEntityModel);
					}
				}
			}else{
				// Add all but the given anchors
				for (int anchorId = 0 ; anchorId < anchoredEntities.Count ; ++anchorId) {
					if (!anchorIds.Contains(anchorId)) {
						subjects.Add(StateManager.state.GetModel(anchoredEntities[anchorId]) as GameEntityModel);
					}
				}
			}
		});
	}

	// 5: GetHitters
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate GetHitters(Storage.GenericParameter parameter){
		// Read orientation options, types options, list of types, collision ids options, collision ids list
		OrientationOptions orientationOptions = (OrientationOptions)parameter.SafeInt(1);
		AnyOrAllOptions typesOptions = (AnyOrAllOptions)parameter.SafeInt(2);
		int[] types = parameter.SafeIntsList(0);
		AnyOrAllOptions collisionIdsOptions = (AnyOrAllOptions)parameter.SafeInt(3);
		int[] collisionIds = parameter.SafeIntsList(1);
		int subjectId = parameter.SafeInt(0);

		return DefaultDelegation(subjectId, delegate(GameEntityModel model, List<GameEntityModel> subjects){
			GameEntityController controller = model.Controller() as GameEntityController;
			List<HitInformation> hurts = controller.lastHurts;
			GameEntityModel subject;
			foreach (HitInformation hitInformation in hurts){
				if (isHitConformingType(hitInformation, typesOptions, types)
					&& isHitConformingCollisionId(hitInformation, collisionIdsOptions, collisionIds)
				){
					subject = getHitEntityIfConformingOrientationOptions(hitInformation, orientationOptions, model);
					if (subject != null) {
						subjects.Add(subject);
					}
				}
			}
		});
			
	}

	// 6: GetHittens
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate GetHittens(Storage.GenericParameter parameter){
		// Read types options, list of types, hit ids options, hit ids list
		AnyOrAllOptions typesOptions = (AnyOrAllOptions)parameter.SafeInt(1);
		int[] types = parameter.SafeIntsList(0);
		AnyOrAllOptions hitIdsOptions = (AnyOrAllOptions)parameter.SafeInt(2);
		int[] hitIds = parameter.SafeIntsList(1);
		int subjectId = parameter.SafeInt(0);

		return DefaultDelegation(subjectId, delegate(GameEntityModel model, List<GameEntityModel> subjects){
			GameEntityController controller = model.Controller() as GameEntityController;
			List<HitInformation> hits = controller.lastHits;
			GameEntityModel subject;
			foreach (HitInformation hitInformation in hits){
				if (isHitConformingType(hitInformation, typesOptions, types)
					&& isHitConformingHitId(hitInformation, hitIdsOptions, hitIds)
				){
					subject = StateManager.state.GetModel(hitInformation.entityId) as GameEntityModel;
					subjects.Add(subject);
				}
			}
		});
	}

	// 7: GetColliding
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate GetColliding(Storage.GenericParameter parameter){
		int subjectId = parameter.SafeInt(0);
		return DefaultDelegation(subjectId, delegate(GameEntityModel model, List<GameEntityModel> subjects){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller.lastCollisionEntityId != null && controller.lastCollisionEntityId != ModelReference.InvalidModelIndex){
				GameEntityModel collidingEntity = StateManager.state.GetModel(controller.lastCollisionEntityId) as GameEntityModel;
				subjects.Add(collidingEntity);
			}
		});
	}

	// 8: GetAll
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate GetAll(Storage.GenericParameter parameter){
		return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
			List<GameEntityModel> subjects = new List<GameEntityModel>();
			GameEntityModel subject;
			WorldModel world = StateManager.state.MainModel as WorldModel;
			TeamsManagerModel teamsModel = StateManager.state.GetModel(world.teamsModelId) as TeamsManagerModel;
			for (int i = 0 ; i < teamsModel.teams.Length ; ++i) {
				TeamData teamData = teamsModel.teams[i];
				if (teamData != null && teamData.entities != null){
					foreach (ModelReference modelRef in teamData.entities) {
						subject = StateManager.state.GetModel(modelRef) as GameEntityModel;
						if (subject != null) {
							subjects.Add(subject);
						}
					}
				}
			}
			return subjects;
		};
	}

#endregion




#region Auxiliar HitInformation functions


	// Filter hit by types list
	private static bool isHitConformingType(HitInformation hit, AnyOrAllOptions typesOptions, int[] types){
		if (typesOptions == AnyOrAllOptions.anyOf){
			return types.Contains((int)hit.hitData.type);
		} else{
			return !types.Contains((int)hit.hitData.type);
		}
	}

	// Filter hit by collision IDs list
	private static bool isHitConformingCollisionId(HitInformation hit, AnyOrAllOptions collisionIdsOptions, int[] collisionIds){
		if (collisionIdsOptions == AnyOrAllOptions.anyOf){
			return collisionIds.Contains(hit.collisionId);
		} else{
			return !collisionIds.Contains(hit.collisionId);
		}
	}

	// Filter hit by hit IDs list
	private static bool isHitConformingHitId(HitInformation hit, AnyOrAllOptions hitIdsOptions, int[] hitIds){
		if (hitIdsOptions == AnyOrAllOptions.anyOf){
			return hitIds.Contains(hit.hitData.hitboxID);
		} else{
			return !hitIds.Contains(hit.hitData.hitboxID);
		}
	}

	// Filter hit by hitter entity location
	private static GameEntityModel getHitEntityIfConformingOrientationOptions(HitInformation hit, OrientationOptions orientationOptions, GameEntityModel model){
		if (orientationOptions == OrientationOptions.any) return null;
		GameEntityModel hitterModel = StateManager.state.GetModel(hit.entityId) as GameEntityModel;

		PhysicPointModel modelPoint = GameEntityController.GetPointModel(model);
		PhysicPointModel hitterPoint = GameEntityController.GetPointModel(hitterModel);
		bool isFrontal;
		if (model.IsFacingRight()) isFrontal = hitterPoint.position.X >= modelPoint.position.X;
		else isFrontal = hitterPoint.position.X <= modelPoint.position.X;
		if (isFrontal == (orientationOptions == OrientationOptions.fromFront)){
			return hitterModel;
		}
		return null;
	}

#endregion


}

}
