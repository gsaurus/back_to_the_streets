using System;
using System.Collections.Generic;

namespace RetroBread{
	
	// A trigger condition evaluates to true/false
	public interface GenericTriggerCondition<T>{
		bool Evaluate(T model);
	}

	// An animation event is something we can execute if certain condition is met
	// Implementations can store information to be used during execute
	// Example: playSound stores the name of the sound and call playSound(soundName) on execute
	public abstract class GenericEvent<T>{

		// Condition may be null if executed directly at a key frame
		// It is public only to be edited during character loading..
		public GenericTriggerCondition<T> condition;

		public GenericEvent(){
			// Nothing here
		}

		public GenericEvent(GenericTriggerCondition<T> condition){
			this.condition = condition;
		}

		// To be implemented, execute the event
		public abstract void Execute(T model);

		// Check the condition and execute event if condition passes
		public void Evaluate(T model){
			if (condition == null || condition.Evaluate(model)){
				Execute(model);
			}
		}
	}


	// Parameterless event
	public class SimpleEvent<T>: GenericEvent<T>{

		// Delegate of the event
		public delegate void EventExecutionDelegate(T model);

		private EventExecutionDelegate eventExecutionDelegate;


		// Constructor with delegate and parameters
		public SimpleEvent(GenericTriggerCondition<T> condition, EventExecutionDelegate eventDelegate)
			:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
		}


		// Execute event trough the delegate
		public override void Execute(T model){
			eventExecutionDelegate(model);
		}
	}


	// Event with a single parameter
	public class SingleEvent<T, U>: GenericEvent<T>{

		// Delegate of the event
		public delegate void EventExecutionDelegate(T model, U param);

		private EventExecutionDelegate eventExecutionDelegate;

		// Parameter
		private U param;


		// Constructor with delegate and parameters
		public SingleEvent(GenericTriggerCondition<T> condition, EventExecutionDelegate eventDelegate, U param)
			:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
			this.param = param;
		}


		// Execute event trough the delegate
		public override void Execute(T model){
			eventExecutionDelegate(model, param);
		}
	}


	// Event with two parameters
	public class DoubleEvent<T, U, V>: GenericEvent<T>{

		// Delegate of the event
		public delegate void EventExecutionDelegate(T model, U param1, V param2);

		private EventExecutionDelegate eventExecutionDelegate;

		// Parameter
		private U param1;
		private V param2;


		// Constructor with delegate and parameters
		public DoubleEvent(GenericTriggerCondition<T> condition, EventExecutionDelegate eventDelegate, U param1, V param2)
			:base(condition)
		{
			this.eventExecutionDelegate = eventDelegate;
			this.param1 = param1;
			this.param2 = param2;
		}


		// Execute event trough the delegate
		public override void Execute(T model){
			eventExecutionDelegate(model, param1, param2);
		}
	}



	// An event that can hold a variadic number of parameters to be executed
	public class VariadicEvent<T>: GenericEvent<T>{

		// Delegate of the event
		public delegate void EventExecutionDelegate(T model, params object[] parameters);

		private EventExecutionDelegate eventExecutionDelegate;

		// Parameters
		private object[] parameters;


		// Constructor with delegate and parameters
		public VariadicEvent(
			GenericTriggerCondition<T> condition, 
			EventExecutionDelegate eventDelegate,
			params object[] parameters
		):base(condition){
			this.eventExecutionDelegate = eventDelegate;
			this.parameters = parameters;
		}


		// Execute event trough the delegate
		public override void Execute(T model){
			eventExecutionDelegate(model, parameters);
		}

	}


	public class EventsList<T>: GenericEvent<T>{
		private List<GenericEvent<T>> events;

		public EventsList(GenericTriggerCondition<T> condition, List<GenericEvent<T>> events):base(condition){
			this.events = events;
		}

		public override void Execute(T model){
			foreach (GenericEvent<T> e in events) {
				e.Execute(model);
			}
		}

	}

}

