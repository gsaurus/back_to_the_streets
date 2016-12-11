using System;
using UnityEngine;
using System.Collections.Generic;



namespace RetroBread{


// Parameterless event// Parameterless event
public class SimpleEntityAnimationEvent: GenericEvent<GameEntityModel>{
	
	// Possible delegates
	public delegate void EventExecutionDelegate(GameEntityModel model, List<GenericEventSubject<GameEntityModel>> subjects);
	
	private EventExecutionDelegate eventExecutionDelegate;
	
	
	// Constructor with delegate and parameters
	public SimpleEntityAnimationEvent(GenericTriggerCondition<GameEntityModel> condition, EventExecutionDelegate eventDelegate)
	:base(condition)
	{
		this.eventExecutionDelegate = eventDelegate;
	}
	
	
	// Execute event trough the delegate
	public override void Execute(GameEntityModel entityModel, List<GenericEventSubject<GameEntityModel>> subjects){
		eventExecutionDelegate(entityModel, subjects);
	}
}


// Event with a single parameter
public class SingleEntityAnimationEvent<T>: GenericEvent<GameEntityModel>{
	
	// Possible delegates
	public delegate void EventExecutionDelegate(GameEntityModel model, List<GenericEventSubject<GameEntityModel>> subjects, T param);
	
	private EventExecutionDelegate eventExecutionDelegate;
	
	// Parameter
	private T param;
	
	
	// Constructor with delegate and parameters
	public SingleEntityAnimationEvent(GenericTriggerCondition<GameEntityModel> condition, EventExecutionDelegate eventDelegate, T param)
	:base(condition)
	{
		this.eventExecutionDelegate = eventDelegate;
		this.param = param;
	}
	
	
	// Execute event trough the delegate
	public override void Execute(GameEntityModel entityModel, List<GenericEventSubject<GameEntityModel>> subjects){
		eventExecutionDelegate(entityModel, subjects, param);
	}
}


// Event with two parameters
public class DoubleEntityAnimationEvent<U,V>: GenericEvent<GameEntityModel>{
	
	// Possible delegates
	public delegate void EventExecutionDelegate(GameEntityModel model, List<GenericEventSubject<GameEntityModel>> subjects, U param1, V param2);
	
	private EventExecutionDelegate eventExecutionDelegate;
	
	// Parameter
	private U param1;
	private V param2;
	
	
	// Constructor with delegate and parameters
	public DoubleEntityAnimationEvent(GenericTriggerCondition<GameEntityModel> condition, EventExecutionDelegate eventDelegate, U param1, V param2)
	:base(condition)
	{
		this.eventExecutionDelegate = eventDelegate;
		this.param1 = param1;
		this.param2 = param2;
	}
	
	
	// Execute event trough the delegate
	public override void Execute(GameEntityModel entityModel, List<GenericEventSubject<GameEntityModel>> subjects){
		eventExecutionDelegate(entityModel, subjects, param1, param2);
	}
}



// Event with three parameter
public class TrippleEntityAnimationEvent<U,V,K>: GenericEvent<GameEntityModel>{

	// Possible delegates
	public delegate void EventExecutionDelegate(GameEntityModel model, List<GenericEventSubject<GameEntityModel>> subjects, U param1, V param2, K param3);

	private EventExecutionDelegate eventExecutionDelegate;

	// Parameter
	private U param1;
	private V param2;
	private K param3;


	// Constructor with delegate and parameters
	public TrippleEntityAnimationEvent(GenericTriggerCondition<GameEntityModel> condition, EventExecutionDelegate eventDelegate, U param1, V param2, K param3)
		:base(condition)
	{
		this.eventExecutionDelegate = eventDelegate;
		this.param1 = param1;
		this.param2 = param2;
		this.param3 = param3;
	}


	// Execute event trough the delegate
	public override void Execute(GameEntityModel entityModel, List<GenericEventSubject<GameEntityModel>> subjects){
		eventExecutionDelegate(entityModel, subjects, param1, param2, param3);
	}
}



// Event with four parameters
public class QuadEntityAnimationEvent<U,V,K,L>: GenericEvent<GameEntityModel>{

	// Possible delegates
	public delegate void EventExecutionDelegate(GameEntityModel model, List<GenericEventSubject<GameEntityModel>> subjects, U param1, V param2, K param3, L param4);

	private EventExecutionDelegate eventExecutionDelegate;

	// Parameter
	private U param1;
	private V param2;
	private K param3;
	private L param4;


	// Constructor with delegate and parameters
	public QuadEntityAnimationEvent(GenericTriggerCondition<GameEntityModel> condition, EventExecutionDelegate eventDelegate, U param1, V param2, K param3, L param4)
		:base(condition)
	{
		this.eventExecutionDelegate = eventDelegate;
		this.param1 = param1;
		this.param2 = param2;
		this.param3 = param3;
		this.param4 = param4;
	}


	// Execute event trough the delegate
	public override void Execute(GameEntityModel entityModel, List<GenericEventSubject<GameEntityModel>> subjects){
		eventExecutionDelegate(entityModel, subjects, param1, param2, param3, param4);
	}
}


// Event with five parameters
public class PentaEntityAnimationEvent<U,V,K,L,M>: GenericEvent<GameEntityModel>{

	// Possible delegates
	public delegate void EventExecutionDelegate(GameEntityModel model, List<GenericEventSubject<GameEntityModel>> subjects, U param1, V param2, K param3, L param4, M param5);

	private EventExecutionDelegate eventExecutionDelegate;

	// Parameter
	private U param1;
	private V param2;
	private K param3;
	private L param4;
	private M param5;


	// Constructor with delegate and parameters
	public PentaEntityAnimationEvent(GenericTriggerCondition<GameEntityModel> condition, EventExecutionDelegate eventDelegate, U param1, V param2, K param3, L param4, M param5)
		:base(condition)
	{
		this.eventExecutionDelegate = eventDelegate;
		this.param1 = param1;
		this.param2 = param2;
		this.param3 = param3;
		this.param4 = param4;
		this.param5 = param5;
	}


	// Execute event trough the delegate
	public override void Execute(GameEntityModel entityModel, List<GenericEventSubject<GameEntityModel>> subjects){
		eventExecutionDelegate(entityModel, subjects, param1, param2, param3, param4, param5);
	}
}



}
