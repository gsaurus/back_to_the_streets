using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class HUDConditionsBuilder {

		private static int negationParamId = 0;



		// Condition builders indexed by type directly on array
		private delegate GenericTriggerCondition<HUDViewBehaviour> BuilderAction(Storage.GenericParameter param);
		private static BuilderAction[] builderActions = {
			BuildOnEnable,			// 0: enable / disable
			BuildOnVariableChange,	// 1: changed(energy)
			BuildOnVariableValue	// 2: energy >= 4

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
				if ( IsNegated(parameter) ) {
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
			private uint teamId;
			private uint playerId;

			public HUDEnableCondition(bool expectedEnable, uint teamId, uint playerId) {
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
			private uint teamId;
			private uint playerId;


			public HUDDelegateEnableCondition(bool expectedEnable, uint teamId, uint playerId) {
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




#endregion


#region Conditions


		// 0: enable / disable
		private static GenericTriggerCondition<HUDViewBehaviour> BuildOnEnable(Storage.GenericParameter parameter){
			// TODO: receive HUDObject to know team, player and if using delegation
			return null;
		}


		// 1: changed(energy)
		private static GenericTriggerCondition<HUDViewBehaviour> BuildOnVariableChange(Storage.GenericParameter parameter){
			return null;
		}


		// 2: energy >= 4
		private static GenericTriggerCondition<HUDViewBehaviour> BuildOnVariableValue(Storage.GenericParameter parameter){
			return null;
		}



#endregion


	}

}
