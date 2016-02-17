using System;
using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{



	// Boolean condition with delegates
	// Delegates should be obtained from controllers
	// Right value can be a constant
	public class BoolCondition: AnimationTriggerCondition{

		// Delegate of the getters
		public delegate bool BoolConditionDelegate(AnimationModel model);

		// Left and right value getters delegates
		protected BoolConditionDelegate getLeftVariableDelegate;
		protected BoolConditionDelegate getRightVariableDelegate;
		// A constant for the right value
		protected bool rightValue;

		// Explicit default constructor
		protected BoolCondition(){
			// Nothing to do here..
		}
		
		// Constructor with two getter delegates
		public BoolCondition(
			BoolConditionDelegate leftVariableDelegate,
			BoolConditionDelegate rightVariableDelegate
		){
			this.getLeftVariableDelegate = leftVariableDelegate;
			this.getRightVariableDelegate = rightVariableDelegate;
		}
		
		// Constructor with left delegate and right const value
		public BoolCondition(
			BoolConditionDelegate leftVariableDelegate,
			bool rightValue
		){
			this.getLeftVariableDelegate = leftVariableDelegate;
			this.rightValue = rightValue;
		}
		
		
		// Evaluate the condition
		public bool Evaluate(AnimationModel model){
			
			// obtain left & right values
			bool lvalue, rvalue;
			lvalue = getLeftVariableDelegate(model);
			if (getRightVariableDelegate != null) {
				rvalue = getRightVariableDelegate(model);
			}else {
				rvalue = rightValue;
			}
			
			// compare them
			return lvalue == rvalue;
		}
		
	}


	public class NegateCondition: AnimationTriggerCondition{
		private AnimationTriggerCondition originalCondition;

		public NegateCondition(AnimationTriggerCondition originalCondition){
			this.originalCondition = originalCondition;
		}

		public bool Evaluate(AnimationModel model){
			return !originalCondition.Evaluate(model);
		}

	}


	public class ConditionsList: AnimationTriggerCondition{
		private List<AnimationTriggerCondition> conditions;

		public ConditionsList(List<AnimationTriggerCondition> conditions){
			this.conditions = conditions;
		}

		public bool Evaluate(AnimationModel model){
			if (conditions == null) return true;
			foreach (AnimationTriggerCondition condition in conditions){
				if (!condition.Evaluate(model)){
					return false;
				}
			}
			return true;
		}

	}



}

