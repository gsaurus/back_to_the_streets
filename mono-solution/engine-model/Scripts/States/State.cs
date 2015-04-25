using System;
using System.Collections.Generic;
using ProtoBuf;


namespace RetroBread{


	// A State contains a set of game data models, and has the ability to use
	// the corresponding controllers and views for logic updates and visualisation

	// State interface from the Controllers point of view
	// I.e. what controllers can access while updating
	public interface State{

		// Add a new model to the state (it's added only after the update cycle)
		ModelReference AddModel(Model model);
		ModelReference AddModel(Model model, InternalState.onModelChanged callback, object context);

		// Remove a model from the state (it's removed only after the update cycle)
		void RemoveModel(Model model);
		void RemoveModel(Model model, InternalState.onModelChanged callback, object context);
		void RemoveModel(uint modelId);
		void RemoveModel(uint modelId, InternalState.onModelChanged callback, object context);

		// Reorder model if update ordering changed
		void ReorderModel(Model model, int newUpdatingOrder);
		void ReorderModel(Model model, int newUpdatingOrder, InternalState.onModelChanged callback, object context);

		// Set the main model
		void SetAsMainModel(Model mainModel);
		void SetAsMainModel(Model mainModel, InternalState.onModelChanged callback, object context);

		// Get the model with the given index
		Model GetModel(uint modelIndex);

		// Get the main model (perhaps to gain access to other models)
		Model MainModel { get; }

		// Is it on the middle of an update cycle?
		bool IsUpdating { get; }

		bool IsPostUpdating { get; }

		// The keyframe in which this state happened
		uint Keyframe { get; }

		// Access to the random generator
		RandomGenerator Random { get; }

		State Clone();

	}

	// In a nutshell, State has all the information necessary to roll deterministic gameplay.
	// A state contains a set of models, and updates their views and controllers
	// State can be serialized / deserialized for save/load state, send throgh network, etc
	// It also contains a random number generator, which state is also serialized
	//
	//
	// Conventions to take in account:
	//
	// A state is always initiated with a single (main) model
	// The main model may create other models during the first update cycle
	// Any model can create, remove, or reorder models only during the update cycle
	// Main model can be set only during update cycle
	// All changes to the state (creation, remotion, reordering, main model set) made
	// during the update cycle only take place once it finishes
	[ProtoContract]
	public sealed class InternalState: State{

		public delegate void onModelChanged(Model model, object context);

		// A dictionary holds all models,
		// new models are added with an incremental key
		[ProtoMember(1)]
		public uint nextModelIndex; // private
		[ProtoMember(2)]
		public uint mainModelIndex; // private
		[ProtoMember(3)]
		public Dictionary<uint,Model> models; // private

		// The keyframe in which this state happened
		// This is handled by the StateManager
		[ProtoMember(4)]
		public uint Keyframe {get ; set; }


		// Random generator, it's state is also serialized
		[ProtoMember(5)]
		public RandomGenerator random;

		public RandomGenerator Random {
			get{
				// return null when it's not updating
				return updateChanges.IsUpdating ? random : null;
			}
			private set{
				random = value;
			}
		}

		// Models sorted by updating order, no need to serialize this
		private SortedList<Model, Model> sortedModels; // SortedSet isn't supported by Unity

		// Store update changes that must happen after update cycle
		private TemporaryUpdateChanges updateChanges;

		// This flag is handy for controllers to apply changes directly or postpone them
		public bool IsPostUpdating { get; private set; }


		// Give access to the main model
		public Model MainModel{
			get{
				Model ret;
				models.TryGetValue(mainModelIndex, out ret);
				return ret;
			}
		}

		// Is it doing an update cycle?
		public bool IsUpdating{
			get{
				return updateChanges.IsUpdating;
			}
		}

		// Access a model from it's index
		public Model GetModel(uint modelIndex) {
			Model ret;
			models.TryGetValue(modelIndex, out ret);
			return ret;
		}


		// Default constructor
		public InternalState(){
			// Create necessary structures
			models = new Dictionary<uint, Model>();
			sortedModels = new SortedList<Model, Model>(new ModelComparerByUpdatingOrder());
		}

		// Constructor: create a State with an initial model (set as main model) and random seed
		public InternalState(Model mainModel, long randomSeed){

			// Initialize the random generator
			random = new SimpleRandomGenerator(randomSeed);

			// Create necessary structures
			models = new Dictionary<uint, Model>();
			sortedModels = new SortedList<Model, Model>(new ModelComparerByUpdatingOrder());

			// Setup initial model
			if (nextModelIndex == ModelReference.InvalidModelIndex) ++nextModelIndex;
			mainModelIndex = nextModelIndex;
			mainModel.Index.UpdateIndex(mainModelIndex);
			models.Add(mainModelIndex, mainModel);
			sortedModels.Add(mainModel, null);
			++nextModelIndex;

		}

