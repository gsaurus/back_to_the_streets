using System;
using System.Collections.Generic;

namespace RetroBread{


public static class CharacterConditionsBuilder {

	public static int InvalidKeyframe = -1;

	private enum Orientation {
		horizontal	= 0,
		vertical	= 1,
		any			= 2
	}


	// Condition evaluation delegate builders indexed by type directly on array
	// Receives animation just to pre-process special cases such as last frame (currently the only situation)
	private delegate EventCondition<GameEntityModel>.EvaluationDelegate BuilderAction(Storage.GenericParameter param, out int keyFrame, Storage.CharacterAnimation animation);
	private static BuilderAction[] builderActions = {
		BuildFrame,
		BuildInputVelocity,
		BuildInputButton,
		BuildGrounded,
		BuildFacingRight,
		BuildCollisionImpact,
		BuildVariable,
		BuildVelocity,
		BuildExistence,
		BuildTeam,
		BuildName,
		BuildAnimation,
	};
		

	// The public builder method
	public static List<EventCondition<GameEntityModel>> Build(Storage.Character charData, int[] conditionIds, out int keyFrame, Storage.CharacterAnimation animation){
		List<EventCondition<GameEntityModel>> conditions = new List<EventCondition<GameEntityModel>>(conditionIds.Length);
		EventCondition<GameEntityModel> condition;
		keyFrame = InvalidKeyframe;
		int conditionKeyFrame;
		foreach (int conditionId in conditionIds) {
			condition = BuildFromParameter(charData.genericParameters[conditionId], out conditionKeyFrame, animation);
			if (condition != null) {
				conditions.Add(condition);
			} else if (conditionKeyFrame != InvalidKeyframe){
				keyFrame = conditionKeyFrame;
			}
		}
		return conditions;
	}


	// Build a single condition
	private static EventCondition<GameEntityModel> BuildFromParameter(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		keyFrame = InvalidKeyframe;
		EventCondition<GameEntityModel>.EvaluationDelegate evaluationDelegate;
		EventCondition<GameEntityModel> condition;
		int callIndex = parameter.type;
		int subjectId;
		if (callIndex < builderActions.Length) {
			evaluationDelegate = builderActions[callIndex](parameter, out keyFrame, animation);
			if (evaluationDelegate == null){
				// No delegate, fixed keyFrame only
				return null;
			}
			evaluationDelegate = FilterNegation(evaluationDelegate, parameter);
			subjectId = GetSubjectId(parameter);
			return new EventCondition<GameEntityModel>(evaluationDelegate, subjectId);
		}
		Debug.Log("CharacterConditionsBuilder: Unknown condition type: " + parameter.type);
		return null;
	}


	// Get subjectId from parameter
	private static int GetSubjectId(Storage.GenericParameter parameter) {
		return parameter.SafeInt(0);
	}


	// Negation delegate
	private static EventCondition<GameEntityModel>.EvaluationDelegate FilterNegation(EventCondition<GameEntityModel>.EvaluationDelegate evaluationDelegate, Storage.GenericParameter parameter){
		if (parameter.SafeBool(0)){
			return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
				return !evaluationDelegate(model, subjectModels);
			};
		}
		return evaluationDelegate;
	}



#region Conditions

