
using System;
using UnityEngine;
using System.Collections.Generic;


// Condition based on the animation current frame number
// Right value can be a constant, or given by a getter delegate
public class AnimationFrameCondition: AnimationTriggerCondition{

	// Delegate of the getter
	public delegate uint GetArithmeticConditionVariable(AnimationModel model);
	
	private GetArithmeticConditionVariable getRightVariableDelegate;
	// A constant for the right value
	private uint rightValue;

	// Condition operator
	private ArithmeticConditionOperatorType conditionOperator;


	// Constructor with getter delegate
	public AnimationFrameCondition(
		ArithmeticConditionOperatorType operatorType,
		GetArithmeticConditionVariable rightVariableDelegate
	){
		this.conditionOperator = operatorType;
		this.getRightVariableDelegate = rightVariableDelegate;
	}

	// Constructor with const value
	public AnimationFrameCondition(
		ArithmeticConditionOperatorType operatorType,
		uint rightValue
	){
		this.conditionOperator = operatorType;
		this.rightValue = rightValue;
	}


	// Evaluate the condition
	public bool Evaluate(AnimationModel model){

		// obtain left & right values
		uint lvalue, rvalue;
		lvalue = model.currentFrame;
		if (getRightVariableDelegate != null) {
			rvalue = getRightVariableDelegate(model);
		}else {
			rvalue = rightValue;
		}

		// +1 because we want this kind of conditions to be checked by the end of the frame
		++rvalue;

		// compare them
		int result = lvalue.CompareTo(rvalue);
		switch (conditionOperator){
			case ArithmeticConditionOperatorType.equal:{
				return result == 0;
			}
			case ArithmeticConditionOperatorType.notEqual:{
				return result != 0;
			}
			case ArithmeticConditionOperatorType.less:{
				return result < 0;
			}
			case ArithmeticConditionOperatorType.greater:{
				return result > 0;
			}
		}
		// won't reach here, but compiler complains, so..
		return false;
	}

}

