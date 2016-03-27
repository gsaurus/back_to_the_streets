using System;
using UnityEngine;
using System.Collections.Generic;



namespace RetroBread{


	// Condition over an entity model (rather than just animation)
	// parameterless
	public class EntityBoolCondition: AnimationTriggerCondition{

		// Possible delegates
		public delegate bool ConditionExecutionDelegate(GameEntityModel model);

		private ConditionExecutionDelegate eventExecutionDelegate;


		public EntityBoolCondition(ConditionExecutionDelegate del){
			eventExecutionDelegate = del;
		}


		public bool Evaluate(AnimationModel model){
		
			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			if (entityModel != null){
				return eventExecutionDelegate(entityModel);
			}
			return false;
		}

	}


	public class SingleEntityBoolCondition<T>: AnimationTriggerCondition{

		// Possible delegates
		public delegate bool ConditionExecutionDelegate(GameEntityModel model, T param);

		private ConditionExecutionDelegate eventExecutionDelegate;
		private T param;


		public SingleEntityBoolCondition(ConditionExecutionDelegate del, T param){
			eventExecutionDelegate = del;
			this.param = param;
		}


		public bool Evaluate(AnimationModel model){

			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			if (entityModel != null){
				return eventExecutionDelegate(entityModel, param);
			}
			return false;
		}

	}



	// Arithmetics over entity model
	public class EntityArithmeticCondition<T>: ArithmeticCondition<T> where T:IComparable<T>{
		
		// Possible delegates
		public delegate T ConditionExecutionDelegate(GameEntityModel model);
		
		private ConditionExecutionDelegate leftDelegate;
		private ConditionExecutionDelegate rightDelegate;

		// Those two methods translate delegates receiving AnimationModel
		// into delegates receiving GameEntityModel
		private T ExecuteLeftConditionDelegate(AnimationModel model){
			GameEntityModel gameEntityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			return leftDelegate(gameEntityModel);
		}

		private T ExecuteRightConditionDelegate(AnimationModel model){
			GameEntityModel gameEntityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			return rightDelegate(gameEntityModel);
		}

		
		// Constructor with two getter delegates
		public EntityArithmeticCondition(
			ArithmeticConditionOperatorType operatorType,
			ConditionExecutionDelegate leftDelegate,
			ConditionExecutionDelegate rightDelegate
		){
			this.conditionOperator = operatorType;
			this.getLeftVariableDelegate = ExecuteLeftConditionDelegate;
			this.getRightVariableDelegate = ExecuteRightConditionDelegate;
			this.leftDelegate = leftDelegate;
			this.rightDelegate = rightDelegate;
		}
		
		// Constructor with left delegate and right const value
		public EntityArithmeticCondition(
			ArithmeticConditionOperatorType operatorType,
			ConditionExecutionDelegate leftDelegate,
			T rightValue
		){
			this.conditionOperator = operatorType;
			this.getLeftVariableDelegate = ExecuteLeftConditionDelegate;
			this.rightValue = rightValue;
			this.leftDelegate = leftDelegate;
		}
		
	}



}