	// Frame
	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildFrame(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		keyFrame = InvalidKeyframe;
		// Read subject, operator, numerator subject, numerator variable, frame number
		CharacterSubjectsBuilder.SubjectOption subjectId = (CharacterSubjectsBuilder.SubjectOption)parameter.SafeInt(0);
		ConditionUtils<GameEntityModel>.ComparisonOperation comparisonOperator = (ConditionUtils<GameEntityModel>.ComparisonOperation)parameter.SafeInt(1);
		CharacterSubjectsBuilder.SubjectOption numeratorSubjectId = (CharacterSubjectsBuilder.SubjectOption)parameter.SafeInt(2);
		string numeratorSubjectVarName = parameter.SafeString(0);
		int staticComparisonFrame = parameter.SafeInt(3);

		// If it's equal to a static frame, no delegate required, return frame directly
		if (subjectId == CharacterSubjectsBuilder.SubjectOption.self
			&& comparisonOperator == ConditionUtils<GameEntityModel>.ComparisonOperation.equal
		    && numeratorSubjectId == CharacterSubjectsBuilder.SubjectOption.none
		){
			keyFrame = parameter.SafeInt(1);
			if (keyFrame < 0)
				keyFrame = animation.numFrames;
			return null;
		}
		// Else return delegate
		return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
			List<GameEntityModel> mainSubject;
			mainSubject	= ConditionUtils<GameEntityModel>.GetNonEmptySubjectOrNil(subjectModels, (int)subjectId);
			if (mainSubject == null) return false;
			AnimationModel animModel;
			if (numeratorSubjectId != CharacterSubjectsBuilder.SubjectOption.none){
				List<GameEntityModel> comparisonSubject = ConditionUtils<GameEntityModel>.GetNonEmptySubjectOrNil(subjectModels, (int)numeratorSubjectId);
				if (comparisonSubject == null) return false;
				// compare each model's frame with each comparison subject variable, return true if all pass
				int variableValue;
				foreach (GameEntityModel mainModel in mainSubject) {
					animModel = StateManager.state.GetModel(mainModel.animationModelId) as AnimationModel;
					foreach (GameEntityModel comparisonModel in comparisonSubject) {
						if (!comparisonModel.customVariables.TryGetValue(numeratorSubjectVarName, out variableValue)) {
							return false;
						}
						if (!ConditionUtils<GameEntityModel>.Compare(comparisonOperator, (int)animModel.currentFrame, variableValue)){
							return false;
						}
					}
				}
			}else {
				// compare each model's frame with static frame number, return true if all pass
				foreach (GameEntityModel mainModel in mainSubject) {
					animModel = StateManager.state.GetModel(mainModel.animationModelId) as AnimationModel;
					if (!ConditionUtils<GameEntityModel>.Compare(comparisonOperator, (int)animModel.currentFrame, staticComparisonFrame)){
						return false;
					}
				}
			}
			return true;
		};
	}

	// Auxiliar method to get Oriented Axis
	private static FixedFloat getOrientedAxisValue(FixedVector3 axis, CharacterConditionsBuilder.Orientation orientation, bool useModule){
		FixedFloat inputAxisValue = FixedFloat.Zero;
		switch(orientation){
			case Orientation.horizontal: inputAxisValue = axis.X; break;
			case Orientation.vertical:   inputAxisValue = axis.Y; break;
			case Orientation.any:		 inputAxisValue = axis.Magnitude; break;
		}
		if (useModule) {
			inputAxisValue = FixedFloat.Abs(inputAxisValue);
		}
		return inputAxisValue;
	}


	// Input Velocity
	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildInputVelocity(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		keyFrame = InvalidKeyframe;
		// Read subject, orientation, operator, numerator subject, numerator var, number, module
		CharacterSubjectsBuilder.SubjectOption subjectId = (CharacterSubjectsBuilder.SubjectOption)parameter.SafeInt(0);
		CharacterConditionsBuilder.Orientation orientation = (CharacterConditionsBuilder.Orientation)parameter.SafeInt(1);
		ConditionUtils<GameEntityModel>.ComparisonOperation comparisonOperator = (ConditionUtils<GameEntityModel>.ComparisonOperation)parameter.SafeInt(2);
		CharacterSubjectsBuilder.SubjectOption numeratorSubjectId = (CharacterSubjectsBuilder.SubjectOption)parameter.SafeInt(3);
		string numeratorSubjectVarName = parameter.SafeString(0);
		FixedFloat staticComparisonValue = parameter.SafeFloat(0);
		bool useModule = parameter.SafeBool(1);

		// return delegate
		return delegate(GameEntityModel model, List<GameEntityModel>[] subjectModels){
			List<GameEntityModel> mainSubject;
			mainSubject	= ConditionUtils<GameEntityModel>.GetNonEmptySubjectOrNil(subjectModels, (int)subjectId);
			if (mainSubject == null) return false;
			FixedFloat inputAxisValue;
			PlayerInputModel inputModel;
			if (numeratorSubjectId != CharacterSubjectsBuilder.SubjectOption.none){
				List<GameEntityModel> comparisonSubject = ConditionUtils<GameEntityModel>.GetNonEmptySubjectOrNil(subjectModels, (int)numeratorSubjectId);
				if (comparisonSubject == null) return false;
				// compare each model's velocity with each comparison subject variable, return true if all pass
				int variableValue;
				foreach (GameEntityModel mainModel in mainSubject) {
					inputModel = StateManager.state.GetModel(mainModel.inputModelId) as PlayerInputModel;
					foreach (GameEntityModel comparisonModel in comparisonSubject) {
						if (!comparisonModel.customVariables.TryGetValue(numeratorSubjectVarName, out variableValue)) {
							return false;
						}
						inputAxisValue = getOrientedAxisValue(inputModel.axis, orientation, useModule);
						if (!ConditionUtils<GameEntityModel>.Compare(comparisonOperator, inputAxisValue, (FixedFloat) variableValue)){
							return false;
						}
					}
				}
			}else {
				// compare each model's frame with static frame number, return true if all pass
				foreach (GameEntityModel mainModel in mainSubject) {
					inputModel = StateManager.state.GetModel(mainModel.inputModelId) as PlayerInputModel;
					inputAxisValue = getOrientedAxisValue(inputModel.axis, orientation, useModule);
					if (!ConditionUtils<GameEntityModel>.Compare(comparisonOperator, inputAxisValue, staticComparisonValue)){
						return false;
					}
				}
			}
			return true;
		};
	}

	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildInputButton(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		return null;
	}

	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildGrounded(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		return null;
	}

	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildFacingRight(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		return null;
	}

	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildCollisionImpact(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		return null;
	}

	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildVariable(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		return null;
	}

	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildVelocity(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		return null;
	}

	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildExistence(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		return null;
	}

	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildTeam(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		return null;
	}

	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildName(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		return null;
	}

	private static EventCondition<GameEntityModel>.EvaluationDelegate BuildAnimation(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
		return null;
	}




#endregion


}

}
