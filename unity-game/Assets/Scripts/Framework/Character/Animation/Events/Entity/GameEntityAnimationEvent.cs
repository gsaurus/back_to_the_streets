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
		public SimpleEntityAnimationEvent(AnimationTriggerCondition condition, EventExecutionDelegate eventDelegate)
		:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
		}
		
		
		// Execute event trough the delegate
		public override void Execute(AnimationModel model){
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
		public SingleEntityAnimationEvent(AnimationTriggerCondition condition, EventExecutionDelegate eventDelegate, T param)
		:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
			this.param = param;
		}
		
		
		// Execute event trough the delegate
		public override void Execute(AnimationModel model){
			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			if (entityModel == null) return;
			eventExecutionDelegate(entityModel, param);
		}
	}


	// Event with two parameters
	public class DoubleEntityAnimationEvent<U,V>: AnimationEvent{
		
		// Possible delegates
		public delegate void EventExecutionDelegate(GameEntityModel model, U param1, V param2);
		
		private EventExecutionDelegate eventExecutionDelegate;
		
		// Parameter
		private U param1;
		private V param2;
		
		
		// Constructor with delegate and parameters
		public DoubleEntityAnimationEvent(AnimationTriggerCondition condition, EventExecutionDelegate eventDelegate, U param1, V param2)
		:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
			this.param1 = param1;
			this.param2 = param2;
		}
		
		
		// Execute event trough the delegate
		public override void Execute(AnimationModel model){
			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			if (entityModel == null) return;
			eventExecutionDelegate(entityModel, param1, param2);
		}
	}



	// Event with three parameter
	public class TrippleEntityAnimationEvent<U,V,K>: AnimationEvent{

		// Possible delegates
		public delegate void EventExecutionDelegate(GameEntityModel model, U param1, V param2, K param3);

		private EventExecutionDelegate eventExecutionDelegate;

		// Parameter
		private U param1;
		private V param2;
		private K param3;


		// Constructor with delegate and parameters
		public TrippleEntityAnimationEvent(AnimationTriggerCondition condition, EventExecutionDelegate eventDelegate, U param1, V param2, K param3)
			:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
			this.param1 = param1;
			this.param2 = param2;
			this.param3 = param3;
		}


		// Execute event trough the delegate
		public override void Execute(AnimationModel model){
			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			if (entityModel == null) return;
			eventExecutionDelegate(entityModel, param1, param2, param3);
		}
	}



	// Event with four parameters
	public class QuadEntityAnimationEvent<U,V,K,L>: AnimationEvent{

		// Possible delegates
		public delegate void EventExecutionDelegate(GameEntityModel model, U param1, V param2, K param3, L param4);

		private EventExecutionDelegate eventExecutionDelegate;

		// Parameter
		private U param1;
		private V param2;
		private K param3;
		private L param4;


		// Constructor with delegate and parameters
		public QuadEntityAnimationEvent(AnimationTriggerCondition condition, EventExecutionDelegate eventDelegate, U param1, V param2, K param3, L param4)
			:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
			this.param1 = param1;
			this.param2 = param2;
			this.param3 = param3;
			this.param4 = param4;
		}


		// Execute event trough the delegate
		public override void Execute(AnimationModel model){
			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			if (entityModel == null) return;
			eventExecutionDelegate(entityModel, param1, param2, param3, param4);
		}
	}


	// Event with five parameters
	public class PentaEntityAnimationEvent<U,V,K,L,M>: AnimationEvent{

		// Possible delegates
		public delegate void EventExecutionDelegate(GameEntityModel model, U param1, V param2, K param3, L param4, M param5);

		private EventExecutionDelegate eventExecutionDelegate;

		// Parameter
		private U param1;
		private V param2;
		private K param3;
		private L param4;
		private M param5;


		// Constructor with delegate and parameters
		public PentaEntityAnimationEvent(AnimationTriggerCondition condition, EventExecutionDelegate eventDelegate, U param1, V param2, K param3, L param4, M param5)
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
		public override void Execute(AnimationModel model){
			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			if (entityModel == null) return;
			eventExecutionDelegate(entityModel, param1, param2, param3, param4, param5);
		}
	}



}
