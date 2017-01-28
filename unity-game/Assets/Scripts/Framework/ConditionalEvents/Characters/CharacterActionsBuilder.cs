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


	// 'walk'
	private static EventAction<GameEntityModel>.ExecutionDelegate BuildSetAnimation(Storage.GenericParameter parameter){
		// TODO
		return null;
	}

//	// vel(zero)
//	private static GenericEvent<GameEntityModel> BuildZeroAnimationVelocity(Storage.GenericParameter parameter){
//		return new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityPhysicsOperations.SetAnimationVelocity,
//			FixedVector3.Zero
//		);
//	}
//
//	// vel(2.3, 1.5, 0.0)
//	private static GenericEvent<GameEntityModel> BuildSetAnimationVelocity(Storage.GenericParameter parameter){
//		FixedVector3 vel = BuildFixedVector3(parameter);
//		return new SingleEntityAnimationEvent<FixedVector3>(
//			null, GameEntityPhysicsOperations.SetAnimationVelocity, vel
//		);
//	}
//
//
//	// inputVel(zero)
//	private static GenericEvent<GameEntityModel> BuildZeroMaxInputVelocity(Storage.GenericParameter parameter){
//		return new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityPhysicsOperations.SetMaxInputVelocity,
//			FixedVector3.Zero
//		);
//	}
//
//	// inputVel(2.3, 1.5, 0.0)
//	private static GenericEvent<GameEntityModel> BuildSetMaxInputVelocity(Storage.GenericParameter parameter){
//		FixedFloat velx = parameter.SafeFloat(0);
//		FixedFloat velz = parameter.SafeFloat(1);
//		return new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityPhysicsOperations.SetMaxInputVelocity,
//			new FixedVector3(velx, 0, velz)
//		);
//	}
//
//
//	// impulseV(1.5)
//	private static GenericEvent<GameEntityModel> BuildAddAnimationVerticalImpulse(Storage.GenericParameter parameter){
//		FixedFloat vely = parameter.SafeFloat(0);
//		return new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityPhysicsOperations.AddImpulse,
//			new FixedVector3(0, vely, 0)
//		);
//	}
//
//
//	// flip
//	private static GenericEvent<GameEntityModel> BuildFlip(Storage.GenericParameter parameter){
//		return new SimpleEntityAnimationEvent(null, GameEntityController.Flip);
//	}
//
//	// autoFlip(false)
//	private static GenericEvent<GameEntityModel> BuildAutoFlip(Storage.GenericParameter parameter){
//		return new SingleEntityAnimationEvent<bool>(null, GameEntityController.SetAutomaticFlip, parameter.SafeBool(0));
//	}
//
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


}

}
