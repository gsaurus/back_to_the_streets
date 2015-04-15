using System;


namespace RetroBread{


// Handy interface to encapsulate a view when it's type is unknown
public interface View{
	// Nothing to declare
}


// It can have non logical state, but the view must always reflect the current state of the model
public class View<T>:View where T:Model{
	
	// Visual update - frame rate dependant
	public virtual void Update(T model, float deltaTime){
		// Default view does nothing
	}

	// Helps identify if this view can be reused for other model
	// Incompatible if there are heavy resources that must be (re)load
	public virtual bool IsCompatible(T originalModel, T newModel){
		return true; // Default 
	}


	public virtual void OnDestroy(T model){
		// Nothing by default
	}
	
}

}