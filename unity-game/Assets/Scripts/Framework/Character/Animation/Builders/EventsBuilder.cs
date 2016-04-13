using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class EventsBuilder {


		// Event builders indexed by type directly on array
		private delegate AnimationEvent BuilderAction(Storage.GenericParameter param);
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
			BuildResetImpulse					// 19: reset(impulse)

		};


		// The public builder method
		public static AnimationEvent Build(Storage.Character charData, int[] eventIds){
			List<AnimationEvent> events = new List<AnimationEvent>(eventIds.Length);
			AnimationEvent e;
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
				return new EventsList(null, events);
			}
			return null;
		}


		// Build a single event
		private static AnimationEvent BuildFromParameter(Storage.GenericParameter parameter){
			int callIndex = parameter.type;
			if (callIndex < builderActions.Length) {
				return builderActions[callIndex](parameter);
			}
			Debug.Log("EventsBuilder: Unknown event type: " + parameter.type);
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
		private static AnimationEvent BuildSetAnimation(Storage.GenericParameter parameter){
			return new AnimationTransitionEvent(null, parameter.SafeString(0), (float) parameter.SafeFloat(0), (uint)parameter.SafeInt(0));
		}

		// vel(zero)
		private static AnimationEvent BuildZeroAnimationVelocity(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityPhysicsOperations.SetAnimationVelocity,
				FixedVector3.Zero
			);
		}

		// vel(2.3, 1.5, 0.0)
		private static AnimationEvent BuildSetAnimationVelocity(Storage.GenericParameter parameter){
			FixedVector3 vel = BuildFixedVector3(parameter);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null, GameEntityPhysicsOperations.SetAnimationVelocity, vel
			);
		}


		// inputVel(zero)
		private static AnimationEvent BuildZeroMaxInputVelocity(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityPhysicsOperations.SetMaxInputVelocity,
				FixedVector3.Zero
			);
		}

		// inputVel(2.3, 1.5, 0.0)
		private static AnimationEvent BuildSetMaxInputVelocity(Storage.GenericParameter parameter){
			FixedFloat velx = parameter.SafeFloat(0);
			FixedFloat velz = parameter.SafeFloat(1);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityPhysicsOperations.SetMaxInputVelocity,
				new FixedVector3(velx, 0, velz)
			);
		}


		// impulseV(1.5)
		private static AnimationEvent BuildAddAnimationVerticalImpulse(Storage.GenericParameter parameter){
			FixedFloat vely = parameter.SafeFloat(0);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityPhysicsOperations.AddImpulse,
				new FixedVector3(0, vely, 0)
			);
		}


		// flip
		private static AnimationEvent BuildFlip(Storage.GenericParameter parameter){
			return new SimpleEntityAnimationEvent(null, GameEntityController.Flip);
		}

		// autoFlip(false)
		private static AnimationEvent BuildAutoFlip(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<bool>(null, GameEntityController.SetAutomaticFlip, parameter.SafeBool(0));
		}

		// pause(10)
		private static AnimationEvent BuildPause(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<int>(null, GameEntityController.PausePhysics, parameter.SafeInt(0));
		}

		// ++combo
		private static AnimationEvent BuildIncrementCombo(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<int>(null, GameEntityController.IncrementCombo, parameter.SafeInt(0));
		}

		// reset(combo)
		private static AnimationEvent BuildResetCombo(Storage.GenericParameter parameter){
			return new SimpleEntityAnimationEvent(null, GameEntityController.ResetCombo);
		}



		// instantMove(2.1, 4.2, 5.3)
		private static AnimationEvent BuildInstantMove(Storage.GenericParameter parameter){
			FixedVector3 vel = BuildFixedVector3(parameter);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null, GameEntityPhysicsOperations.MoveEntity, vel
			);
		}


	#region Anchoring


		// grab(hitten, 2, (2.1, 4.2, 5.3))
		private static AnimationEvent BuildAnchorWithMove(Storage.GenericParameter parameter){

			GameEntityReferenceDelegator delegator = BuildEntityDelegator(parameter);
			int anchorId = parameter.SafeInt(2);
			FixedVector3 movement = BuildFixedVector3(parameter);
			return new TrippleEntityAnimationEvent<GameEntityReferenceDelegator, int, FixedVector3>(
				null, GameEntityAnchoringOperations.AnchorEntity,
				delegator, anchorId, movement
			);

		}

		// grab(hitten, 2)
		private static AnimationEvent BuildAnchor(Storage.GenericParameter parameter){
			GameEntityReferenceDelegator delegator = BuildEntityDelegator(parameter);
			int anchorId = parameter.SafeInt(2);
			return new DoubleEntityAnimationEvent<GameEntityReferenceDelegator, int>(
				null, GameEntityAnchoringOperations.AnchorEntity, delegator, anchorId
			);
		}

		// release(all)
		private static AnimationEvent BuildReleaseAll(Storage.GenericParameter parameter){
			return new SimpleEntityAnimationEvent(null, GameEntityAnchoringOperations.ReleaseAllAnchoredEntities);
		}

		// release(2)
		private static AnimationEvent BuildRelease(Storage.GenericParameter parameter){
			int anchorId = parameter.SafeInt(0);
			return new SingleEntityAnimationEvent<int>(null, GameEntityAnchoringOperations.ReleaseAnchoredEntity, anchorId);
		}

		// grabbedPos(2, (2.1, 4.2, 5.3))
		private static AnimationEvent BuildSetAnchoredPos(Storage.GenericParameter parameter){
			int anchorId = parameter.SafeInt(0);
			FixedVector3 pos = BuildFixedVector3(parameter);
			return new DoubleEntityAnimationEvent<int, FixedVector3>(
				null, GameEntityAnchoringOperations.SetAnchoredEntityRelativePosition,
				anchorId, pos
			);
		}


		// grabbedAnim(2, jump)
		private static AnimationEvent BuildSetAnchoredAnim(Storage.GenericParameter parameter){
			int anchorId = parameter.SafeInt(0);
			string animName = parameter.SafeString(0);
			return new DoubleEntityAnimationEvent<int, string>(
				null, GameEntityAnchoringOperations.SetAnchoredEntityAnimation,
				anchorId, animName
			);
		}


	#endregion


		// impulse(grabbed, (2.1, 4.2, 5.3))
		private static AnimationEvent BuildAddRefImpulse(Storage.GenericParameter parameter){
			GameEntityReferenceDelegator delegator = BuildEntityDelegator(parameter);
			FixedVector3 impulse = BuildFixedVector3(parameter);
			return new DoubleEntityAnimationEvent<GameEntityReferenceDelegator, FixedVector3>(
				null, GameEntityPhysicsOperations.AddImpulse,
				delegator, impulse
			);
		}

		// reset(impulse)
		private static AnimationEvent BuildResetImpulse(Storage.GenericParameter parameter){
			return new SimpleEntityAnimationEvent(
				null, GameEntityPhysicsOperations.ResetPlanarImpulse
			);
		}


#endregion


	}

}
