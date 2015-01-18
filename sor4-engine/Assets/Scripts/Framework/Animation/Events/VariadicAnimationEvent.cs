
using System;
using UnityEngine;
using System.Collections.Generic;


// 
public class VariadicAnimationEvent: AnimationEvent{

	// Delegate of the event
	public delegate void EventExecutionDelegate(AnimationModel model, params object[] parameters);

	private EventExecutionDelegate eventExecutionDelegate;
	
	// Parameters
	private object[] parameters;
	

	// Constructor with delegate and parameters
	public VariadicAnimationEvent(
		EventExecutionDelegate eventDelegate,
		params object[] parameters
	){
		this.eventExecutionDelegate = eventExecutionDelegate;
		this.parameters = parameters;
	}
	
	
	// Execute event trough the delegate
	public void Execute(AnimationModel model){
		eventExecutionDelegate(model, parameters);
	}
	
}

