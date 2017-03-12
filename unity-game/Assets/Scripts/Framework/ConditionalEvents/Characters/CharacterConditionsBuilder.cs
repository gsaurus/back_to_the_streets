using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RetroBread{


	public static class CharacterConditionsBuilder {

		public static int InvalidKeyframe = -1;

		private const string defaultVariableTeam = "team";
		private const string defaultVariableCharacterNumber = "character";

		private enum Orientation {
			horizontal	= 0,
			vertical	= 1,
			any			= 2,
			z			= 3
		}

		private enum ButtonState {
			press	= 0,
			hold	= 1,
			release	= 2
		}

		private enum AnyOrAllOptions{
			anyOf = 0,
			allBut = 1
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
			BuildGlobalVariable,
			BuildVelocity,
			BuildExistence,
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







	#region Conditions





		// Frame
		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildFrame(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read subject, operator, numerator subject, numerator variable, frame number
			int subjectId = parameter.SafeInt(0);
			ConditionUtils<GameEntityModel>.ComparisonOperation comparisonOperator = (ConditionUtils<GameEntityModel>.ComparisonOperation)parameter.SafeInt(1);
			int numeratorSubjectId = parameter.SafeInt(2);
			string numeratorSubjectVarName = parameter.SafeString(0);
			int staticComparisonFrame = parameter.SafeInt(3);

			// If it's equal to a static frame, no delegate required, return frame directly
			if (subjectId == (int)CharacterSubjectsBuilder.PredefinedSubjects.self
				&& comparisonOperator == ConditionUtils<GameEntityModel>.ComparisonOperation.equal
				&& numeratorSubjectId == (int)CharacterSubjectsBuilder.PredefinedSubjects.none
			){
				keyFrame = staticComparisonFrame;
				if (keyFrame < 0)
					keyFrame = animation.numFrames;
				return null;
			}

			// Else return delegate
			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				AnimationModel animModel = StateManager.state.GetModel(mainModel.animationModelId) as AnimationModel;
				if (animModel == null) return false;
				return CompareWithNumerator(mainModel, numeratorSubjectId, numeratorSubjectVarName, (int)animModel.currentFrame, staticComparisonFrame, comparisonOperator, subjectModels);
			};
		}









		// Auxiliar method to get Oriented Axis
		private static FixedFloat getOrientedAxisValue(FixedVector3 axis, Orientation orientation, bool useModule){
			FixedFloat axisValue = FixedFloat.Zero;
			switch(orientation){
				case Orientation.horizontal: axisValue = axis.X; break;
				case Orientation.vertical:   axisValue = axis.Y; break;
				case Orientation.z:			 axisValue = axis.Z; break;
				case Orientation.any:		 axisValue = axis.Magnitude; break;
			}
			if (useModule) {
				axisValue = FixedFloat.Abs(axisValue);
			}
			return axisValue;
		}
			
		// Input Velocity
		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildInputVelocity(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read orientation, operator, numerator subject, numerator var, number, module
			Orientation	orientation = (Orientation)parameter.SafeInt(1);
			ConditionUtils<GameEntityModel>.ComparisonOperation comparisonOperator = (ConditionUtils<GameEntityModel>.ComparisonOperation)parameter.SafeInt(2);
			int			numeratorSubjectId		= parameter.SafeInt(3);
			string		numeratorSubjectVarName	= parameter.SafeString(0);
			FixedFloat	staticComparisonValue	= parameter.SafeFloat(0);
			bool		useModule				= parameter.SafeBool(1);

			// return delegate
			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				PlayerInputModel inputModel = StateManager.state.GetModel(mainModel.inputModelId) as PlayerInputModel;
				if (inputModel == null) return false;
				FixedFloat inputAxisValue = getOrientedAxisValue(inputModel.axis, orientation, useModule);
				return CompareWithNumerator(mainModel, numeratorSubjectId, numeratorSubjectVarName, inputAxisValue, staticComparisonValue, comparisonOperator, subjectModels);
			};
		}







		// Input Button
		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildInputButton(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read button, state, negation
			uint		buttonId		= (uint) parameter.SafeInt(1);
			ButtonState	buttonState		= (ButtonState)parameter.SafeInt(2);
			bool		positiveCheck	= !parameter.SafeBool(0);

			// Return delegate
			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				PlayerInputModel inputModel = StateManager.state.GetModel(mainModel.inputModelId) as PlayerInputModel;
				if (inputModel == null) return false;
				PlayerInputController inputController = inputModel.Controller() as PlayerInputController;
				if (inputController == null) return false;
				bool verifies = false;
				// verify model's button state
				switch(buttonState) {
					case ButtonState.hold:		verifies = inputController.IsButtonHold(inputModel, buttonId);		break;
					case ButtonState.press:		verifies = inputController.IsButtonPressed(inputModel, buttonId);	break;
					case ButtonState.release:	verifies = inputController.IsButtonReleased(inputModel, buttonId);	break;
				}
				return verifies == positiveCheck;
			};
		}








		// Grounded
		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildGrounded(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read negation
			bool positiveCheck = !parameter.SafeBool(0);

			// Return delegate
			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				PhysicPointModel pointModel;
				pointModel = StateManager.state.GetModel(mainModel.physicsModelId) as PhysicPointModel;
				if (pointModel == null) return false;
				return PhysicPointController.IsGrounded(pointModel) == positiveCheck;
			};
		}








		// Facing right
		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildFacingRight(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read negation
			bool positiveCheck = !parameter.SafeBool(0);

			// Return delegate
			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				return mainModel.IsFacingRight() == positiveCheck;
			};
		}








		// Collision Impact
		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildCollisionImpact(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read orientation, operator, numerator subject, numerator var, number, module
			Orientation	orientation = (Orientation)parameter.SafeInt(1);
			ConditionUtils<GameEntityModel>.ComparisonOperation comparisonOperator = (ConditionUtils<GameEntityModel>.ComparisonOperation)parameter.SafeInt(2);
			int			numeratorSubjectId		= parameter.SafeInt(3);
			string		numeratorSubjectVarName	= parameter.SafeString(0);
			FixedFloat	staticComparisonValue	= parameter.SafeFloat(0);
			bool		useModule				= parameter.SafeBool(1);

			// return delegate
			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				PhysicPointModel pointModel = StateManager.state.GetModel(mainModel.physicsModelId) as PhysicPointModel;
				if (pointModel == null) return false;
				FixedFloat impactValue = getOrientedAxisValue(pointModel.collisionInpact, orientation, useModule);
				return CompareWithNumerator(mainModel, numeratorSubjectId, numeratorSubjectVarName, impactValue, staticComparisonValue, comparisonOperator, subjectModels);
			};
		}







		// Variable
		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildVariable(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read var name, operator, numerator subject, numerator var, number, is timer
			string varName = parameter.SafeString(0);
			ConditionUtils<GameEntityModel>.ComparisonOperation comparisonOperator = (ConditionUtils<GameEntityModel>.ComparisonOperation)parameter.SafeInt(1);
			int			numeratorSubjectId		= parameter.SafeInt(2);
			string		numeratorSubjectVarName	= parameter.SafeString(1);
			int			staticComparisonValue	= parameter.SafeInt(3);

			// return delegate
			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				int varValue;
				if (!TryGetVariableValue(mainModel, varName, out varValue)){
					return false;
				}
				return CompareWithNumerator(mainModel, numeratorSubjectId, numeratorSubjectVarName, varValue, staticComparisonValue, comparisonOperator, subjectModels);
			};
		}







		// Global Variable
		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildGlobalVariable(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read var name, operator, numerator subject, numerator var, number, is timer
			string varName = parameter.SafeString(0);
			ConditionUtils<GameEntityModel>.ComparisonOperation comparisonOperator = (ConditionUtils<GameEntityModel>.ComparisonOperation)parameter.SafeInt(1);
			int			numeratorSubjectId		= parameter.SafeInt(2);
			string		numeratorSubjectVarName	= parameter.SafeString(1);
			int			staticComparisonValue	= parameter.SafeInt(3);

			// return delegate
			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				int varValue = 0;
				WorldModel worldModel = StateManager.state.MainModel as WorldModel;
				worldModel.globalVariables.TryGetValue(varName, out varValue);
				return CompareWithNumerator(mainModel, numeratorSubjectId, numeratorSubjectVarName, varValue, staticComparisonValue, comparisonOperator, subjectModels);
			};
		}









		// Velocity
		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildVelocity(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read orientation, operator, numerator subject, numerator var, number, module
			Orientation	orientation = (Orientation)parameter.SafeInt(1);
			ConditionUtils<GameEntityModel>.ComparisonOperation comparisonOperator = (ConditionUtils<GameEntityModel>.ComparisonOperation)parameter.SafeInt(2);
			int			numeratorSubjectId		= parameter.SafeInt(3);
			string		numeratorSubjectVarName	= parameter.SafeString(0);
			FixedFloat	staticComparisonValue	= parameter.SafeFloat(0);
			bool		useModule				= parameter.SafeBool(1);

			// return delegate
			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				PhysicPointModel pointModel = StateManager.state.GetModel(mainModel.physicsModelId) as PhysicPointModel;
				if (pointModel == null) return false;
				FixedFloat velocityValue = getOrientedAxisValue(pointModel.GetVelocity(), orientation, useModule);
				return CompareWithNumerator(mainModel, numeratorSubjectId, numeratorSubjectVarName, velocityValue, staticComparisonValue, comparisonOperator, subjectModels);
			};
		}






		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildExistence(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read target subject, negation
			int targerSubjectId	= parameter.SafeInt(1);
			bool positiveCheck	= !parameter.SafeBool(0);

			// Return delegate
			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				bool exists = subjectModels[targerSubjectId] != null && subjectModels[targerSubjectId].Count > 0;
				return exists == positiveCheck;
			};
		}




		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildName(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read anchor options, anchor IDs and if it's a single subject
			AnyOrAllOptions options = (AnyOrAllOptions)parameter.SafeInt(1);
			string[] names = parameter.SafeStringsList(0);

			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				AnimationModel animModel = StateManager.state.GetModel(mainModel.animationModelId) as AnimationModel;
				bool anyOf = options == AnyOrAllOptions.anyOf;
				// "smart" loop here
				foreach (string name in names) {
					if (animModel.characterName == name){
						return anyOf;
					}
				}
				return !anyOf;
			};
		}

		private static EventCondition<GameEntityModel>.EvaluationDelegate BuildAnimation(Storage.GenericParameter parameter, out int keyFrame, Storage.CharacterAnimation animation){
			keyFrame = InvalidKeyframe;
			// Read anchor options, anchor IDs and if it's a single subject
			AnyOrAllOptions options = (AnyOrAllOptions)parameter.SafeInt(1);
			string[] names = parameter.SafeStringsList(0);

			return delegate(GameEntityModel mainModel, List<GameEntityModel>[] subjectModels){
				AnimationModel animModel = StateManager.state.GetModel(mainModel.animationModelId) as AnimationModel;
				bool anyOf = options == AnyOrAllOptions.anyOf;
				// "smart" loop here
				foreach (string name in names) {
					if (animModel.animationName == name){
						return anyOf;
					}
				}
				return !anyOf;
			};
		}




	#endregion




	#region Auxiliar methods

		// TODO: move to somwhere handling global variables
		public static string ParseVariableValuesInGlobalName(GameEntityModel model, string text){
			GroupCollection groups = Regex.Match(text, @"\[(.*?)\]").Groups;
			int variableValue;
			string resultingText = text;
			foreach (Group g in groups) {
				if (TryGetVariableValue(model, g.Value, out variableValue)){
					resultingText.Replace(g.Value, variableValue + "");
				}
			}
			return resultingText;
		}

		// TODO: move to some utils
		public static bool TryGetVariableValue(GameEntityModel model, string varName, out int varValue){
			// Look at default variables
			if (varName.Equals(defaultVariableTeam)) {
				// team
				varValue = WorldUtils.GetEntityTeam(model.Index);
				return true;
			}
			// Look at model variables
			if (model.customVariables.TryGetValue (varName, out varValue)){
				return true;
			}
			varValue = -1;
			return false;
		}

		// CompareWithNumerator, int version
		// Comes in duplicate due to conversion FixedFloat to int not working with parameterized types
		private static bool CompareWithNumerator(
			GameEntityModel mainModel,
			int numeratorSubjectId,
			string numeratorSubjectVarName,
			int comparisonValue,
			int staticComparisonValue,
			ConditionUtils<GameEntityModel>.ComparisonOperation comparisonOperator,
			List<GameEntityModel>[] subjectModels
		){
			// no subject
			if (numeratorSubjectId == 0){
				return ConditionUtils<GameEntityModel>.Compare(comparisonOperator, comparisonValue, staticComparisonValue);
			}
			// global variable
			if (numeratorSubjectId == 1){

				// Global variable may have references to a team ID, character name, player number, etc
				numeratorSubjectVarName = ParseVariableValuesInGlobalName(mainModel, numeratorSubjectVarName);

				int globalVariableValue = 0;
				WorldModel worldModel = StateManager.state.MainModel as WorldModel;
				worldModel.globalVariables.TryGetValue(numeratorSubjectVarName, out globalVariableValue);
				return ConditionUtils<GameEntityModel>.Compare(comparisonOperator, comparisonValue, globalVariableValue);
			}
			// subject variable
			numeratorSubjectId -= 2;
			List<GameEntityModel> comparisonSubject = ConditionUtils<GameEntityModel>.GetNonEmptySubjectOrNil(subjectModels, numeratorSubjectId);
			if (comparisonSubject == null || comparisonSubject.Count == 0) return false;
			// compare each model's velocity with each comparison subject variable, return true if all pass
			int variableValue;
			foreach (GameEntityModel comparisonModel in comparisonSubject) {

				if (!TryGetVariableValue(comparisonModel, numeratorSubjectVarName, out variableValue)) {
					return false;
				}
				if (!ConditionUtils<GameEntityModel>.Compare(comparisonOperator, comparisonValue, variableValue)){
					return false;
				}
			}
			return true;
		}
		// CompareWithNumerator, FixedFloat version
		private static bool CompareWithNumerator(
			GameEntityModel mainModel,
			int numeratorSubjectId,
			string numeratorSubjectVarName,
			FixedFloat comparisonValue,
			FixedFloat staticComparisonValue,
			ConditionUtils<GameEntityModel>.ComparisonOperation comparisonOperator,
			List<GameEntityModel>[] subjectModels
		){
			// no subject
			if (numeratorSubjectId == 0){
				return ConditionUtils<GameEntityModel>.Compare(comparisonOperator, comparisonValue, staticComparisonValue);
			}
			// global variable
			if (numeratorSubjectId == 1){

				// Global variable may have references to a team ID, character name, player number, etc
				numeratorSubjectVarName = ParseVariableValuesInGlobalName(mainModel, numeratorSubjectVarName);

				int globalVariableValue = 0;
				WorldModel worldModel = StateManager.state.MainModel as WorldModel;
				worldModel.globalVariables.TryGetValue(numeratorSubjectVarName, out globalVariableValue);
				return ConditionUtils<GameEntityModel>.Compare(comparisonOperator, comparisonValue, (FixedFloat) globalVariableValue);
			}
			// subject variable
			numeratorSubjectId -= 2;
			List<GameEntityModel> comparisonSubject = ConditionUtils<GameEntityModel>.GetNonEmptySubjectOrNil(subjectModels, numeratorSubjectId);
			if (comparisonSubject == null || comparisonSubject.Count == 0) return false;
			// compare each model's velocity with each comparison subject variable, return true if all pass
			int variableValue;
			foreach (GameEntityModel comparisonModel in comparisonSubject) {
				if (!comparisonModel.customVariables.TryGetValue(numeratorSubjectVarName, out variableValue)) {
					return false;
				}
				if (!ConditionUtils<GameEntityModel>.Compare(comparisonOperator, comparisonValue, (FixedFloat) variableValue)){
					return false;
				}
			}
			return true;
		}

	#endregion


	}

}
