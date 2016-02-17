using System;
using UnityEngine;
using System.Collections.Generic;



namespace RetroBread{


	// Parameterless event
	public class SimpleAnimationEvent: AnimationEvent{
		
		// Delegate of the event
		public delegate void EventExecutionDelegate(AnimationModel model);
		
		private EventExecutionDelegate eventExecutionDelegate;
		
		
		// Constructor with delegate and parameters
		public SimpleAnimationEvent(AnimationTriggerCondition condition, EventExecutionDelegate eventDelegate)
		:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
		}
		
		
		// Execute event trough the delegate
		protected override void Execute(AnimationModel model){
			eventExecutionDelegate(model);
		}
	}


	// Event with a single parameter
	public class SingleAnimationEvent<T>: AnimationEvent{
		
		// Delegate of the event
		public delegate void EventExecutionDelegate(AnimationModel model, T param);
		
		private EventExecutionDelegate eventExecutionDelegate;
		
		// Parameter
		private T param;
		
		
		// Constructor with delegate and parameters
		public SingleAnimationEvent(AnimationTriggerCondition condition, EventExecutionDelegate eventDelegate, T param)
		:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
			this.param = param;
		}
		
		
		// Execute event trough the delegate
		protected override void Execute(AnimationModel model){
			eventExecutionDelegate(model, param);
		}
	}


	// Event with two parameters
	public class DoubleAnimationEvent<U, V>: AnimationEvent{
		
		// Delegate of the event
		public delegate void EventExecutionDelegate(AnimationModel model, U param1, V param2);
		
		private EventExecutionDelegate eventExecutionDelegate;
		
		// Parameter
		private U param1;
		private V param2;
		
		
		// Constructor with delegate and parameters
		public DoubleAnimationEvent(AnimationTriggerCondition condition, EventExecutionDelegate eventDelegate, U param1, V param2)
		:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
			this.param1 = param1;
			this.param2 = param2;
		}
		
		
		// Execute event trough the delegate
		protected override void Execute(AnimationModel model){
			eventExecutionDelegate(model, param1, param2);
		}
	}



	// An event that can hold a variadic number of parameters to be executed
	public class VariadicAnimationEvent: AnimationEvent{
		
		// Delegate of the event
		public delegate void EventExecutionDelegate(AnimationModel model, params object[] parameters);
		
		private EventExecutionDelegate eventExecutionDelegate;
		
		// Parameters
		private object[] parameters;
		
		
		// Constructor with delegate and parameters
		public VariadicAnimationEvent(
			AnimationTriggerCondition condition, 
			EventExecutionDelegate eventDelegate,
			params object[] parameters
		):base(condition){
			this.eventExecutionDelegate = eventDelegate;
			this.parameters = parameters;
		}
		
		
		// Execute event trough the delegate
		protected override void Execute(AnimationModel model){
			eventExecutionDelegate(model, parameters);
		}
		
	}



}