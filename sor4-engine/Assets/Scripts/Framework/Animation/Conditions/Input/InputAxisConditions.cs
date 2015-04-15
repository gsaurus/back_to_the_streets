using System;
using UnityEngine;
using System.Collections.Generic;



namespace RetroBread{



#region movement detection

// Check if there is any axis movement happening
public class InputAxisMovingCondition: AnimationTriggerCondition{

	public bool Evaluate(AnimationModel model){
		FixedVector3 axis = InputConditionsHelper.GetInputAxis(model);
		return axis.Magnitude > 0;
	}

}

// Check if character is mostly moving forward
public class InputAxisForwardDominanceCondition: AnimationTriggerCondition{
	
	public bool Evaluate(AnimationModel model){
		FixedVector3 axis = InputConditionsHelper.GetInputAxis(model);
		bool isFacingRight = InputConditionsHelper.IsOwnerFacingRight(model);
		if ((isFacingRight && axis.X <= 0) || (!isFacingRight && axis.X >= 0)){
			// moving backward
			return false;
		}
		return FixedFloat.Abs(axis.X) / axis.Magnitude >= InputConditionsHelper.AxisDominanceFactor;
	}
	
}


// Check if character is mostly moving backward
public class InputAxisBackwardDominanceCondition: AnimationTriggerCondition{
	
	public bool Evaluate(AnimationModel model){
		FixedVector3 axis = InputConditionsHelper.GetInputAxis(model);
		bool isFacingRight = InputConditionsHelper.IsOwnerFacingRight(model);
		if ((isFacingRight && axis.X >= 0) || (!isFacingRight && axis.X <= 0)){
			// moving forward
			return false;
		}
		return FixedFloat.Abs(axis.X) / axis.Magnitude >= InputConditionsHelper.AxisDominanceFactor;
	}
	
}


// Check if character is mostly moving up
public class InputAxisUpDominanceCondition: AnimationTriggerCondition{
	
	public bool Evaluate(AnimationModel model){
		FixedVector3 axis = InputConditionsHelper.GetInputAxis(model);
		if (axis.Z <= 0){
			// moving down
			return false;
		}
		return FixedFloat.Abs(axis.Z) / axis.Magnitude >= InputConditionsHelper.AxisDominanceFactor;
	}
	
}


// Check if character is mostly moving down
public class InputAxisDownDominanceCondition: AnimationTriggerCondition{
	
	public bool Evaluate(AnimationModel model){
		FixedVector3 axis = InputConditionsHelper.GetInputAxis(model);
		if (axis.Z >= 0){
			// moving up
			return false;
		}
		return FixedFloat.Abs(axis.Z) / axis.Magnitude >= InputConditionsHelper.AxisDominanceFactor;
	}
	
}


#endregion


#region axis arithmetics

public enum InputAxisComponentType{
	horizontal,
	vertical
}


public class InputAxisComponentCondition: ArithmeticCondition<FixedFloat>{

	// Get the horizontal axis component
	private FixedFloat HorizontalAxis(AnimationModel model){
		FixedVector3 axis = InputConditionsHelper.GetInputAxis(model);
		return axis.X;
	}

	// Get the vertical axis component
	private FixedFloat VerticalAxis(AnimationModel model){
		FixedVector3 axis = InputConditionsHelper.GetInputAxis(model);
		return axis.Z;
	}

	private void SetupAxisType(InputAxisComponentType type){
		switch(type){
			case InputAxisComponentType.horizontal:{
				getLeftVariableDelegate = HorizontalAxis;
			}break;
			case InputAxisComponentType.vertical:{
				getLeftVariableDelegate = VerticalAxis;
			}break;
		}
	}


	// Redefine constructor as private
	public InputAxisComponentCondition(
		ArithmeticConditionOperatorType operatorType,
		InputAxisComponentType type,
		GetArithmeticConditionVariable rightVariableDelegate
	){
		this.getRightVariableDelegate = rightVariableDelegate;
		this.conditionOperator = operatorType;
		SetupAxisType(type);
	}
	
	// Redefine constructor as private
	public InputAxisComponentCondition(
		ArithmeticConditionOperatorType operatorType,
		InputAxisComponentType type,
		FixedFloat rightValue
	){
		this.rightValue = rightValue;
		this.conditionOperator = operatorType;
		SetupAxisType(type);
	}


	// Static builders

	public static InputAxisComponentCondition HorizontalAxisCondition(
		ArithmeticConditionOperatorType operatorType,
		GetArithmeticConditionVariable rightVariableDelegate
	){
		return new InputAxisComponentCondition(operatorType, InputAxisComponentType.horizontal, rightVariableDelegate);
	}

	public static InputAxisComponentCondition VerticalAxisCondition(
		ArithmeticConditionOperatorType operatorType,
		GetArithmeticConditionVariable rightVariableDelegate
	){
		return new InputAxisComponentCondition(operatorType, InputAxisComponentType.vertical, rightVariableDelegate);
	}

	public static InputAxisComponentCondition HorizontalAxisCondition(
		ArithmeticConditionOperatorType operatorType,
		FixedFloat rightValue
	){
		return new InputAxisComponentCondition(operatorType, InputAxisComponentType.horizontal, rightValue);
	}

	public static InputAxisComponentCondition VerticalAxisCondition(
		ArithmeticConditionOperatorType operatorType,
		FixedFloat rightValue
	){
		return new InputAxisComponentCondition(operatorType, InputAxisComponentType.vertical, rightValue);
	}

}

#endregion



}

