﻿using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class CharacterConditionsBuilder {

		// Combo counts sequences of animations
		public static readonly string comboCustomVariableName = "combo";
		// Combo can only be incremented once per animation, thus store if it was already incremented on a variable
		public static readonly string comboAnimationClearFlag = "comboAnimClearFlag";


		public static int invalidKeyframe = -1;
		private static int negationParamId = 0;



		// Condition builders indexed by type directly on array
		private delegate GenericTriggerCondition<AnimationModel> BuilderAction(Storage.GenericParameter param, out int keyFrame, Storage.CharacterAnimation animation);
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
			BuildOnHurtDirection,					// 13: on hurt(1)
			BuildComboCounter,						// 14: combo >= 2
			BuildComboTimer,						// 15: combo timer < 10
			BuildIsAnchored,						// 16: grabbed
			BuildIsAnchoring,						// 17: grabbing(2)
			BuildCompareVerticalImpulse,			// 18: impulseV(1.5)

		};
			

		// The public builder method
		public static GenericTriggerCondition<AnimationModel> Build(Storage.Character charData, int[] conditionIds, out int keyFrame, Storage.CharacterAnimation animation){
			List<GenericTriggerCondition<AnimationModel>> conditions = new List<GenericTriggerCondition<AnimationModel>>(conditionIds.Length);
			GenericTriggerCondition<AnimationModel> condition;
			keyFrame = invalidKeyframe;
			int conditionKeyFrame;
			foreach (int conditionId in conditionIds) {
				condition = BuildFromParameter(charData.genericParameters[conditionId], out conditionKeyFrame, animation);
				if (condition != null) {
					conditions.Add(condition);
				} else if (conditionKeyFrame != invalidKeyframe){
					keyFrame = conditionKeyFrame;
				}
			}
			if (conditions.Count > 0) {
				if (conditions.Count == 1) {
					return conditions[0];
				}
				return new ConditionsList<AnimationModel>(conditions);
			}
			return null;
		}


		// Build a single condition
		private static GenericTriggerCondition<AnimationModel> BuildFromParameter(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			GenericTriggerCondition<AnimationModel> condition;
			int callIndex = parameter.type;
			if (callIndex < builderActions.Length) {
				condition = builderActions[callIndex](parameter, out keyFrame, animation);
				if ( IsNegated(parameter) ) {
					condition = new NegateCondition<AnimationModel>(condition);
				}
				return condition;
			}
			Debug.Log("CharacterConditionsBuilder: Unknown condition type: " + parameter.type);
			return null;
		}


		// Negation check
		private static bool IsNegated(Storage.GenericParameter parameter){
			return parameter.SafeBool(negationParamId);
		}




