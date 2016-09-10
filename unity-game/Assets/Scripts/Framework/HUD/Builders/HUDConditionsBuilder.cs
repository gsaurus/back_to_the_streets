using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class HUDConditionsBuilder {

		private static int negationParamId = 0;



		// Condition builders indexed by type directly on array
		private delegate GenericTriggerCondition<HUDViewBehaviour> BuilderAction(Storage.GenericParameter param);
		private static BuilderAction[] builderActions = {
			BuildFrameArithmetics							// 0: frame = 4

		};
			

		// The public builder method
		public static GenericTriggerCondition<HUDViewBehaviour> Build(Storage.HUD hudData, int[] conditionIds){
			List<GenericTriggerCondition<HUDViewBehaviour>> conditions = new List<GenericTriggerCondition<HUDViewBehaviour>>(conditionIds.Length);
			GenericTriggerCondition<HUDViewBehaviour> condition;
			foreach (int conditionId in conditionIds) {
				condition = BuildFromParameter(hudData.genericParameters[conditionId]);
				if (condition != null) {
					conditions.Add(condition);
				}
			}
			if (conditions.Count > 0) {
				if (conditions.Count == 1) {
					return conditions[0];
				}
				return new ConditionsList<HUDViewBehaviour>(conditions);
			}
			return null;
		}


		// Build a single condition
		private static GenericTriggerCondition<HUDViewBehaviour> BuildFromParameter(Storage.GenericParameter parameter){
			GenericTriggerCondition<HUDViewBehaviour> condition;
			int callIndex = parameter.type;
			if (callIndex < builderActions.Length) {
				condition = builderActions[callIndex](parameter);
				if ( IsNegated(parameter) ) {
					condition = new NegateCondition<HUDViewBehaviour>(condition);
				}
				return condition;
			}
			Debug.Log("HUDConditionsBuilder: Unknown condition type: " + parameter.type);
			return null;
		}


		// Negation check
		private static bool IsNegated(Storage.GenericParameter parameter){
			return parameter.SafeBool(negationParamId);
		}




#region Conditions


		// frame >= 4
		private static GenericTriggerCondition<HUDViewBehaviour> BuildFrameArithmetics(Storage.GenericParameter parameter){
//			keyFrame = invalidKeyframe;
//			uint frameIndex = (uint)parameter.SafeInt(0);
//			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType)parameter.SafeInt(1);
//			return new HUDFrameCondition(type, frameIndex);
			return null;
		}



#endregion


	}

}
