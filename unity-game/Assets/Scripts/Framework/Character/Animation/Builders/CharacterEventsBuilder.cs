using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class CharacterEventsBuilder {

		// Event builders indexed by type directly on array
		private delegate GenericEvent<AnimationModel> BuilderAction(Storage.GenericParameter param);
		private static BuilderAction[] builderActions = {
			BuildSetAnimation,					// 0: 'walk'
			BuildZeroAnimationVelocity,			// 1: vel(zero)
			BuildSetAnimationVelocity,			// 2: vel(2.3, 1.5, 0.0)
			BuildZeroMaxInputVelocity,			// 3: inputVel(zero)
			BuildSetMaxInputVelocity,			// 4: inputVel(2.3, 1.5)
			BuildAddAnimationVerticalImpulse,	// 5: impulseV(1.5)
			BuildFlip,							// 6: flip
			BuildAutoFlip,						// 7: autoFlip(false)
			BuildPause,							// 8: pause(10)
			BuildIncrementCombo,				// 9: ++combo
			BuildResetCombo,					// 10: reset(combo)
			BuildInstantMove,					// 11: instantMove(2.1, 4.2, 5.3)
			BuildAnchorWithMove,				// 12: grab(hitten, 2, (2.1, 4.2, 5.3))
			BuildAnchor,						// 13: grab(hitten, 2)
			BuildReleaseAll,					// 14: release(all)
			BuildRelease,						// 15: release(2)
			BuildSetAnchoredPos,				// 16: grabbedPos(2, (2.1, 4.2, 5.3))
			BuildSetAnchoredAnim,				// 17: grabbedAnim(2, jump)
			BuildAddRefImpulse,					// 18: impulse(grabbed, 0, (2.1, 4.2, 5.3))
			BuildResetImpulse,					// 19: reset(impulse)
			BuildConsuleInput,					// 20: consumeInput(A)
			BuildGetHurt,						// 21: getHurt(10%)
			BuildSpawnEffect					// 22: spawnFX(sparks)

		};


		// The public builder method
		public static GenericEvent<AnimationModel> Build(Storage.Character charData, int[] eventIds){
			List<GenericEvent<AnimationModel>> events = new List<GenericEvent<AnimationModel>>(eventIds.Length);
			GenericEvent<AnimationModel> e;
			foreach (int eventId in eventIds) {
				e = BuildFromParameter(charData.genericParameters[eventId]);
				if (e != null) {
					events.Add(e);
				}
			}
			if (events.Count > 0) {
				if (events.Count == 1) {
					return events[0];
				}
				return new EventsList<AnimationModel>(null, events);
			}
			return null;
		}


		// Build a single event
		private static GenericEvent<AnimationModel> BuildFromParameter(Storage.GenericParameter parameter){
			int callIndex = parameter.type;
			if (callIndex < builderActions.Length) {
				return builderActions[callIndex](parameter);
			}
			Debug.Log("CharacterEventsBuilder: Unknown event type: " + parameter.type);
			return null;
		}



		// Build a point
		static FixedVector3 BuildFixedVector3(Storage.GenericParameter parameter, int startFloatIndex = 0){
			FixedFloat x = parameter.SafeFloat(startFloatIndex);
			FixedFloat y = parameter.SafeFloat(startFloatIndex + 1);
			FixedFloat z = parameter.SafeFloat(startFloatIndex + 2);
			return new FixedVector3(x, y, z);
		}


		// Build an entity delegator
		static GameEntityReferenceDelegator BuildEntityDelegator(Storage.GenericParameter parameter, int startIntIndex = 0){
			EntityDelegatorType delegatorType = (EntityDelegatorType)parameter.SafeInt(startIntIndex);
			switch (delegatorType){
				case EntityDelegatorType.anchor:{
					return new AnchoredEntityDelegator(parameter.SafeInt(startIntIndex + 1));
				}
				case EntityDelegatorType.parent:{
					return new ParentEntityDelegator();
				}
				case EntityDelegatorType.collision:{
					return new CollisionEntityDelegator();
				}
				case EntityDelegatorType.hitten:{
					return new HittenEntityDelegator();
				}
				case EntityDelegatorType.hitter:{
					return new HitterEntityDelegator();
				}
			}
			return null;
		}


#region Events


		// 'walk'
		private static GenericEvent<AnimationModel> BuildSetAnimation(Storage.GenericParameter parameter){
			return new AnimationTransitionEvent(null, parameter.SafeString(0), (float) parameter.SafeFloat(0), (uint)parameter.SafeInt(0));
		}

		// vel(zero)
		private static GenericEvent<AnimationModel> BuildZeroAnimationVelocity(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityPhysicsOperations.SetAnimationVelocity,
				FixedVector3.Zero
			);
		}

		// vel(2.3, 1.5, 0.0)
		private static GenericEvent<AnimationModel> BuildSetAnimationVelocity(Storage.GenericParameter parameter){
			FixedVector3 vel = BuildFixedVector3(parameter);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null, GameEntityPhysicsOperations.SetAnimationVelocity, vel
			);
		}


		// inputVel(zero)
		private static GenericEvent<AnimationModel> BuildZeroMaxInputVelocity(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityPhysicsOperations.SetMaxInputVelocity,
				FixedVector3.Zero
			);
		}

		// inputVel(2.3, 1.5, 0.0)
		private static GenericEvent<AnimationModel> BuildSetMaxInputVelocity(Storage.GenericParameter parameter){
			FixedFloat velx = parameter.SafeFloat(0);
			FixedFloat velz = parameter.SafeFloat(1);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityPhysicsOperations.SetMaxInputVelocity,
				new FixedVector3(velx, 0, velz)
			);
		}


		// impulseV(1.5)
		private static GenericEvent<AnimationModel> BuildAddAnimationVerticalImpulse(Storage.GenericParameter parameter){
			FixedFloat vely = parameter.SafeFloat(0);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityPhysicsOperations.AddImpulse,
				new FixedVector3(0, vely, 0)
			);
		}


		// flip
		private static GenericEvent<AnimationModel> BuildFlip(Storage.GenericParameter parameter){
			return new SimpleEntityAnimationEvent(null, GameEntityController.Flip);
		}

		// autoFlip(false)
		private static GenericEvent<AnimationModel> BuildAutoFlip(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<bool>(null, GameEntityController.SetAutomaticFlip, parameter.SafeBool(0));
		}

		// pause(10)
		private static GenericEvent<AnimationModel> BuildPause(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<int>(null, GameEntityController.PausePhysics, parameter.SafeInt(0));
		}

		// ++combo
		private static GenericEvent<AnimationModel> BuildIncrementCombo(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<int>(null,
				delegate(GameEntityModel model, int comboTimer){
					// Do not increment combo more than once per animation
					int comboAnimFlag;
					model.customVariables.TryGetValue(CharacterConditionsBuilder.comboAnimationClearFlag, out comboAnimFlag);
					if (comboAnimFlag == 0) {
						int currentCombo;
						model.customVariables.TryGetValue(CharacterConditionsBuilder.comboCustomVariableName, out currentCombo);
						model.customVariables[CharacterConditionsBuilder.comboCustomVariableName] = currentCombo + 1;
						model.customTimers[CharacterConditionsBuilder.comboCustomVariableName] = comboTimer;
						model.customVariables[CharacterConditionsBuilder.comboAnimationClearFlag] = 1;
					}
				},
				parameter.SafeInt(0));
		}

		// reset(combo)
		private static GenericEvent<AnimationModel> BuildResetCombo(Storage.GenericParameter parameter){
			return new SimpleEntityAnimationEvent(
				null,
				delegate(GameEntityModel model){
					model.customVariables[CharacterConditionsBuilder.comboCustomVariableName] = 0;
					model.customTimers[CharacterConditionsBuilder.comboCustomVariableName] = 0;
				}
			);
		}



		// instantMove(2.1, 4.2, 5.3)
		private static GenericEvent<AnimationModel> BuildInstantMove(Storage.GenericParameter parameter){
			FixedVector3 vel = BuildFixedVector3(parameter);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null, GameEntityPhysicsOperations.MoveEntity, vel
			);
		}


	#region Anchoring


		// grab(hitten, 2, (2.1, 4.2, 5.3))
		private static GenericEvent<AnimationModel> BuildAnchorWithMove(Storage.GenericParameter parameter){

			GameEntityReferenceDelegator delegator = BuildEntityDelegator(parameter);
			int anchorId = parameter.SafeInt(2);
			FixedVector3 movement = BuildFixedVector3(parameter);
			return new TrippleEntityAnimationEvent<GameEntityReferenceDelegator, int, FixedVector3>(
				null, GameEntityAnchoringOperations.AnchorEntity,
				delegator, anchorId, movement
			);

		}

		// grab(hitten, 2)
		private static GenericEvent<AnimationModel> BuildAnchor(Storage.GenericParameter parameter){
			GameEntityReferenceDelegator delegator = BuildEntityDelegator(parameter);
			int anchorId = parameter.SafeInt(2);
			return new DoubleEntityAnimationEvent<GameEntityReferenceDelegator, int>(
				null, GameEntityAnchoringOperations.AnchorEntity, delegator, anchorId
			);
		}

		// release(all)
		private static GenericEvent<AnimationModel> BuildReleaseAll(Storage.GenericParameter parameter){
			return new SimpleEntityAnimationEvent(null, GameEntityAnchoringOperations.ReleaseAllAnchoredEntities);
		}

		// release(2)
		private static GenericEvent<AnimationModel> BuildRelease(Storage.GenericParameter parameter){
			int anchorId = parameter.SafeInt(0);
			return new SingleEntityAnimationEvent<int>(null, GameEntityAnchoringOperations.ReleaseAnchoredEntity, anchorId);
		}

		// grabbedPos(2, (2.1, 4.2, 5.3))
		private static GenericEvent<AnimationModel> BuildSetAnchoredPos(Storage.GenericParameter parameter){
			int anchorId = parameter.SafeInt(0);
			FixedVector3 pos = BuildFixedVector3(parameter);
			return new DoubleEntityAnimationEvent<int, FixedVector3>(
				null, GameEntityAnchoringOperations.SetAnchoredEntityRelativePosition,
				anchorId, pos
			);
		}


		// grabbedAnim(2, jump)
		private static GenericEvent<AnimationModel> BuildSetAnchoredAnim(Storage.GenericParameter parameter){
			int anchorId = parameter.SafeInt(0);
			string animName = parameter.SafeString(0);
			return new DoubleEntityAnimationEvent<int, string>(
				null, GameEntityAnchoringOperations.SetAnchoredEntityAnimation,
				anchorId, animName
			);
		}


	#endregion


		// impulse(grabbed, (2.1, 4.2, 5.3))
		private static GenericEvent<AnimationModel> BuildAddRefImpulse(Storage.GenericParameter parameter){
			GameEntityReferenceDelegator delegator = BuildEntityDelegator(parameter);
			FixedVector3 impulse = BuildFixedVector3(parameter);
			return new DoubleEntityAnimationEvent<GameEntityReferenceDelegator, FixedVector3>(
				null, GameEntityPhysicsOperations.AddImpulse,
				delegator, impulse
			);
		}

		// reset(impulse)
		private static GenericEvent<AnimationModel> BuildResetImpulse(Storage.GenericParameter parameter){
			return new SimpleEntityAnimationEvent(
				null, GameEntityPhysicsOperations.ResetPlanarImpulse
			);
		}

		// consumeInput(A)
		private static GenericEvent<AnimationModel> BuildConsuleInput(Storage.GenericParameter parameter){
			bool isRelease = parameter.SafeBool(0);
			uint buttonId = (uint) parameter.SafeInt(0);
			if (isRelease){
				return new SingleEvent<AnimationModel, uint>(null, InputButtonOperations.ConsumeRelease, buttonId);
			}else{
				return new SingleEvent<AnimationModel, uint>(null, InputButtonOperations.ConsumePress, buttonId);
			}
		}

		// getHurt(10%)
		private static GenericEvent<AnimationModel> BuildGetHurt(Storage.GenericParameter parameter){
			FixedFloat damagePercentage = parameter.SafeInt(0) * 0.01f;
			int facingOptions = parameter.SafeInt(1);
			facingOptions -= 1;
			if (facingOptions < 0) {
				// inherit from hitter data
				return new SingleEntityAnimationEvent<FixedFloat>(
					null,
					GameEntityController.HurtBasedOnHitData,
					damagePercentage
				);
			} else {
				return new DoubleEntityAnimationEvent<HitData.HitFacing, FixedFloat>(
					null,
					GameEntityController.HurtBasedOnFacingOptions,
					(HitData.HitFacing)facingOptions,
					damagePercentage
				);
			}
		}

		private static GenericEvent<AnimationModel> BuildSpawnEffect(Storage.GenericParameter parameter){
			FixedVector3 offset = BuildFixedVector3(parameter);
			string prefabName = parameter.SafeString(0);
			int locationType = parameter.SafeInt(0);
			int lifetime = parameter.SafeInt(1);
			bool localSpace = parameter.SafeBool(0);

			// {"self", "anchor", "hit intersection", "hurt intersection"}
			PentaEntityAnimationEvent<string, int, FixedVector3, bool, GameEntityView.ConvertGameToViewCoordinates>.EventExecutionDelegate theDelegate = null;
			switch (locationType) {
				case 0:
					// self
					theDelegate = GameEntityView.SpawnAtSelf;
					break;
				case 1:
					// Anchor, TODO: which anchor?
					RetroBread.Debug.LogError("Spawn at anchor not supported yet");
					break;
				case 2: 
					// hit
					theDelegate = GameEntityView.SpawnAtHitIntersection;
					break;
				case 3:
					// hurt
					theDelegate = GameEntityView.SpawnAtHurtIntersection;
					break;
			}

			return new PentaEntityAnimationEvent<string, int, FixedVector3, bool, GameEntityView.ConvertGameToViewCoordinates>(
				null,
				theDelegate,
				prefabName,
				lifetime,
				offset,
				localSpace,
				PhysicPoint2DView.ConvertGameToViewCoordinates // TODO: store this delegate at a setup file, so it's easier to configure
			);

		}


#endregion


	}

}
