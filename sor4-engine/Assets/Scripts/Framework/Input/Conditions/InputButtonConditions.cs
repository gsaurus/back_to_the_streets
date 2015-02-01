
using System;
using UnityEngine;
using System.Collections.Generic;



public enum InputButtonConditionType{
	pressed,
	hold,
	released
}


// Condition based on input button
public class InputButtonCondition: BoolCondition{

	// The button to be checked
	private uint buttonId;

#region Checkers

	// Check button press
	private bool IsButtonPressed(AnimationModel model){
		Model inputModel = InputConditionsHelper.GetInputModel(model);
		if (inputModel == null) return false;
		GameEntityInputProvider inputController = inputModel.Controller() as GameEntityInputProvider;
		if (inputController == null) return false;
		return inputController.IsButtonPressed(inputModel, buttonId);
	}

	// Check button hold
	private bool IsButtonHold(AnimationModel model){
		Model inputModel = InputConditionsHelper.GetInputModel(model);
		if (inputModel == null) return false;
		GameEntityInputProvider inputController = inputModel.Controller() as GameEntityInputProvider;
		if (inputController == null) return false;
		return inputController.IsButtonHold(inputModel, buttonId);
	}

	// Check button release
	private bool IsButtonReleased(AnimationModel model){
		Model inputModel = InputConditionsHelper.GetInputModel(model);
		if (inputModel == null) return false;
		GameEntityInputProvider inputController = inputModel.Controller() as GameEntityInputProvider;
		if (inputController == null) return false;
		return inputController.IsButtonReleased(inputModel, buttonId);
	}

#endregion


#region Private constructors


	private void SetupLeftDelegate(InputButtonConditionType type){
		switch(type){
			case InputButtonConditionType.pressed:{
				this.getLeftVariableDelegate = IsButtonPressed;
			}break;
			case InputButtonConditionType.hold:{
				this.getLeftVariableDelegate = IsButtonHold;
			}break;
			case InputButtonConditionType.released:{
				this.getLeftVariableDelegate = IsButtonReleased;
			}break;
		}
	}
	
	
	// Private constructor with two getter delegates
	public InputButtonCondition(
		InputButtonConditionType type,
		BoolConditionDelegate rightVariableDelegate,
		uint buttonId
	){
		this.getRightVariableDelegate = rightVariableDelegate;
		this.buttonId = buttonId;
		SetupLeftDelegate(type);
	}
	
	// Private constructor with left delegate and right const value
	public InputButtonCondition(
		InputButtonConditionType type,
		bool rightValue,
		uint buttonId
	){
		this.rightValue = rightValue;
		this.buttonId = buttonId;
		SetupLeftDelegate(type);
	}

#endregion


#region Public static builders

	// Versions with right delegate
	public static InputButtonCondition ButtonPressedCondition(BoolConditionDelegate rightVariableDelegate, uint buttonId){
		return new InputButtonCondition(InputButtonConditionType.pressed, rightVariableDelegate, buttonId);
	}
	public static InputButtonCondition ButtonHoldCondition(BoolConditionDelegate rightVariableDelegate, uint buttonId){
		return new InputButtonCondition(InputButtonConditionType.hold, rightVariableDelegate, buttonId);
	}
	public static InputButtonCondition ButtonReleasedCondition(BoolConditionDelegate rightVariableDelegate, uint buttonId){
		return new InputButtonCondition(InputButtonConditionType.released, rightVariableDelegate, buttonId);
	}

	// Versions with right value
	public static InputButtonCondition ButtonPressedCondition(bool rightValue, uint buttonId){
		return new InputButtonCondition(InputButtonConditionType.pressed, rightValue, buttonId);
	}
	public static InputButtonCondition ButtonHoldCondition(bool rightValue, uint buttonId){
		return new InputButtonCondition(InputButtonConditionType.hold, rightValue, buttonId);
	}
	public static InputButtonCondition ButtonReleasedCondition(bool rightValue, uint buttonId){
		return new InputButtonCondition(InputButtonConditionType.released, rightValue, buttonId);
	}

#endregion

}

