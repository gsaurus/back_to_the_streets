using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class GenericParameterExtensions{

		public static int SafeInt(this Storage.GenericParameter parameter, int index){
			return index >= 0 && index < parameter.intsList.Length ? parameter.intsList[index] : 0; 
		}

		public static FixedFloat SafeFloat(this Storage.GenericParameter parameter, int index){
			return index >= 0 && index < parameter.floatsList.Length ? parameter.floatsList[index] : 0; 
		}

		public static string SafeString(this Storage.GenericParameter parameter, int index){
			return index >= 0 && index < parameter.stringsList.Length ? parameter.stringsList[index] : null; 
		}

		public static bool SafeBool(this Storage.GenericParameter parameter, int index){
			return index >= 0 && index < parameter.boolsList.Length ? parameter.boolsList[index] : false; 
		}

	}


	public class ConditionsBuilder {

		private static int invalidKeyframe = -1;
		private static int negationParamId = 0;

		private static int defaultBaseType = 90000;

//		private enum ConditionType:int {
//			Frame = 0,
//			FrameArithmetics = 1,
//			InputAxisMoving = 2,
//			InputButton = 3,
//			EntityIsGrounded = 4,
//			EntityIsFacingRight = 5,
//			EntityHittingWall = 6,
//			EntityCollisionForceArithmetics = 7
//		};

		// Condition builders indexed by type directly on array
		private delegate AnimationTriggerCondition BuilderAction(Storage.GenericParameter param, out int keyFrame);
		private static BuilderAction[] builderActions = {
			BuildKeyFrame,
			BuildFrameArithmetics,
			BuildInputAxisMoving,
			BuildInputButton,
			BuildEntityIsGrounded,
			BuildEntityIsFacingRight,
			BuildEntityHittingWall,
			BuildEntityCollisionForceArithmetics
		};
			

		// The public builder method
		public static AnimationTriggerCondition Build(Storage.Character charData, int[] conditionIds, out int keyFrame){
			List<AnimationTriggerCondition> conditions = new List<AnimationTriggerCondition>(conditionIds.Length);
			AnimationTriggerCondition condition;
			keyFrame = invalidKeyframe;
			foreach (int conditionId in conditionIds) {
				condition = BuildFromParameter(charData.genericParameters [conditionId], out keyFrame);
				if (condition != null) {
					conditions.Add(condition);
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


		// Build a single condition
		private static AnimationTriggerCondition BuildFromParameter(Storage.GenericParameter parameter, out int keyFrame){
			keyFrame = invalidKeyframe;
			AnimationTriggerCondition condition;
			if (parameter.type >= defaultBaseType) {
				int callIndex = parameter.type - defaultBaseType;
				if (callIndex < builderActions.Length) {
					condition = builderActions[callIndex](parameter, out keyFrame);
					if ( IsNegated(parameter) ) {
						condition = new NegateCondition(condition);
					}
					return condition;
				}
			}
			Debug.Log("ConditionsBuilder: Unknown condition type: " + parameter.type);
			return null;
		}


		// Negation check
		private static bool IsNegated(Storage.GenericParameter parameter){
			return parameter.SafeBool(negationParamId);
		}




#region Conditions


		// frame = 4
		private static AnimationTriggerCondition BuildKeyFrame(Storage.GenericParameter parameter, out int keyFrame){
			keyFrame = parameter.SafeInt(0);
			return null;
		}


		// frame >= 4
		private static AnimationTriggerCondition BuildFrameArithmetics(Storage.GenericParameter parameter, out int keyFrame){
			keyFrame = invalidKeyframe;
			int frameIndex = parameter.SafeInt(0);
			ArithmeticConditionOperatorType type = parameter.SafeInt(1);
			return new AnimationFrameCondition(type, frameIndex);
		}


		// moving
		private static AnimationTriggerCondition BuildInputAxisMoving(Storage.GenericParameter parameter, out int keyFrame){
			keyFrame = invalidKeyframe;
			return null;
		}


		// button D
		private static AnimationTriggerCondition BuildInputButton(Storage.GenericParameter parameter, out int keyFrame){
			keyFrame = invalidKeyframe;
			return null;
		}


		// grounded
		private static AnimationTriggerCondition BuildEntityIsGrounded(Storage.GenericParameter parameter, out int keyFrame){
			keyFrame = invalidKeyframe;
			return null;
		}


		// facing right
		private static AnimationTriggerCondition BuildEntityIsFacingRight(Storage.GenericParameter parameter, out int keyFrame){
			keyFrame = invalidKeyframe;
			return null;
		}


		// collide left wall
		private static AnimationTriggerCondition BuildEntityHittingWall(Storage.GenericParameter parameter, out int keyFrame){
			keyFrame = invalidKeyframe;
			return null;
		}


		// collisionForceY >= 4
		private static AnimationTriggerCondition BuildEntityCollisionForceArithmetics(Storage.GenericParameter parameter, out int keyFrame){
			keyFrame = invalidKeyframe;
			return null;
		}


#endregion


	}

}
