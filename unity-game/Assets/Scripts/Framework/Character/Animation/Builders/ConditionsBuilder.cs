using System;
using System.Collections.Generic;

namespace RetroBread{

	public class ConditionsBuilder {

		private static int negationParamId = 0;

		private enum ConditionType:int {
			Frame = 9001,
			FrameArithmetics = 9002,
			InputAxisMoving = 9003,
			InputButton = 9004,
			EntityIsGrounded = 9005,
			EntityIsFacingRight = 9006,
			EntityHittingWall = 9007,
			EntityCollisionForceArithmetics = 9008
		};


		public static AnimationTriggerCondition Build(Storage.Character charData, int[] conditionIds, out int keyFrame){
			List<AnimationTriggerCondition> conditions = new List<AnimationTriggerCondition>(conditionIds.Length);
			AnimationTriggerCondition condition;
			foreach (int conditionId in conditionIds) {
				condition = BuildFromParameter(charData.genericParameters [conditionId], out keyFrame);
				if (condition != null) {
					conditions.Add (condition);
				}
			}
			if (conditions.Count > 0) {
				if (conditions.Count == 1) {
					return conditions[0];
				}
				return new ConditionsList(conditions);
			}
			return null;
		}


		private static AnimationTriggerCondition BuildFromParameter(Storage.GenericParameter parameter, out int keyFrame){
			
		}

	}

}
