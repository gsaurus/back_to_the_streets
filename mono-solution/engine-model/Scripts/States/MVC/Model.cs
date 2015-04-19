using System;
using System.Collections.Generic;

namespace RetroBread{

	#region Interface

	// Model interface, not generic so we can have polymorphism in game state
	public interface Model{

		// Unique model index in the StateManager
		ModelReference Index { get; }

		// Models with lower update order updates first
		// Models within the same update order are updated by index order
		int UpdatingOrder { get; set; }
		
		// Set View and Controller from other model, if compatible.
		// If this is a compatible state, we want to reuse our VC
		bool SetVCFromModel(Model other);

		// Get the view associated to this model
		View View();

		// Get the controller associated to this model
		Controller Controller();

		// Unload resources
		void Destroy();

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
		public int UpdatingOrder { get; set; }
		// used to obtain the VC factories
		private string viewFactoryId;
		private string controllerFactoryId;

		public string ViewFactoryId {
			get{
				return viewFactoryId;
			}
			set{
				viewFactoryId = value;
				InvalidateView();
			}
		}

		public string ControllerFactoryId {
			get{
				return controllerFactoryId;
			}
			set{
				controllerFactoryId = value;
				InvalidateController();
			}
		}


		[NonSerialized]
		private View<T> view;

		[NonSerialized]
		private Controller<T> controller;
		

		#region Constructors

		// Simpler Constructor
		public Model(string controllerFactoryId):this(controllerFactoryId, null, 0){}

		// Constructor given factory ids and updatingOrder
		public Model(string controllerFactoryId, string viewFactoryId, int updatingOrder){
			Index = new ModelReference();
			this.controllerFactoryId = controllerFactoryId;
			this.viewFactoryId = viewFactoryId;
			this.UpdatingOrder = updatingOrder;
		}

		#endregion


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
			bool isViewCompatible = otherModel.viewFactoryId == this.viewFactoryId;
			bool isControllerCompatible = otherModel.controllerFactoryId == this.controllerFactoryId;

			if (isViewCompatible){
				isViewCompatible = otherModel.view != null ? otherModel.view.IsCompatible(otherModel as T, thisModel) : false;
			}
			if (isControllerCompatible){
				isControllerCompatible = otherModel.controller != null ? otherModel.controller.IsCompatible(otherModel as T, thisModel) : false;
			}

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
		public View View() {
			if (view == null && viewFactoryId != null) {
				view = VCFactoriesManager.Instance.CreateView<T>(viewFactoryId, this as T);
			}
			return view;
		}

		// Make sure controller exists
		public Controller Controller() {
			if (controller == null && controllerFactoryId != null) {
				controller = VCFactoriesManager.Instance.CreateController<T>(controllerFactoryId, this as T);
			}
			return controller;
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

