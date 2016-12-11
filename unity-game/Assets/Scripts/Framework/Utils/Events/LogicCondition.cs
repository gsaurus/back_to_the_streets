using System;
using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{



// Boolean condition with delegates
// Delegates should be obtained from controllers
// Right value can be a constant
public class BoolCondition<T>: GenericTriggerCondition<T>{

	// Delegate of the getters
	public delegate bool BoolConditionDelegate(T model, List<GenericEventSubject<T>> subjects);

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
	public bool Evaluate(T model, List<GenericEventSubject<T>> subjects){
		
		// obtain left & right values
		bool lvalue, rvalue;
		lvalue = getLeftVariableDelegate(model, subjects);
		if (getRightVariableDelegate != null) {
			rvalue = getRightVariableDelegate(model, subjects);
		}else {
			rvalue = rightValue;
		}
		
		// compare them
		return lvalue == rvalue;
	}
	
}


public class NegateCondition<T>: GenericTriggerCondition<T>{
	private GenericTriggerCondition<T> originalCondition;

	public NegateCondition(GenericTriggerCondition<T> originalCondition){
		this.originalCondition = originalCondition;
	}

	public bool Evaluate(T model, List<GenericEventSubject<T>> subjects){
		return !originalCondition.Evaluate(model, subjects);
	}

}


public class ConditionsList<T>: GenericTriggerCondition<T>{
	private List<GenericTriggerCondition<T>> conditions;

	public ConditionsList(List<GenericTriggerCondition<T>> conditions){
		this.conditions = conditions;
	}

	public bool Evaluate(T model, List<GenericEventSubject<T>> subjects){
		if (conditions == null) return true;
		foreach (GenericTriggerCondition<T> condition in conditions){
			if (!condition.Evaluate(model, subjects)){
				return false;
			}
		}
		return true;
	}

}



}

