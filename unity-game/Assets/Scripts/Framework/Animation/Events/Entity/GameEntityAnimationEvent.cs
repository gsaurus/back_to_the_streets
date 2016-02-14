using System;
using UnityEngine;
using System.Collections.Generic;



namespace RetroBread{


	// Parameterless event// Parameterless event
	public class SimpleEntityAnimationEvent: AnimationEvent{
		
		// Possible delegates
		public delegate void EventExecutionDelegate(GameEntityModel model);
		
		private EventExecutionDelegate eventExecutionDelegate;
		
		
		// Constructor with delegate and parameters
		public SimpleEntityAnimationEvent(EventExecutionDelegate eventDelegate){
			this.eventExecutionDelegate = eventDelegate;
		}
		
		
		// Execute event trough the delegate
		public void Execute(AnimationModel model){
			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			if (entityModel == null) return;
			eventExecutionDelegate(entityModel);
		}
	}


	// Event with a single parameter
	public class SingleEntityAnimationEvent<T>: AnimationEvent{
		
		// Possible delegates
		public delegate void EventExecutionDelegate(GameEntityModel model, T param);
		
		private EventExecutionDelegate eventExecutionDelegate;
		
		// Parameter
		private T param;
		
		
		// Constructor with delegate and parameters
		public SingleEntityAnimationEvent(EventExecutionDelegate eventDelegate, T param){
			this.eventExecutionDelegate = eventDelegate;
			this.param = param;
		}
		
		
		// Execute event trough the delegate
		public void Execute(AnimationModel model){
			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			if (entityModel == null) return;
			eventExecutionDelegate(entityModel, param);
		}
	}


	// Event with a single parameter
	public class DoubleEntityAnimationEvent<U,V>: AnimationEvent{
		
		// Possible delegates
		public delegate void EventExecutionDelegate(GameEntityModel model, U param1, V param2);
		
		private EventExecutionDelegate eventExecutionDelegate;
		
		// Parameter
		private U param1;
		private V param2;
		
		
		// Constructor with delegate and parameters
		public DoubleEntityAnimationEvent(EventExecutionDelegate eventDelegate, U param1, V param2){
			this.eventExecutionDelegate = eventDelegate;
			this.param1 = param1;
			this.param2 = param2;
		}
		
		
		// Execute event trough the delegate
		public void Execute(AnimationModel model){
			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			if (entityModel == null) return;
			eventExecutionDelegate(entityModel, param1, param2);
		}
	}



}
