using System;
using System.Collections.Generic;

namespace RetroBread{

#region Interface

// Model interface, not generic so we can have polymorphism in StateManager
public interface Model{

	// Unique model index in the StateManager
	ModelReference Index { get; }

	// Models with lower update order updates first
	// Models within the same update order are updated by index order
	int UpdatingOrder { get; set; }
	
	// Set View and Controller from other model, if compatible.
	// If this is a compatible state, we want to reuse our VC
	bool SetVCFromModel(Model other);

	// Ensure the model has a view
	void EnsureView();

	// Ensure the model has a controller
	void EnsureController();

	// View is updated every frame, even if the game is paused
	void UpdateView(float deltaTime);

	// Controller is updated every logics tick
	void UpdateController();
	
	// Controller is post updated after all models have updated
	void PostUpdateController();

	// Unload resources
	void Destroy();

	Controller Controller();

}

#endregion

#region Comparer

// Comparer using updating order, if equal use index
public class ModelComparerByUpdatingOrder:IComparer<Model>{

	int IComparer<Model>.Compare(Model model1, Model model2) {
		if (model1.UpdatingOrder == model2.UpdatingOrder){
			return model1.Index.CompareTo(model2.Index);
		}
		return model1.UpdatingOrder.CompareTo(model2.UpdatingOrder);
	}

}

#endregion

#region Class

// We use the Curiously Recurring Template Pattern here to enforce types
[Serializable]
public abstract class Model<T>:Model where T:Model<T>{

	// Unique model index in the StateManager
	// Note: this class object is the only exception on models serialization policy.
	// The object is only useful when a new model is created and others reference it.
	// From there, the reference never change, maintaining the consistency
	public ModelReference Index { get ; private set; }

	// Models with lower update order updates first
	private int updatingOrder;

	// updating order getter/setter, automatically reorders when order changes
	public int UpdatingOrder {
		get{
			return updatingOrder;
		}
		set{
			// only modify if it's during a state update cycle
			if (StateManager.state.IsUpdating && updatingOrder != value) {
				updatingOrder = value;
				StateManager.state.ReorderModel(this);
			}
		}
	}


	[NonSerialized]
	private View<T> view;

	[NonSerialized]
	private Controller<T> controller;
	

	// A model should be created with a given updatingOrder
	public Model(int updatingOrder = 0){
		Index = new ModelReference();
		this.updatingOrder = updatingOrder;
	}


	// Set View and Controller from other model, if compatible.
	// If this is a compatible state, we want to reuse our VC
	public bool SetVCFromModel(Model other) {

		// Only use same View and Controller if they are of same type
		// otherwise proper VC should be created afterwards
		if (this.GetType() != other.GetType()) {
			return false;
		}
		Model<T> otherModel = other as Model<T>;

		// Even though they are of the same type, only reuse if it's data is compatible with the present VC
		// Compatibility checks may include if the model use the same resources (textures, AI models, etc)
		T thisModel = this as T;
		bool isViewCompatible = otherModel.view != null ? otherModel.view.IsCompatible(otherModel as T, thisModel) : false;
		bool isControllerCompatible = otherModel.controller != null ? otherModel.controller.IsCompatible(otherModel as T, thisModel) : false;
	
		this.view = isViewCompatible ? otherModel.view : null;
		this.controller = isControllerCompatible ? otherModel.controller : null;

		if (isViewCompatible){
			this.view = otherModel.view;
			otherModel.view = null;
		}

		if (isControllerCompatible){
			this.controller = otherModel.controller;
			otherModel.controller = null;
		}

		return isViewCompatible && isControllerCompatible;

	}

	// Make sure view exists
	public void EnsureView() {
		if (view == null) {
			view = CreateView();
		}
	}

	// Make sure controller exists
	public void EnsureController() {
		if (controller == null) {
			controller = CreateController();
		}
	}


	public Controller<T> GetController() {
		EnsureController();
		return controller;
	}

	public View<T> GetView() {
		// no need to ensure view here
		EnsureView();
		return view;
	}

	public Controller Controller(){
		return GetController() as Controller;
	}

	// Create view
	protected virtual View<T> CreateView(){
		// Create an empty view by default
		return new View<T>();
	}

	// Create controller
	protected virtual Controller<T> CreateController(){
		// Create an empty controller by default
		return new Controller<T>();
	}

	// Update View every frame
	public void UpdateView(float deltaTime)
	{
		if (view != null){
			view.Update(this as T, deltaTime);
		}
	}

	// Update Controller every logics tick
	public void UpdateController(){
		if (controller != null){
			controller.Update(this as T);
		}
	}

	// Post update Controller every logics tick after all updates
	public void PostUpdateController(){
		if (controller != null){
			controller.PostUpdate(this as T);
		}
	}


	public void InvalidateController(){
		if (controller != null) controller.OnDestroy(this as T);
		controller = null;
	}

	public void InvalidateView(){
		if (view != null) view.OnDestroy(this as T);
		view = null;
	}

	public void InvalidateVC(){
		InvalidateView();
		InvalidateController();
	}

	public void Destroy(){
		InvalidateVC();
		OnDestroy();
	}

	public virtual void OnDestroy(){
		// Nothing by default
	}
		
}

#endregion

}