#region Conditions


		// frame = 4
		private static GenericTriggerCondition<AnimationModel> BuildKeyFrame(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = parameter.SafeInt(0);
			if (keyFrame < 0) keyFrame = animation.numFrames;
			return null;
		}


		// frame >= 4
		private static GenericTriggerCondition<AnimationModel> BuildFrameArithmetics(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			uint frameIndex = (uint)parameter.SafeInt(0);
			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType)parameter.SafeInt(1);
			return new AnimationFrameCondition(type, frameIndex);
		}


		// moving
		private static GenericTriggerCondition<AnimationModel> BuildInputAxisMoving(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new InputAxisMovingCondition();
		}

		// move left
		private static GenericTriggerCondition<AnimationModel> BuildInputAxisMovingDominance(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
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
		private static GenericTriggerCondition<AnimationModel> BuildInputAxisComponentArithmetics(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
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
		private static GenericTriggerCondition<AnimationModel> BuildInputButton(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
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
		private static GenericTriggerCondition<AnimationModel> BuildEntityIsGrounded(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new EntityBoolCondition(GameEntityPhysicsOperations.IsGrounded);
		}


		// facing right
		private static GenericTriggerCondition<AnimationModel> BuildEntityIsFacingRight(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new EntityBoolCondition(GameEntityPhysicsOperations.IsFacingRight);
		}


		// collide left wall
		private static GenericTriggerCondition<AnimationModel> BuildEntityHittingWall(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			switch (parameter.SafeInt(0)) {
				case 0: return new EntityBoolCondition(GameEntityPhysicsOperations.IsHittingFarWall);	 // 0: far wall
				case 1: return new EntityBoolCondition(GameEntityPhysicsOperations.IsHittingNearWall);  // 1: near wall
				case 2: return new EntityBoolCondition(GameEntityPhysicsOperations.IsHittingLeftWall);	 // 2: left wall
				case 3: return new EntityBoolCondition(GameEntityPhysicsOperations.IsHittingRightWall); // 3: right wall
			}
			return null;
		}


		// collideH >= 4.3
		private static GenericTriggerCondition<AnimationModel> BuildEntityCollisionForceArithmetics(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType) parameter.SafeInt(1);
			FixedFloat value = parameter.SafeFloat(0);
			switch (parameter.SafeInt(0)) {
				case 0: return new EntityArithmeticCondition<FixedFloat>(type, GameEntityPhysicsOperations.CollisionHorizontalForce, value);	 // 0: H
				case 1: return new EntityArithmeticCondition<FixedFloat>(type, GameEntityPhysicsOperations.CollisionVerticalForce, value); 	 // 1: V
				case 2: return new EntityArithmeticCondition<FixedFloat>(type, GameEntityPhysicsOperations.CollisionZForce, value);	 		 // 2: Z
			}
			return null;
		}

		// entity collision
		private static GenericTriggerCondition<AnimationModel> BuildEntityEntityCollisionCheck(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new EntityBoolCondition(GameEntityController.IsCollidingWithOthers);
		}


		// on hit
		private static GenericTriggerCondition<AnimationModel> BuildOnHit(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new EntityArithmeticCondition<int>(ArithmeticConditionOperatorType.greater, GameEntityController.HitTargetsCount, 0);
		}

										
		// on hurt(K.O.)
		private static GenericTriggerCondition<AnimationModel> BuildOnHurt(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			int hitTypeNum = parameter.SafeInt(0);
			if (hitTypeNum < Enum.GetNames(typeof(HitData.HitType)).Length) {
				return new SingleEntityBoolCondition<HitData.HitType>(
					GameEntityController.HurtContainsType,
					(HitData.HitType)hitTypeNum
				);
			}else{
				return new EntityArithmeticCondition<int>(ArithmeticConditionOperatorType.greater, GameEntityController.HurtSourcesCount, 0);
			}
		}

		// hurt_from(front)
		private static GenericTriggerCondition<AnimationModel> BuildOnHurtDirection(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			int direction = parameter.SafeInt(0);
			return new SingleEntityBoolCondition<bool>(GameEntityController.IsHurtFrontal, direction == 0);
		}

		// combo <= 3
		private static GenericTriggerCondition<AnimationModel> BuildComboCounter(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType) parameter.SafeInt(1);
			int value = parameter.SafeInt(0);
			return new EntityArithmeticCondition<int>(
				type,
				delegate(GameEntityModel model){
					int res;
					model.customVariables.TryGetValue(comboCustomVariableName, out res);
					return res;
				},
				value
			);
		}

		// combo timer <= 10
		private static GenericTriggerCondition<AnimationModel> BuildComboTimer(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType) parameter.SafeInt(1);
			int value = parameter.SafeInt(0);
			return new EntityArithmeticCondition<int>(
				type,
				delegate(GameEntityModel model){
					int res;
					model.customTimers.TryGetValue(comboCustomVariableName, out res);
					return res;
				},
				value
			);
		}


		// grabbed
		private static GenericTriggerCondition<AnimationModel> BuildIsAnchored(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new EntityBoolCondition(GameEntityAnchoringOperations.IsAnchored);
		}

		// grabbing(3)
		private static GenericTriggerCondition<AnimationModel> BuildIsAnchoring(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			return new SingleEntityBoolCondition<int>(GameEntityAnchoringOperations.IsAnchoring, parameter.SafeInt(0));
		}


		// impulseV(1.5)
		private static GenericTriggerCondition<AnimationModel> BuildCompareVerticalImpulse(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = invalidKeyframe;
			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType) parameter.SafeInt(0);
			FixedFloat value = parameter.SafeFloat(0);
			return new EntityArithmeticCondition<FixedFloat>(type, GameEntityPhysicsOperations.GetVerticalImpulse, value);
		}


#endregion


	}

}
