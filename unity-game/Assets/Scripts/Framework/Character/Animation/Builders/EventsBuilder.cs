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
			BuildResetCombo						// 10: reset(combo)
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


		// Build a single condition
		private static AnimationEvent BuildFromParameter(Storage.GenericParameter parameter){
			int callIndex = parameter.type;
			if (callIndex < builderActions.Length) {
				return builderActions[callIndex](parameter);
			}
			Debug.Log("EventsBuilder: Unknown event type: " + parameter.type);
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
				GameEntityController.SetAnimationVelocity,
				FixedVector3.Zero
			);
		}

		// vel(2.3, 1.5, 0.0)
		private static AnimationEvent BuildSetAnimationVelocity(Storage.GenericParameter parameter){
			FixedFloat velx = parameter.SafeFloat(0);
			FixedFloat vely = parameter.SafeFloat(1);
			FixedFloat velz = parameter.SafeFloat(2);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityController.SetAnimationVelocity,
				new FixedVector3(velx, vely, velz)
			);
		}


		// inputVel(zero)
		private static AnimationEvent BuildZeroMaxInputVelocity(Storage.GenericParameter parameter){
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityController.SetMaxInputVelocity,
				FixedVector3.Zero
			);
		}

		// inputVel(2.3, 1.5, 0.0)
		private static AnimationEvent BuildSetMaxInputVelocity(Storage.GenericParameter parameter){
			FixedFloat velx = parameter.SafeFloat(0);
			FixedFloat velz = parameter.SafeFloat(1);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityController.SetMaxInputVelocity,
				new FixedVector3(velx, 0, velz)
			);
		}


		// impulseV(1.5)
		private static AnimationEvent BuildAddAnimationVerticalImpulse(Storage.GenericParameter parameter){
			FixedFloat vely = parameter.SafeFloat(0);
			return new SingleEntityAnimationEvent<FixedVector3>(
				null,
				GameEntityController.AddImpulse,
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




		#endregion


	}

}
