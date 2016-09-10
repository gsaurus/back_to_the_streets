using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class HudEventsBuilder {

		// Event builders indexed by type directly on array
		private delegate GenericEvent<HUDViewBehaviour> BuilderAction(Storage.GenericParameter param);
		private static BuilderAction[] builderActions = {
			BuildSetHUD					// 0: 'walk'

		};


		// The public builder method
		public static GenericEvent<HUDViewBehaviour> Build(Storage.HUD hudData, int[] eventIds){
			List<GenericEvent<HUDViewBehaviour>> events = new List<GenericEvent<HUDViewBehaviour>>(eventIds.Length);
			GenericEvent<HUDViewBehaviour> e;
			foreach (int eventId in eventIds) {
				e = BuildFromParameter(hudData.genericParameters[eventId]);
				if (e != null) {
					events.Add(e);
				}
			}
			if (events.Count > 0) {
				if (events.Count == 1) {
					return events[0];
				}
				return new EventsList<HUDViewBehaviour>(null, events);
			}
			return null;
		}


		// Build a single event
		private static GenericEvent<HUDViewBehaviour> BuildFromParameter(Storage.GenericParameter parameter){
			int callIndex = parameter.type;
			if (callIndex < builderActions.Length) {
				return builderActions[callIndex](parameter);
			}
			Debug.Log("HudEventsBuilder: Unknown event type: " + parameter.type);
			return null;
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
		private static GenericEvent<HUDViewBehaviour> BuildSetHUD(Storage.GenericParameter parameter){
			//return new HUDTransitionEvent(null, parameter.SafeString(0), (float) parameter.SafeFloat(0), (uint)parameter.SafeInt(0));
			return null;
		}


#endregion

	}

}
