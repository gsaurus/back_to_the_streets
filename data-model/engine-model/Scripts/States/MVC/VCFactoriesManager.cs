using System;
using System.Collections.Generic;

namespace RetroBread{

	public class VCFactoriesManager:Singleton<VCFactoriesManager>{
		
		// Factory delegates
		public delegate Controller<T> CreateControllerDelegate<T>(T model) where T:Model<T>, new();
		public delegate View<T> CreateViewDelegate<T>(T model) where T:Model<T>, new();

		private Dictionary<string, object> controllerFactories = new Dictionary<string, object>();
		private Dictionary<string, object> viewFactories = new Dictionary<string, object>();
	

		// Register Controller factory
		public void RegisterControllerFactory<T>(string identifier, CreateControllerDelegate<T> factory) where T:Model<T>, new(){
			controllerFactories[identifier] = factory;
		}

		// Register View factory
		public void RegisterViewFactory<T>(string identifier, CreateViewDelegate<T> factory) where T:Model<T>, new(){
			viewFactories[identifier] = factory;
		}


		// Create a controller with the requested factory
		public Controller<T> CreateController<T>(string identifier, T model) where T:Model<T>, new(){
			object obj;
			if (controllerFactories.TryGetValue(identifier, out obj)){
				CreateControllerDelegate<T> createController = obj as CreateControllerDelegate<T>;
				if (createController == null) return null;
				return createController(model);
			}
			return null;
		}


		// Create a view with the requested factory
		public View<T> CreateView<T>(string identifier, T model) where T:Model<T>, new(){
			object obj;
			if (viewFactories.TryGetValue(identifier, out obj)){
				CreateViewDelegate<T> createView = obj as CreateViewDelegate<T>;
				if (createView == null) return null;
				return createView(model);
			}
			return null;
		}

	}


}

