using System;


namespace RetroBread{

	// Handy interface to encapsulate a controller when it's type is unknown
	public interface Controller{

		void Update(State state, Model model);

		void PostUpdate(State state, Model model);

	}


	// Do not keep any state in controllers, unless between Update and PostUpdate calls
	public class Controller<T>:Controller where T:Model<T>, new(){

		// Translate to inner method
		public void Update(State state, Model model){
			T typedModel = model as T;
			Update(state, typedModel);
		}
		// Translate to inner method
		public void PostUpdate(State state, Model model){
			T typedModel = model as T;
			PostUpdate(state, typedModel);
		}

		// Called once per logical tick
		protected virtual void Update(State state, T model){
			// Default controller does nothing
		}

		// Called once per logical tick after all updates
		// Can be used to apply update effects without interfering with the updates cycle
		// Update effects may include physics, hits, etc
		protected virtual void PostUpdate(State state, T model){
			// Default controller does nothing
		}


		// Helps identify if this controller can be reused for other model
		// Incompatible if there are heavy resources that must be (re)load
		public virtual bool IsCompatible(State state, T originalModel, T newModel){
			return true; // Default
		}


		public virtual void OnDestroy(State state, T model){
			// Nothing by default
		}

	}

}