		// Clone state and it's models
		public State Clone(){
			Debug.StartProfiling("Clone State");
			InternalState clone = new InternalState();
			clone.nextModelIndex = nextModelIndex;
			clone.mainModelIndex = mainModelIndex;
			clone.Keyframe = Keyframe;
			clone.random = random != null ? random.Clone() : null;

			if (models != null){
				Model newModel;
				foreach(KeyValuePair<uint, Model> pair in models){
					newModel = pair.Value.Clone();
					clone.models.Add(pair.Key, newModel);
					clone.sortedModels.Add(newModel, null);
				}
			}
			Debug.StopProfiling();
			return clone;
		}

		// Update all views
		public void UpdateViews(float deltaTime) {

			foreach(Model model in models.Values) {
				View view = model.View();
				if (view != null){

					Debug.StartProfiling("view update " + model.Index + " " + model.GetType().ToString());

					view.Update(model, deltaTime);

					Debug.StopProfiling();
				}
			}

		}

		// Update cycle (tick)
		public void UpdateControllers(){

			// Enable update changes
			updateChanges.IsUpdating = true;
			// ensure sorted models are up to date
			EnsureSortedModelsList();

			// First update all
			foreach(Model model in sortedModels.Keys) {
				Controller controller = model.Controller();
				if (controller != null){

					Debug.StartProfiling("update " + model.Index + " " + model.GetType().ToString());

					controller.Update(model);

					Debug.StopProfiling();
				}
			}
			// Then post update all
			IsPostUpdating = true;
			foreach(Model model in sortedModels.Keys) {
				Controller controller = model.Controller();
				if (controller != null){

					Debug.StartProfiling("post update " + model.Index + " " + model.GetType().ToString());

					controller.PostUpdate(model);

					Debug.StopProfiling();
				}
			}

			// All temporary changes apply now
			UpdateConsolidation();
			// Disable update changes
			updateChanges.IsUpdating = false;
			IsPostUpdating = false;

			// In the end increment keyframe
			++Keyframe;

		}


		// (roughly) check if sorted models are up to date and rebuild it if necessary
		private void EnsureSortedModelsList(){
			if (sortedModels == null || sortedModels.Count != models.Count){
				sortedModels = new SortedList<Model, Model>(models.Count, new ModelComparerByUpdatingOrder());
				foreach (Model model in models.Values) {
					sortedModels.Add(model, null);
				}
				//sortedModels = new SortedList<Model, Model>(models.Values);
			}
		}


		private void ReorderModelInternal(ModelChangeInfo sortedModel){
			sortedModels.Remove(sortedModel.model);
			sortedModels.Add(sortedModel.model, null);
			sortedModel.PerformCallback();
		}

		private void RemoveModelInternal(ModelChangeInfo deletedModel){
			sortedModels.Remove(deletedModel.model);
			models.Remove(deletedModel.model.Index);
			deletedModel.PerformCallback();
			deletedModel.model.Destroy();
		}

		private void AddModelInternal(ModelChangeInfo createdModel){
			if (nextModelIndex == ModelReference.InvalidModelIndex) ++nextModelIndex;
			createdModel.model.Index.UpdateIndex(nextModelIndex);
			models.Add(nextModelIndex, createdModel.model);
			sortedModels.Add(createdModel.model, null);
			++nextModelIndex;
			createdModel.PerformCallback();
		}



		// Once update cycle ends we apply pending changes (creations, removals, ordering changes)
		private void UpdateConsolidation(){

			// Main model
			if (updateChanges.newMainModel != null && updateChanges.newMainModel.model != null) {
				mainModelIndex = updateChanges.newMainModel.model.Index;
				updateChanges.newMainModel.PerformCallback();
			}

			// Ordering
			if (updateChanges.reorderedModelsFromUpdate != null) {
				foreach(ModelChangeInfo sortedModel in updateChanges.reorderedModelsFromUpdate) {
					ReorderModelInternal(sortedModel);
				}
			}
			// Removals
			if (updateChanges.deletedModelsFromUpdate != null) {
				foreach(ModelChangeInfo deletedModel in updateChanges.deletedModelsFromUpdate) {
					RemoveModelInternal(deletedModel);
				}
			}
			// Additions
			if (updateChanges.createdModelsFromUpdate != null) {
				foreach(ModelChangeInfo createdModel in updateChanges.createdModelsFromUpdate) {
					AddModelInternal(createdModel);
				}
			}

		}


