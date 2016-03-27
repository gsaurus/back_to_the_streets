using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class ConditionsBuilder {

		public static int invalidKeyframe = -1;
		private static int negationParamId = 0;



		// Condition builders indexed by type directly on array
		private delegate AnimationTriggerCondition BuilderAction(Storage.GenericParameter param, out int keyFrame, Storage.CharacterAnimation animation);
		private static BuilderAction[] builderActions = {
			BuildKeyFrame,							// 0: frame = 4
			BuildFrameArithmetics,					// 1: frame >= 4
			BuildInputAxisMoving,					// 2: moving
			BuildInputAxisMovingDominance,          // 3: move left
			BuildInputAxisComponentArithmetics,     // 4: move_H >= 5.2
			BuildInputButton,						// 5: press D
			BuildEntityIsGrounded,					// 6: grounded
			BuildEntityIsFacingRight,				// 7: facing right
			BuildEntityHittingWall,					// 8: collide left wall
			BuildEntityCollisionForceArithmetics,	// 9: collide_H >= 4.3
			BuildEntityEntityCollisionCheck,		// 10: entity collision
			BuildOnHit,								// 11: on hit
			BuildOnHurt,							// 12: on hurt
			BuildOnSpecificHurt						// 13: on hurt(1)
			// TODO: everything else, including custom values List<int>, List<FixedFloat>, List<int> timers for combo counter etc
		};
			

		// The public builder method
		public static AnimationTriggerCondition Build(Storage.Character charData, int[] conditionIds, out int keyFrame, Storage.CharacterAnimation animation){
			List<AnimationTriggerCondition> conditions = new List<AnimationTriggerCondition>(conditionIds.Length);
			AnimationTriggerCondition condition;
			keyFrame = invalidKeyframe;
			foreach (int conditionId in conditionIds) {
				condition = BuildFromParameter(charData.genericParameters[conditionId], out keyFrame, animation);
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
		private static AnimationTriggerCondition BuildFromParameter(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			AnimationTriggerCondition condition;
			int callIndex = parameter.type;
			if (callIndex < builderActions.Length) {
				condition = builderActions[callIndex](parameter, out keyFrame, animation);
				if ( IsNegated(parameter) ) {
					condition = new NegateCondition(condition);
				}
				return condition;
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
		private static AnimationTriggerCondition BuildKeyFrame(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = parameter.SafeInt(0);
			if (keyFrame < 0) keyFrame = animation.numFrames;
			return null;
		}


		// frame >= 4
		private static AnimationTriggerCondition BuildFrameArithmetics(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			uint frameIndex = (uint)parameter.SafeInt(0);
			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType)parameter.SafeInt(1);
			return new AnimationFrameCondition(type, frameIndex);
		}


		// moving
		private static AnimationTriggerCondition BuildInputAxisMoving(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new InputAxisMovingCondition();
		}

		// move left
		private static AnimationTriggerCondition BuildInputAxisMovingDominance(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			switch (parameter.SafeInt(0)) {
                case 0: return new InputAxisUpDominanceCondition();         // 0: up
                case 1: return new InputAxisDownDominanceCondition();       // 1: down
                case 2: return new InputAxisBackwardDominanceCondition();   // 2: left
                case 3: return new InputAxisForwardDominanceCondition();    // 3: right
			}
			return null;
		}


        // move_H >= 5.2
		private static AnimationTriggerCondition BuildInputAxisComponentArithmetics(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
            keyFrame = invalidKeyframe;
			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType) parameter.SafeInt(1);
            FixedFloat value = parameter.SafeFloat(0);
            switch (parameter.SafeInt(0)) {
                case 0: return InputAxisComponentCondition.HorizontalAxisCondition(type, value);		// 0: horizontal
                case 1: return InputAxisComponentCondition.VerticalAxisCondition(type, value);			// 1: vertical
            }
            return null;
        }


		// press D
		private static AnimationTriggerCondition BuildInputButton(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			uint buttonId = (uint) parameter.SafeInt(1);
			switch (parameter.SafeInt(0)) {
				case 0: return InputButtonCondition.ButtonPressedCondition(true, buttonId);	 // 0: press
				case 1: return InputButtonCondition.ButtonHoldCondition(true, buttonId);	 // 1: hold
				case 2: return InputButtonCondition.ButtonReleasedCondition(true, buttonId); // 2: release
			}
			return null;
		}


		// grounded
		private static AnimationTriggerCondition BuildEntityIsGrounded(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new EntityBoolCondition(GameEntityController.IsGrounded);
		}


		// facing right
		private static AnimationTriggerCondition BuildEntityIsFacingRight(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new EntityBoolCondition(GameEntityController.IsFacingRight);
		}


		// collide left wall
		private static AnimationTriggerCondition BuildEntityHittingWall(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			switch (parameter.SafeInt(0)) {
				case 0: return new EntityBoolCondition(GameEntityController.IsHittingFarWall);	 // 0: far wall
				case 1: return new EntityBoolCondition(GameEntityController.IsHittingNearWall);  // 1: near wall
				case 2: return new EntityBoolCondition(GameEntityController.IsHittingLeftWall);	 // 2: left wall
				case 3: return new EntityBoolCondition(GameEntityController.IsHittingRightWall); // 3: right wall
			}
			return null;
		}


		// collideH >= 4.3
		private static AnimationTriggerCondition BuildEntityCollisionForceArithmetics(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType) parameter.SafeInt(1);
			FixedFloat value = parameter.SafeFloat(0);
			switch (parameter.SafeInt(0)) {
				case 0: return new EntityArithmeticCondition<FixedFloat>(type, GameEntityController.CollisionHorizontalForce, value);	 // 0: H
				case 1: return new EntityArithmeticCondition<FixedFloat>(type, GameEntityController.CollisionVerticalForce, value); 	 // 1: V
				case 2: return new EntityArithmeticCondition<FixedFloat>(type, GameEntityController.CollisionZForce, value);	 		 // 2: Z
			}
			return null;
		}

		// entity collision
		private static AnimationTriggerCondition BuildEntityEntityCollisionCheck(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new EntityBoolCondition(GameEntityController.IsCollidingWithOthers);
		}


		// on hit
		private static AnimationTriggerCondition BuildOnHit(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new EntityArithmeticCondition<int>(ArithmeticConditionOperatorType.greater, GameEntityController.HitTargetsCount, 0);
		}

										
		// on hurt
		private static AnimationTriggerCondition BuildOnHurt(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new EntityArithmeticCondition<int>(ArithmeticConditionOperatorType.greater, GameEntityController.HurtSourcesCount, 0);
		}

		// on hurt(3)
		private static AnimationTriggerCondition BuildOnSpecificHurt(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			int collisionId = (int)parameter.SafeInt(0);
			return new SingleEntityBoolCondition<int>(GameEntityController.HurtsContainCollisionId, collisionId);
		}

										


#endregion


	}

}
