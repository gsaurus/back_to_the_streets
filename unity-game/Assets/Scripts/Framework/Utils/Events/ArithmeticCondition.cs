using System;
using UnityEngine;
using System.Collections.Generic;



namespace RetroBread{


	// Possible operators of arithmetic conditions
	public enum ArithmeticConditionOperatorType:int {
		equal = 0,
		notEqual = 1,
		less = 2,
		lessOrEqual = 3,
		greater = 4,
		greaterOrEqual = 5
	}


	// Arithmetic comparison condition:
	// Compare two values. Values are obtained via getter delegates
	// Delegates should be obtained from controllers
	// Right value can be a constant
	public class ArithmeticCondition<T>: GenericTriggerCondition<AnimationModel> where T:IComparable<T>{

		// Delegate of the getters
		public delegate T GetArithmeticConditionVariable(AnimationModel model);

		// Left and right value getters delegates
		protected GetArithmeticConditionVariable getLeftVariableDelegate;
		protected GetArithmeticConditionVariable getRightVariableDelegate;
		// A constant for the right value
		protected T rightValue;

		// Condition operator
		protected ArithmeticConditionOperatorType conditionOperator;

		// Explicit default constructor
		protected ArithmeticCondition(){
			// Nothing to do here..
		}

		// Constructor with two getter delegates
		public ArithmeticCondition(
			ArithmeticConditionOperatorType operatorType,
			GetArithmeticConditionVariable leftVariableDelegate,
			GetArithmeticConditionVariable rightVariableDelegate
		){
			this.conditionOperator = operatorType;
			this.getLeftVariableDelegate = leftVariableDelegate;
			this.getRightVariableDelegate = rightVariableDelegate;
		}

		// Constructor with left delegate and right const value
		public ArithmeticCondition(
			ArithmeticConditionOperatorType operatorType,
			GetArithmeticConditionVariable leftVariableDelegate,
			T rightValue
		){
			this.conditionOperator = operatorType;
			this.getLeftVariableDelegate = leftVariableDelegate;
			this.rightValue = rightValue;
		}


		// Evaluate the condition
		public bool Evaluate(AnimationModel model){

			// obtain left & right values
			T lvalue, rvalue;
			lvalue = getLeftVariableDelegate(model);
			if (getRightVariableDelegate != null) {
				rvalue = getRightVariableDelegate(model);
			}else {
				rvalue = rightValue;
			}

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
				case ArithmeticConditionOperatorType.lessOrEqual:{
					return result <= 0;
				}
				case ArithmeticConditionOperatorType.greater:{
					return result > 0;
				}
				case ArithmeticConditionOperatorType.greaterOrEqual:{
					return result >= 0;
				}
			}
			// won't reach here, but compiler complains, so..
			return false;
		}

	}



}