		// Internal update change operation
		private void UpdateOperationChange(ModelChangeInfo modelInfo, ref List<ModelChangeInfo> list){
			// Do not do updating operations outside of update cycles
			if (!updateChanges.IsUpdating) return;
			
			if (list == null) {
				list = new List<ModelChangeInfo>();
			}
			list.Add(modelInfo);
		}


		// Reorder model when the updating order changes
		public void ReorderModel(Model model, int newUpdatingOrder){
			ReorderModel(model, newUpdatingOrder, null, null);
		}

		public void ReorderModel(Model model, int newUpdatingOrder, onModelChanged callback, object context){
			model.UpdatingOrder = newUpdatingOrder;
			ModelChangeInfo changeInfo = new ModelChangeInfo(model, callback, context);
			if (IsUpdating){
				UpdateOperationChange(changeInfo, ref updateChanges.reorderedModelsFromUpdate);
			}else {
				ReorderModelInternal(changeInfo);
			}
		}
		
		// Add a new model to the state (it's added only after the update cycle)
		public ModelReference AddModel(Model model){
			return AddModel(model, null, null);
		}

		public ModelReference AddModel(Model model, onModelChanged callback, object context){
			ModelChangeInfo changeInfo = new ModelChangeInfo(model, callback, context);
			if (IsUpdating){
				UpdateOperationChange(changeInfo, ref updateChanges.createdModelsFromUpdate);
			}else {
				AddModelInternal(changeInfo);
			}
			return model.Index;
		}


		// Remove a model from the state (it's removed only after the update cycle)
		public void RemoveModel(Model model){
			RemoveModel(model, null, null);
		}

		public void RemoveModel(Model model, onModelChanged callback, object context){
			ModelChangeInfo changeInfo = new ModelChangeInfo(model, callback, context);
			if (IsUpdating){
				UpdateOperationChange(changeInfo, ref updateChanges.deletedModelsFromUpdate);
			}else {
				RemoveModelInternal(changeInfo);
			}
		}


		public void RemoveModel(uint modelId){
			RemoveModel(modelId, null, null);
		}

		public void RemoveModel(uint modelId, onModelChanged callback, object context){
			Model model = GetModel(modelId);
			if (model != null){
				RemoveModel(model, callback, context);
			}
		}



		public void SetAsMainModel(Model mainModel){
			SetAsMainModel(mainModel, null, null);
		}

		public void SetAsMainModel(Model mainModel, onModelChanged callback, object context){
			updateChanges.newMainModel = new ModelChangeInfo(mainModel, callback, context);
		}


		// This will try to match models and reuse views and controllers as much as possible
		// All unmatching views and controllers are destroyed from the other state
		public void ReuseVCFromOtherState(InternalState otherState){
			Model myModel;
			foreach (KeyValuePair<uint, Model> pair in otherState.models) {
				if (models.TryGetValue(pair.Key, out myModel)) {
					myModel.SetVCFromModel(pair.Value);
				}
				pair.Value.Destroy();
			}
		}

		public void Destroy(){
			foreach (Model model in models.Values){
				model.Destroy();
			}
		}
		
	}


	// keep callback information for when the model change takes place
	// aditionaly it can store a reference that is updated once the model change takes place
	// (reference currently only used for model additions)
	internal class ModelChangeInfo{
		public Model model;
		public object context;
		public InternalState.onModelChanged callback;

		public ModelChangeInfo(Model model, InternalState.onModelChanged callback, object context){
			this.model = model;
			this.callback = callback;
			this.context = context;
		}

		public void PerformCallback(){
			if (model != null && callback != null) {
				callback(model, context);
			}
		}
	}


	// Struct used during an update cycle to store changes that must happen in the end of the cycle
	internal struct TemporaryUpdateChanges{

		private bool isUpdating;
		public bool IsUpdating {
			get{
				return isUpdating;
			}
			set{
				// reset all information
				if (isUpdating) {
					createdModelsFromUpdate = null;
					deletedModelsFromUpdate = null;
					reorderedModelsFromUpdate = null;
					newMainModel = null;
				}
				isUpdating = value;
			}
		}
		
		// The next lists are temporarily created during update cycle and consolidated after
		// New models can be created
		public List<ModelChangeInfo> createdModelsFromUpdate;
		//Some models can be removed
		public List<ModelChangeInfo> deletedModelsFromUpdate;
		// Some models may change their updaing order
		public List<ModelChangeInfo> reorderedModelsFromUpdate;
		// Main model may change
		public ModelChangeInfo newMainModel;
		
	}


}