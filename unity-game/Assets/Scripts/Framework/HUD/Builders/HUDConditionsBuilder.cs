using System;
using System.Collections.Generic;
using UnityEngine;

namespace RetroBread{


	public static class HUDConditionsBuilder {

		private static int negationParamId = 0;



		// Condition builders indexed by type directly on array
		private delegate GenericTriggerCondition<HUDViewBehaviour> BuilderAction(Storage.GenericParameter parameter);
		private static BuilderAction[] builderActions = {
			BuildOnEnable,			// 0: enable / disable
			BuildOnVariableChange,	// 1: changed(energy)
			BuildOnVariableValue,	// 2: energy >= 4
			BuildOnAnimationPlaying // 3: animation(getUp)
		};
			

		// The public builder method
		public static GenericTriggerCondition<HUDViewBehaviour> Build(Storage.HUD hudData, int[] conditionIds){
			List<GenericTriggerCondition<HUDViewBehaviour>> conditions = new List<GenericTriggerCondition<HUDViewBehaviour>>(conditionIds.Length);
			GenericTriggerCondition<HUDViewBehaviour> condition;
			foreach (int conditionId in conditionIds) {
				condition = BuildFromParameter(hudData.genericParameters[conditionId]);
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
		private static GenericTriggerCondition<HUDViewBehaviour> BuildFromParameter(Storage.GenericParameter parameter){
			GenericTriggerCondition<HUDViewBehaviour> condition;
			int callIndex = parameter.type;
			if (callIndex < builderActions.Length) {
				condition = builderActions[callIndex](parameter);
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

		// Condition that selects a different condition depending on hit/grab delegation options
		private class HUDConditionDelegationChecker:GenericTriggerCondition<HUDViewBehaviour>{
			private GenericTriggerCondition<HUDViewBehaviour> nonDelegationCondition;
			private GenericTriggerCondition<HUDViewBehaviour> delegationCondition;

			public HUDConditionDelegationChecker(GenericTriggerCondition<HUDViewBehaviour> nonDelegationCondition, GenericTriggerCondition<HUDViewBehaviour> delegationCondition){
				this.nonDelegationCondition = nonDelegationCondition;
				this.delegationCondition = delegationCondition;
			}

			public bool Evaluate(HUDViewBehaviour model){
				Storage.HUDObject hudObj = model.hudObjectData;
				if (hudObj.attackAndGrabDelegation) {
					return delegationCondition.Evaluate(model);
				} else {
					return nonDelegationCondition.Evaluate(model);
				}
			}
		}


		// Check when a character enters or leaves from existance
		private class HUDEnableCondition:GenericTriggerCondition<HUDViewBehaviour>{
			private bool isFirstTime = true;
			private bool currentlyEnabled = false;
			private bool expectedEnable;

			public HUDEnableCondition(bool expectedEnable) {
				this.expectedEnable = expectedEnable;
			}

			public bool Evaluate(HUDViewBehaviour model){
				Storage.HUDObject hudObj = model.hudObjectData;
				if (hudObj == null) return false;
				GameEntityModel entity = WorldUtils.GetEntityFromTeam(hudObj.teamId, hudObj.playerId);
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


			public HUDDelegateEnableCondition(bool expectedEnable) {
				this.expectedEnable = expectedEnable;
			}

			public bool Evaluate(HUDViewBehaviour model){
				Storage.HUDObject hudObj = model.hudObjectData;
				if (hudObj == null) return false;
				ModelReference exception = lastEntityReference == null ? new ModelReference() : lastEntityReference;
				GameEntityModel entity = WorldUtils.GetInteractionEntityWithEntityFromTeam(hudObj.teamId, hudObj.playerId, exception);
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

			public HUDVariableChangeCondition(string variableName) {
				this.variableName = variableName;
			}

			public bool Evaluate(HUDViewBehaviour model){
				Storage.HUDObject hudObj = model.hudObjectData;
				if (hudObj == null) return false;
				GameEntityModel entity = WorldUtils.GetEntityFromTeam(hudObj.teamId, hudObj.playerId);
				// Check variable value
				int newValue;
				if (entity != null && entity.customVariables.TryGetValue(variableName, out newValue)) {
					if (previousValue != newValue) {
						bool firstTime = previousValue == int.MinValue;
						previousValue = newValue;
						return !firstTime; // Note: discarding first time, use enable instead..
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

			public HUDDelegateVariableChangeCondition(string variableName) {
				this.variableName = variableName;
			}

			public bool Evaluate(HUDViewBehaviour model){
				Storage.HUDObject hudObj = model.hudObjectData;
				if (hudObj == null) return false;
				ModelReference exception = lastEntityReference == null ? new ModelReference() : lastEntityReference;
				GameEntityModel entity = WorldUtils.GetInteractionEntityWithEntityFromTeam(hudObj.teamId, hudObj.playerId, exception);
				if (entity == null && lastEntityReference != null && lastEntityReference != ModelReference.InvalidModelIndex) {
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

			public HUDDelegateVariableArithmeticsCondition(ArithmeticConditionOperatorType type, string variableName, int value)
				:base(type, null, value)
			{
				this.variableName = variableName;
				this.getLeftVariableDelegate = this.ComparisonFunction;
			}

			private int ComparisonFunction(HUDViewBehaviour model){
				Storage.HUDObject hudObj = model.hudObjectData;
				if (hudObj == null) return int.MinValue;
				ModelReference exception = lastEntityReference == null ? new ModelReference() : lastEntityReference;
				GameEntityModel entity = WorldUtils.GetInteractionEntityWithEntityFromTeam(hudObj.teamId, hudObj.playerId, exception);
				if (entity == null && lastEntityReference != null && lastEntityReference != ModelReference.InvalidModelIndex) {
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
		private static GenericTriggerCondition<HUDViewBehaviour> BuildOnEnable(Storage.GenericParameter parameter){
			bool enabled = !parameter.SafeBool(0); // negation because it's using the negation parameter
			return new HUDConditionDelegationChecker(
				new HUDEnableCondition(enabled),
				new HUDDelegateEnableCondition(enabled)
			);
		}


		// 1: changed(energy)
		private static GenericTriggerCondition<HUDViewBehaviour> BuildOnVariableChange(Storage.GenericParameter parameter){
			return new HUDConditionDelegationChecker(
				new HUDVariableChangeCondition(parameter.SafeString(0)),
				new HUDDelegateVariableChangeCondition(parameter.SafeString(0))
			);
		}


		// 2: energy >= 4
		private static GenericTriggerCondition<HUDViewBehaviour> BuildOnVariableValue(Storage.GenericParameter parameter){
			ArithmeticConditionOperatorType type = (ArithmeticConditionOperatorType) parameter.SafeInt(1);
			string variableName = parameter.SafeString(0);
			int comparisonValue = parameter.SafeInt(0);

			return new HUDConditionDelegationChecker(
				new ArithmeticCondition<HUDViewBehaviour, int>(
					type,
					delegate(HUDViewBehaviour model) {
						Storage.HUDObject hudObj = model.hudObjectData;
						if (hudObj == null) return int.MinValue;
						GameEntityModel entity = WorldUtils.GetEntityFromTeam(hudObj.teamId, hudObj.playerId);
						if (entity != null) {
							int value;
							if (entity.customVariables.TryGetValue(variableName, out value)) {
								return value;
							}
						}
						return int.MinValue;
					},
					comparisonValue
				),
				new HUDDelegateVariableArithmeticsCondition(type, variableName, comparisonValue)
			);
		}


		// 3: animation(getUp)
		private static GenericTriggerCondition<HUDViewBehaviour> BuildOnAnimationPlaying(Storage.GenericParameter parameter){
			return new BoolCondition<HUDViewBehaviour>(
				delegate(HUDViewBehaviour model){
					Animator animator = model.gameObject.GetComponent<Animator>();
					if (animator == null) return false;
					AnimatorStateInfo stateInfo;
					// Get current animation (if transiting we consider next as current)
					if (animator.IsInTransition(0)){
						stateInfo = animator.GetNextAnimatorStateInfo(0);
					}else {
						stateInfo = animator.GetCurrentAnimatorStateInfo(0);
					}
					return stateInfo.IsName(parameter.SafeString(0));
				},
				true
			);
		}



#endregion


	}

}
