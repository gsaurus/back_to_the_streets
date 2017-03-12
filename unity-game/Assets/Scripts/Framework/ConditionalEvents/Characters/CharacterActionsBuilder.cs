using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class CharacterActionsBuilder {

		// Action execution delegate builders indexed by type directly on array
		private delegate EventAction<GameEntityModel>.ExecutionDelegate BuilderAction(Storage.GenericParameter param);
		private static BuilderAction[] builderActions = {
			BuildConsumeInput,
			BuildSetAnimation,
			BuildSetDeltaPosition,
			BuildSetVelocity,
			BuildAddInpulse,
			BuildSetMaxInputVelocity,
			BuildFlip,
			BuildSetAutoFlip,
			BuildPausePhysics,
			BuildSetVariable,
			BuildSetGlobalVariable,
			BuildGrab,
			BuildReleaseGrab,
			BuildGetHurt,
			BuildOwnSubject,
			BuildReleaseOwnership,
			BuildSpawnEntity,
			BuildSpawnEffect,
			BuildDestroy
		};


		// The public builder method
		public static List<EventAction<GameEntityModel>> Build(Storage.Character charData, int[] eventIds){
			List<EventAction<GameEntityModel>> actions = new List<EventAction<GameEntityModel>>(eventIds.Length);
			EventAction<GameEntityModel> action;
			foreach (int eventId in eventIds) {
				action = BuildFromParameter(charData.genericParameters[eventId]);
				if (action != null) {
					actions.Add(action);
				}
			}
			return actions;
		}


		// Build a single event
		private static EventAction<GameEntityModel> BuildFromParameter(Storage.GenericParameter parameter){
			EventAction<GameEntityModel>.ExecutionDelegate executionDelegate;
			int callIndex = parameter.type;
			int subjectId;
			if (callIndex < builderActions.Length) {
				executionDelegate = builderActions[callIndex](parameter);
				subjectId = GetSubjectId(parameter);
				return new EventAction<GameEntityModel>(executionDelegate, subjectId);
			}
			Debug.Log("CharacterActionsBuilder: Unknown condition type: " + parameter.type);
			return null;
		}


		// Get subjectId from parameter
		private static int GetSubjectId(Storage.GenericParameter parameter) {
			return parameter.SafeInt(0);
		}


		// Build a point
		static FixedVector3 BuildFixedVector3(Storage.GenericParameter parameter, int startFloatIndex = 0){
			FixedFloat x = parameter.SafeFloat(startFloatIndex);
			FixedFloat y = parameter.SafeFloat(startFloatIndex + 1);
			FixedFloat z = parameter.SafeFloat(startFloatIndex + 2);
			return new FixedVector3(x, y, z);
		}


	#region Events


		private static EventAction<GameEntityModel>.ExecutionDelegate BuildConsumeInput(Storage.GenericParameter parameter){
			int buttonId = parameter.SafeInt(1);
			bool consumeRelease = parameter.SafeBool(0);

			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				PlayerInputModel inputModel = StateManager.state.GetModel(mainModel.inputModelId) as PlayerInputModel;
				if (inputModel == null) return;
				PlayerInputController inputController = inputModel.Controller() as PlayerInputController;
				if (inputController == null) return;
				if (consumeRelease) {
					inputController.ConsumeRelease(inputModel, buttonId);
				}else{
					inputController.ConsumePress(inputModel, buttonId);
				}
			};
		}


		// 'walk'
		private static EventAction<GameEntityModel>.ExecutionDelegate BuildSetAnimation(Storage.GenericParameter parameter){
			string animationName = parameter.SafeString(0);
			FixedFloat transitionTime = parameter.SafeFloat(0);

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
				AnimationModel animModel = StateManager.state.GetModel(model.animationModelId) as AnimationModel;
				if (animModel == null) return;
				animModel.SetNextAnimation(animationName, 0);
				AnimationView view = animModel.View() as AnimationView;
				if (view != null){
					view.transitionTime = transitionTime;
				}
			};
		}


		// pos(2.1, 4.2, 5.3)
		private static EventAction<GameEntityModel>.ExecutionDelegate BuildSetDeltaPosition(Storage.GenericParameter parameter){
			int relativeSubjectId = parameter.SafeInt(1);
			FixedVector3 deltaPos = BuildFixedVector3(parameter, 0);

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
				List<GameEntityModel> refSubjects = ConditionUtils<GameEntityModel>.GetNonEmptySubjectOrNil(subjectModels, relativeSubjectId);
				if (refSubjects == null || refSubjects.Count == 0) return;
				// Select one randomly
				GameEntityModel refModel = refSubjects[StateManager.state.Random.NextInt(0, refSubjects.Count-1)];
				FixedFloat realDelta;
				if (!refModel.IsFacingRight()) deltaPos.X *= -1;
				PhysicPointModel mainPoint = StateManager.state.GetModel(model.physicsModelId) as PhysicPointModel;
				if (mainPoint == null) return;
				if (refModel != model){
					PhysicPointModel refPoint = StateManager.state.GetModel(refModel.physicsModelId) as PhysicPointModel;
					if (refPoint == null) return;
					realDelta = (refPoint + deltaPos) - mainPoint;
				}else{
					realDelta = deltaPos;
				}
					
				PhysicPointController pointController = mainPoint.Controller() as PhysicPointController;
				if (pointController == null) return;
				pointController.SetVelocityAffector(mainPoint, PhysicPointController.setPositionffectorName, realDelta);
			};
		}



		private static EventAction<GameEntityModel>.ExecutionDelegate BuildSetVelocity(Storage.GenericParameter parameter){
			int mask = parameter.SafeInt(1);
			FixedVector3 velocity = BuildFixedVector3(parameter, 0);
			bool hasX = mask == 0 || mask == 3 || mask == 4 || mask == 6;
			bool hasY = mask == 1 || mask == 3 || mask == 5 || mask == 6;
			bool hasZ = mask == 2 || mask == 4 || mask == 5 || mask == 6;

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
				PhysicPointModel pointModel = StateManager.state.GetModel(model.physicsModelId) as PhysicPointModel;
				if (pointModel == null) return;
				FixedVector3 originalVelocity = pointModel.GetVelocity();
				FixedVector3 finalVelocity = new FixedVector3(
					hasX ? velocity.X : originalVelocity.X,
					hasY ? velocity.Y : originalVelocity.Y,
					hasZ ? velocity.Z : originalVelocity.Z
				);
				pointModel.velocityAffectors[GameEntityController.animVelocityAffector] = finalVelocity;
			};
		}



		private static EventAction<GameEntityModel>.ExecutionDelegate BuildAddInpulse(Storage.GenericParameter parameter){
			FixedVector3 impulse = BuildFixedVector3(parameter, 0);
			bool asPercentage = parameter.SafeBool(0);
			if (asPercentage){
				impulse /= 100;
			}

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
				PhysicPointModel pointModel = StateManager.state.GetModel(model.physicsModelId) as PhysicPointModel;
				if (pointModel == null) return;
				FixedVector3 finalImpulse = impulse;
				if (asPercentage) {
					finalImpulse *= pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName];
				}
				pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] += finalImpulse;
			};
		}

	

		private static EventAction<GameEntityModel>.ExecutionDelegate BuildSetMaxInputVelocity(Storage.GenericParameter parameter){
			FixedFloat maxX = parameter.SafeFloat(0);
			FixedFloat maxZ = parameter.SafeFloat(1);

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
				model.maxInputVelocity = new FixedVector3(maxX, 0, maxZ);
			};
		}



		private static EventAction<GameEntityModel>.ExecutionDelegate BuildFlip(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
				GameEntityController.Flip(model);
			};
		}



		private static EventAction<GameEntityModel>.ExecutionDelegate BuildSetAutoFlip(Storage.GenericParameter parameter){
			bool autoValue = parameter.SafeBool(0);

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
				GameEntityController.SetAutomaticFlip(model, autoValue);
			};
		}



		private static EventAction<GameEntityModel>.ExecutionDelegate BuildPausePhysics(Storage.GenericParameter parameter){
			int numeratorSubjectId = parameter.SafeInt(1);
			string numeratorVariableName = parameter.SafeString(0);
			int defaultValue = parameter.SafeInt(2);
			FixedFloat percentage = (numeratorSubjectId == 0 || defaultValue == 0) ? 1 : defaultValue / new FixedFloat(100);

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
				int numeratorValue = GetNumeratorValue(model, numeratorSubjectId, numeratorVariableName, defaultValue, subjectModels);
				int pauseValue = numeratorValue * percentage;
				if (pauseValue > 0) {
					GameEntityController.PausePhysics(model, pauseValue);
				}
			};
		}



		private static EventAction<GameEntityModel>.ExecutionDelegate BuildSetVariable(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){

			};
		}
			


		private static EventAction<GameEntityModel>.ExecutionDelegate BuildSetGlobalVariable(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){

			};
		}


	
		private static EventAction<GameEntityModel>.ExecutionDelegate BuildGrab(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){

			};
		}



		private static EventAction<GameEntityModel>.ExecutionDelegate BuildReleaseGrab(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){

			};
		}



		private static EventAction<GameEntityModel>.ExecutionDelegate BuildGetHurt(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){

			};
		}
			


		private static EventAction<GameEntityModel>.ExecutionDelegate BuildOwnSubject(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){

			};
		}



		private static EventAction<GameEntityModel>.ExecutionDelegate BuildReleaseOwnership(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){

			};
		}
			


		private static EventAction<GameEntityModel>.ExecutionDelegate BuildSpawnEntity(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){

			};
		}

	

		private static EventAction<GameEntityModel>.ExecutionDelegate BuildSpawnEffect(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){

			};
		}



		private static EventAction<GameEntityModel>.ExecutionDelegate BuildDestroy(Storage.GenericParameter parameter){

			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){

			};
		}





	//	// pause(10)
	//	private static GenericEvent<GameEntityModel> BuildPause(Storage.GenericParameter parameter){
	//		return new SingleEntityAnimationEvent<int>(null, GameEntityController.PausePhysics, parameter.SafeInt(0));
	//	}
	//
	//	// ++combo
	//	private static GenericEvent<GameEntityModel> BuildIncrementCombo(Storage.GenericParameter parameter){
	//		return new SingleEntityAnimationEvent<int>(null,
	//			delegate(GameEntityModel model, int comboTimer){
	//				// Do not increment combo more than once per animation
	//				int comboAnimFlag;
	//				model.customVariables.TryGetValue(CharacterConditionsBuilder.comboAnimationClearFlag, out comboAnimFlag);
	//				if (comboAnimFlag == 0) {
	//					int currentCombo;
	//					model.customVariables.TryGetValue(CharacterConditionsBuilder.comboCustomVariableName, out currentCombo);
	//					model.customVariables[CharacterConditionsBuilder.comboCustomVariableName] = currentCombo + 1;
	//					model.customTimers[CharacterConditionsBuilder.comboCustomVariableName] = comboTimer;
	//					model.customVariables[CharacterConditionsBuilder.comboAnimationClearFlag] = 1;
	//				}
	//			},
	//			parameter.SafeInt(0));
	//	}
	//
	//	// reset(combo)
	//	private static GenericEvent<GameEntityModel> BuildResetCombo(Storage.GenericParameter parameter){
	//		return new SimpleEntityAnimationEvent(
	//			null,
	//			delegate(GameEntityModel model){
	//				model.customVariables[CharacterConditionsBuilder.comboCustomVariableName] = 0;
	//				model.customTimers[CharacterConditionsBuilder.comboCustomVariableName] = 0;
	//			}
	//		);
	//	}
	//
	//
	//
	//	// instantMove(2.1, 4.2, 5.3)
	//	private static GenericEvent<GameEntityModel> BuildInstantMove(Storage.GenericParameter parameter){
	//		FixedVector3 vel = BuildFixedVector3(parameter);
	//		return new SingleEntityAnimationEvent<FixedVector3>(
	//			null, GameEntityPhysicsOperations.MoveEntity, vel
	//		);
	//	}
	//
	//
	//#region Anchoring
	//
	//
	//	// grab(hitten, 2, (2.1, 4.2, 5.3))
	//	private static GenericEvent<GameEntityModel> BuildAnchorWithMove(Storage.GenericParameter parameter){
	//
	//		GameEntityReferenceDelegator delegator = BuildEntityDelegator(parameter);
	//		int anchorId = parameter.SafeInt(2);
	//		FixedVector3 movement = BuildFixedVector3(parameter);
	//		return new TrippleEntityAnimationEvent<GameEntityReferenceDelegator, int, FixedVector3>(
	//			null, GameEntityAnchoringOperations.AnchorEntity,
	//			delegator, anchorId, movement
	//		);
	//
	//	}
	//
	//	// grab(hitten, 2)
	//	private static GenericEvent<GameEntityModel> BuildAnchor(Storage.GenericParameter parameter){
	//		GameEntityReferenceDelegator delegator = BuildEntityDelegator(parameter);
	//		int anchorId = parameter.SafeInt(2);
	//		return new DoubleEntityAnimationEvent<GameEntityReferenceDelegator, int>(
	//			null, GameEntityAnchoringOperations.AnchorEntity, delegator, anchorId
	//		);
	//	}
	//
	//	// release(all)
	//	private static GenericEvent<GameEntityModel> BuildReleaseAll(Storage.GenericParameter parameter){
	//		return new SimpleEntityAnimationEvent(null, GameEntityAnchoringOperations.ReleaseAllAnchoredEntities);
	//	}
	//
	//	// release(2)
	//	private static GenericEvent<GameEntityModel> BuildRelease(Storage.GenericParameter parameter){
	//		int anchorId = parameter.SafeInt(0);
	//		return new SingleEntityAnimationEvent<int>(null, GameEntityAnchoringOperations.ReleaseAnchoredEntity, anchorId);
	//	}
	//
	//	// grabbedPos(2, (2.1, 4.2, 5.3))
	//	private static GenericEvent<GameEntityModel> BuildSetAnchoredPos(Storage.GenericParameter parameter){
	//		int anchorId = parameter.SafeInt(0);
	//		FixedVector3 pos = BuildFixedVector3(parameter);
	//		return new DoubleEntityAnimationEvent<int, FixedVector3>(
	//			null, GameEntityAnchoringOperations.SetAnchoredEntityRelativePosition,
	//			anchorId, pos
	//		);
	//	}
	//
	//
	//	// grabbedAnim(2, jump)
	//	private static GenericEvent<GameEntityModel> BuildSetAnchoredAnim(Storage.GenericParameter parameter){
	//		int anchorId = parameter.SafeInt(0);
	//		string animName = parameter.SafeString(0);
	//		return new DoubleEntityAnimationEvent<int, string>(
	//			null, GameEntityAnchoringOperations.SetAnchoredEntityAnimation,
	//			anchorId, animName
	//		);
	//	}
	//
	//
	//#endregion
	//
	//
	//	// impulse(grabbed, (2.1, 4.2, 5.3))
	//	private static GenericEvent<GameEntityModel> BuildAddRefImpulse(Storage.GenericParameter parameter){
	//		GameEntityReferenceDelegator delegator = BuildEntityDelegator(parameter);
	//		FixedVector3 impulse = BuildFixedVector3(parameter);
	//		return new DoubleEntityAnimationEvent<GameEntityReferenceDelegator, FixedVector3>(
	//			null, GameEntityPhysicsOperations.AddImpulse,
	//			delegator, impulse
	//		);
	//	}
	//
	//	// reset(impulse)
	//	private static GenericEvent<GameEntityModel> BuildResetImpulse(Storage.GenericParameter parameter){
	//		return new SimpleEntityAnimationEvent(
	//			null, GameEntityPhysicsOperations.ResetPlanarImpulse
	//		);
	//	}
	//
	//	// consumeInput(A)
	//	private static GenericEvent<GameEntityModel> BuildConsuleInput(Storage.GenericParameter parameter){
	//		bool isRelease = parameter.SafeBool(0);
	//		uint buttonId = (uint) parameter.SafeInt(0);
	//		if (isRelease){
	//			return new SingleEvent<AnimationModel, uint>(null, InputButtonOperations.ConsumeRelease, buttonId);
	//		}else{
	//			return new SingleEvent<AnimationModel, uint>(null, InputButtonOperations.ConsumePress, buttonId);
	//		}
	//	}
	//
	//	// getHurt(10%)
	//	private static GenericEvent<GameEntityModel> BuildGetHurt(Storage.GenericParameter parameter){
	//		FixedFloat damagePercentage = parameter.SafeInt(0) * 0.01f;
	//		int facingOptions = parameter.SafeInt(1);
	//		facingOptions -= 1;
	//		if (facingOptions < 0) {
	//			// inherit from hitter data
	//			return new SingleEntityAnimationEvent<FixedFloat>(
	//				null,
	//				GameEntityController.HurtBasedOnHitData,
	//				damagePercentage
	//			);
	//		} else {
	//			return new DoubleEntityAnimationEvent<HitData.HitFacing, FixedFloat>(
	//				null,
	//				GameEntityController.HurtBasedOnFacingOptions,
	//				(HitData.HitFacing)facingOptions,
	//				damagePercentage
	//			);
	//		}
	//	}
	//
	//	private static GenericEvent<GameEntityModel> BuildSpawnEffect(Storage.GenericParameter parameter){
	//		FixedVector3 offset = BuildFixedVector3(parameter);
	//		string prefabName = parameter.SafeString(0);
	//		int locationType = parameter.SafeInt(0);
	//		int lifetime = parameter.SafeInt(1);
	//		bool localSpace = parameter.SafeBool(0);
	//
	//		// {"self", "anchor", "hit intersection", "hurt intersection"}
	//		PentaEntityAnimationEvent<string, int, FixedVector3, bool, GameEntityView.ConvertGameToViewCoordinates>.EventExecutionDelegate theDelegate = null;
	//		switch (locationType) {
	//			case 0:
	//				// self
	//				theDelegate = GameEntityView.SpawnAtSelf;
	//				break;
	//			case 1:
	//				// Anchor, TODO: which anchor?
	//				RetroBread.Debug.LogError("Spawn at anchor not supported yet");
	//				break;
	//			case 2: 
	//				// hit
	//				theDelegate = GameEntityView.SpawnAtHitIntersection;
	//				break;
	//			case 3:
	//				// hurt
	//				theDelegate = GameEntityView.SpawnAtHurtIntersection;
	//				break;
	//		}
	//
	//		return new PentaEntityAnimationEvent<string, int, FixedVector3, bool, GameEntityView.ConvertGameToViewCoordinates>(
	//			null,
	//			theDelegate,
	//			prefabName,
	//			lifetime,
	//			offset,
	//			localSpace,
	//			PhysicPoint2DView.ConvertGameToViewCoordinates // TODO: store this delegate at a setup file, so it's easier to configure
	//		);
	//
	//	}
	//
	//
	//	// own(anchored, 2)
	//	private static GenericEvent<GameEntityModel> BuildOwnEntity(Storage.GenericParameter parameter){
	//		GameEntityReferenceDelegator delegator = BuildEntityDelegator(parameter);
	//		return new SingleEntityAnimationEvent<GameEntityReferenceDelegator>(
	//			null, GameEntityAnchoringOperations.OwnEntity, delegator
	//		);
	//	}
	//
	//	// releaseOwnership
	//	private static GenericEvent<GameEntityModel> BuildReleaseOwnership(Storage.GenericParameter parameter){
	//		return new SimpleEntityAnimationEvent(null, GameEntityAnchoringOperations.ReleaseOwnership);
	//	}


	#endregion



	#region Auxiliar Functions


		// CompareWithNumerator, int version
		// Comes in duplicate due to conversion FixedFloat to int not working with parameterized types
		// TODO; extract this method to some utils, as it's used by CharacterConditionsBuilder, adapted
		private static int GetNumeratorValue(
			GameEntityModel mainModel,
			int numeratorSubjectId,
			string numeratorSubjectVarName,
			int staticValue,
			List<GameEntityModel>[] subjectModels
		){
			// no subject
			if (numeratorSubjectId == 0){
				return staticValue;
			}
			// global variable
			if (numeratorSubjectId == 1){

				// Global variable may have references to a team ID, character name, player number, etc
				numeratorSubjectVarName = CharacterConditionsBuilder.ParseVariableValuesInGlobalName(mainModel, numeratorSubjectVarName);

				int globalVariableValue = 0;
				WorldModel worldModel = StateManager.state.MainModel as WorldModel;
				worldModel.globalVariables.TryGetValue(numeratorSubjectVarName, out globalVariableValue);
				return globalVariableValue;
			}
			// subject variable
			numeratorSubjectId -= 2;
			List<GameEntityModel> numeratorSubjects = ConditionUtils<GameEntityModel>.GetNonEmptySubjectOrNil(subjectModels, numeratorSubjectId);
			if (numeratorSubjects == null || numeratorSubjects.Count == 0) return 0;
			// need only one value
			int variableValue;
			foreach (GameEntityModel comparisonModel in numeratorSubjects) {
				if (CharacterConditionsBuilder.TryGetVariableValue(comparisonModel, numeratorSubjectVarName, out variableValue)) {
					return variableValue;
				}
			}
			return 0;
		}


	#endregion


	}

}
