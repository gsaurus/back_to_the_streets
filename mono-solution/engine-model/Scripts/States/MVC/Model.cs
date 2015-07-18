using System;
using System.Collections.Generic;
using ProtoBuf;

namespace RetroBread{

	#region Interface

	// Model interface, not generic so we can have polymorphism in game state
	// This was turned into an abstract class to avoid issues with protobuf-net
	[ProtoContract]
	[ProtoInclude(10, typeof(PlayerInputModel))]
	public abstract class Model{

		// Unique model index in the StateManager
		// Note: this class object is the only exception on models serialization policy.
		// The object is only useful when a new model is created and others reference it.
		// From there, the reference never change, maintaining the consistency
		[ProtoMember(1)]
		public ModelReference Index { get ; set; }
		
		// Models with lower update order updates first
		[ProtoMember(2)]
		public int UpdatingOrder { get; set; }
		
		// used to obtain the VC factories
		protected string viewFactoryId;
		protected string controllerFactoryId;

		[ProtoMember(3)]
		public string ViewFactoryId {
			get{
				return viewFactoryId;
			}
			set{
				viewFactoryId = value;
				InvalidateView();
			}
		}

		[ProtoMember(4)]
		public string ControllerFactoryId {
			get{
				return controllerFactoryId;
			}
			set{
				controllerFactoryId = value;
				InvalidateController();
			}
		}
		
		// Set View and Controller from other model, if compatible.
		// If this is a compatible state, we want to reuse our VC
		public abstract bool SetVCFromModel(Model other);

		// Get the view associated to this model
		public abstract View View();

		// Get the controller associated to this model
		public abstract Controller Controller();

		// Unload resources
		public abstract void Destroy();

		public abstract void InvalidateView();
		public abstract void InvalidateController();

		// Default constructor
		public Model(){
			Index = new ModelReference();
		}

		[Obsolete("SHouldn't use clone methods, too messy")]
		public abstract Model Clone();

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

	#region Generic class

	// We use the Curiously Recurring Template Pattern here to enforce types
	public abstract class Model<T>:Model where T:Model<T>, new(){
	
		private View<T> view;

		private Controller<T> controller;
		

		#region Constructors

		// Default Constructor
		public Model(){
			// Nothing to do
		}

		// Simpler Constructor
		public Model(string controllerFactoryId):this(controllerFactoryId, null, 0){}

		// Constructor given factory ids and updatingOrder
		public Model(string controllerFactoryId, string viewFactoryId, int updatingOrder){
			this.controllerFactoryId = controllerFactoryId;
			this.viewFactoryId = viewFactoryId;
			this.UpdatingOrder = updatingOrder;
		}

		// Copy fields from other model
		protected virtual void AssignCopy(T other){
			Index = new ModelReference(other.Index);
			UpdatingOrder = other.UpdatingOrder;
			controllerFactoryId = other.controllerFactoryId != null ? string.Copy(other.controllerFactoryId) : null;
			viewFactoryId = other.viewFactoryId != null ? string.Copy(other.viewFactoryId) : null;
		}

		public override Model Clone(){
			T newModel = new T();
			newModel.AssignCopy(this as T);
			return newModel;
		}

		#endregion


		// Set View and Controller from other model, if compatible.
		// If this is a compatible state, we want to reuse our VC
		public override bool SetVCFromModel(Model other) {

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
		public override View View() {
			if (view == null && viewFactoryId != null) {
				view = VCFactoriesManager.Instance.CreateView<T>(viewFactoryId, this as T);
			}
			return view;
		}

		// Make sure controller exists
		public override Controller Controller() {
			if (controller == null && controllerFactoryId != null) {
				controller = VCFactoriesManager.Instance.CreateController<T>(controllerFactoryId, this as T);
			}
			return controller;
		}


		public override void InvalidateController(){
			if (controller != null) controller.OnDestroy(this as T);
			controller = null;
		}

		public override void InvalidateView(){
			if (view != null) view.OnDestroy(this as T);
			view = null;
		}

		public void InvalidateVC(){
			InvalidateView();
			InvalidateController();
		}

		public sealed override void Destroy(){
			InvalidateVC();
			OnDestroy();
		}

		protected virtual void OnDestroy(){
			// Nothing by default
		}
			
	}

	#endregion

}

