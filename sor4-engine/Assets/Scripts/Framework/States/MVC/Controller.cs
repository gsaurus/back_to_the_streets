using System;


namespace RetroBread{

// Handy interface to encapsulate a controller when it's type is unknown
public interface Controller{
	// Nothing to declare
}


// Do not keep any state in controllers, unless between Update and PostUpdate calls
public class Controller<T>:Controller where T:Model{

	// Called once per logical tick
	public virtual void Update(T model){
		// Default controller does nothing
	}
	
	// Called once per logical tick after all updates
	// Can be used to apply update effects without interfering with the updates cycle
	// Update effects may include physics, hits, etc
	public virtual void PostUpdate(T model){
		// Default controller does nothing
	}


	// Helps identify if this controller can be reused for other model
	// Incompatible if there are heavy resources that must be (re)load
	public virtual bool IsCompatible(T originalModel, T newModel){
		return true; // Default 
	}


	public virtual void OnDestroy(T model){
		// Nothing by default
	}

}

}

