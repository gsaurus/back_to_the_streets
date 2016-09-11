using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class HUDConditionsBuilder {

		private static int negationParamId = 0;



		// Condition builders indexed by type directly on array
		private delegate GenericTriggerCondition<HUDViewBehaviour> BuilderAction(Storage.HUDObject hudObj, Storage.GenericParameter parameter);
		private static BuilderAction[] builderActions = {
			BuildOnEnable,			// 0: enable / disable
			BuildOnVariableChange,	// 1: changed(energy)
			BuildOnVariableValue	// 2: energy >= 4

		};
			

		// The public builder method
		public static GenericTriggerCondition<HUDViewBehaviour> Build(Storage.HUD hudData, Storage.HUDObject hudObj, int[] conditionIds){
			List<GenericTriggerCondition<HUDViewBehaviour>> conditions = new List<GenericTriggerCondition<HUDViewBehaviour>>(conditionIds.Length);
			GenericTriggerCondition<HUDViewBehaviour> condition;
			foreach (int conditionId in conditionIds) {
				condition = BuildFromParameter(hudObj, hudData.genericParameters[conditionId]);
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
		private static GenericTriggerCondition<HUDViewBehaviour> BuildFromParameter(Storage.HUDObject hudObj, Storage.GenericParameter parameter){
			GenericTriggerCondition<HUDViewBehaviour> condition;
			int callIndex = parameter.type;
			if (callIndex < builderActions.Length) {
				condition = builderActions[callIndex](hudObj, parameter);
				if (IsNegated(parameter)) {
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


#region Helper Condition classes

		// Check when a character enters or leaves from existance
		private class HUDEnableCondition:GenericTriggerCondition<HUDViewBehaviour>{
			private bool isFirstTime = true;
			private bool currentlyEnabled = false;
			private bool expectedEnable;
			private int teamId;
			private int playerId;

			public HUDEnableCondition(bool expectedEnable, int teamId, int playerId) {
				this.expectedEnable = expectedEnable;
				this.teamId = teamId;
				this.playerId = playerId;
			}

			public bool Evaluate(HUDViewBehaviour model){
				GameEntityModel entity = WorldUtils.GetEntityFromTeam(teamId, playerId);
				bool isEnabled = entity != null;
				// this condition ensures it return true only once it becomes enabled/disabled
				if (isFirstTime || isEnabled != currentlyEnabled) {
					currentlyEnabled = isEnabled;
					isFirstTime = false;
					return isEnabled == expectedEnable;
				}
				return false;
			}

		}


		// Check when an entity gets an hitten or grabbed character
		private class HUDDelegateEnableCondition:GenericTriggerCondition<HUDViewBehaviour>{
			private ModelReference lastEntityReference = null;
			private bool expectedEnable;
			private int teamId;
			private int playerId;


			public HUDDelegateEnableCondition(bool expectedEnable, int teamId, int playerId) {
				this.expectedEnable = expectedEnable;
				this.teamId = teamId;
				this.playerId = playerId;
			}

			public bool Evaluate(HUDViewBehaviour model){
				ModelReference exception = lastEntityReference == null ? new ModelReference() : lastEntityReference;
				GameEntityModel entity = WorldUtils.GetInteractionEntityWithEntityFromTeam(teamId, playerId, exception);
				ModelReference newReference = entity == null ? new ModelReference() : entity.Index;
				// this condition ensures it return true only once it becomes enabled/disabled
				if (lastEntityReference == null || lastEntityReference != newReference) {
					lastEntityReference = newReference;
					return (lastEntityReference != ModelReference.InvalidModelIndex) == expectedEnable;
				}
				return false;
			}

		}



		// Check when a character variable changes it's value
		private class HUDVariableChangeCondition:GenericTriggerCondition<HUDViewBehaviour>{
			private string variableName;
			private int previousValue = int.MinValue;
			private int teamId;
			private int playerId;

			public HUDVariableChangeCondition(string variableName, int teamId, int playerId) {
				this.variableName = variableName;
				this.teamId = teamId;
				this.playerId = playerId;
			}

			public bool Evaluate(HUDViewBehaviour model){
				GameEntityModel entity = WorldUtils.GetEntityFromTeam(teamId, playerId);
				// Check variable value
				int newValue;
				if (entity != null && entity.customVariables.TryGetValue(variableName, out newValue)) {
					if (previousValue != newValue) {
						previousValue = newValue;
						return true;
					}
					return false;
				}
				previousValue = int.MinValue;
				return false;
			}

		}


		// Check when an hitten or grabbed character's variable changes
		private class HUDDelegateVariableChangeCondition:GenericTriggerCondition<HUDViewBehaviour>{
			private ModelReference lastEntityReference = null;
			private string variableName;
			private int previousValue = int.MinValue;
			private int teamId;
			private int playerId;

			public HUDDelegateVariableChangeCondition(string variableName, int teamId, int playerId) {
				this.variableName = variableName;
				this.teamId = teamId;
				this.playerId = playerId;
			}

			public bool Evaluate(HUDViewBehaviour model){
				ModelReference exception = lastEntityReference == null ? new ModelReference() : lastEntityReference;
				GameEntityModel entity = WorldUtils.GetInteractionEntityWithEntityFromTeam(teamId, playerId, exception);
				if (entity == null && lastEntityReference != ModelReference.InvalidModelIndex) {
					entity = StateManager.state.GetModel(lastEntityReference) as GameEntityModel;
				}
				lastEntityReference = entity == null ? new ModelReference() : entity.Index;
				// Check variable value
				int newValue;
				if (entity != null && entity.customVariables.TryGetValue(variableName, out newValue)) {
					if (previousValue != newValue) {
						previousValue = newValue;
						return true;
					}
					return false;
				}
				previousValue = int.MinValue;
				return false;
			}

		}


		// Hitten or grabbed character's variable arithmetics
		private class HUDDelegateVariableArithmeticsCondition:ArithmeticCondition<HUDViewBehaviour, int>{
			private ModelReference lastEntityReference = null;
			private string variableName;
			private int teamId;
			private int playerId;

			public HUDDelegateVariableArithmeticsCondition(ArithmeticConditionOperatorType type, string variableName, int value, int teamId, int playerId)
				:base(type, null, value)
			{
				this.variableName = variableName;
				this.teamId = teamId;
				this.playerId = playerId;
				this.getLeftVariableDelegate = this.ComparisonFunction;
			}

			private int ComparisonFunction(HUDViewBehaviour model){
				ModelReference exception = lastEntityReference == null ? new ModelReference() : lastEntityReference;
				GameEntityModel entity = WorldUtils.GetInteractionEntityWithEntityFromTeam(teamId, playerId, exception);
				if (entity == null && lastEntityReference != ModelReference.InvalidModelIndex) {
					entity = StateManager.state.GetModel(lastEntityReference) as GameEntityModel;
				}
				lastEntityReference = entity == null ? new ModelReference() : entity.Index;
				// Check variable value
				int newValue;
				if (entity != null && entity.customVariables.TryGetValue(variableName, out newValue)) {
					return newValue;
				}
				return int.MinValue;
			}

		}




#endregion


#region Conditions


		// 0: enable / disable
		private static GenericTriggerCondition<HUDViewBehaviour> BuildOnEnable(Storage.HUDObject hudObj, Storage.GenericParameter parameter){
			if (hudObj.attackAndGrabDelegation) {
				return new HUDDelegateEnableCondition(parameter.SafeBool(0), hudObj.teamId, hudObj.playerId);
			} else {
				return new HUDEnableCondition(parameter.SafeBool(0), hudObj.teamId, hudObj.playerId);
			}
		}


		// 1: changed(energy)
		private static GenericTriggerCondition<HUDViewBehaviour> BuildOnVariableChange(Storage.HUDObject hudObj, Storage.GenericParameter parameter){
			if (hudObj.attackAndGrabDelegation) {
				return new HUDDelegateVariableChangeCondition(parameter.SafeString(0), hudObj.teamId, hudObj.playerId);
			} else {
				return new HUDVariableChangeCondition(parameter.SafeString(0), hudObj.teamId, hudObj.playerId);
			}
		}


		// 2: energy >= 4
		private static GenericTriggerCondition<HUDViewBehaviour> BuildOnVariableValue(Storage.HUDObject hudObj, Storage.GenericParameter parameter){
			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType) parameter.SafeInt(1);
			string variableName = parameter.SafeString(0);
			int comparisonValue = parameter.SafeInt(0);
			if (hudObj.attackAndGrabDelegation) {
				return new ArithmeticCondition<HUDViewBehaviour, int>(
					type,
					delegate(HUDViewBehaviour model) {
						GameEntityModel entity = WorldUtils.GetEntityFromTeam(hudObj.teamId, hudObj.playerId);
						if (entity != null) {
							int value;
							if (entity.customVariables.TryGetValue(variableName, out value)){
								return value;
							}
						}
						return int.MinValue;
					},
					comparisonValue
				);
			} else {
				return new HUDDelegateVariableArithmeticsCondition(type, variableName, comparisonValue, hudObj.teamId, hudObj.playerId);
			}
		}



#endregion


	}

}
