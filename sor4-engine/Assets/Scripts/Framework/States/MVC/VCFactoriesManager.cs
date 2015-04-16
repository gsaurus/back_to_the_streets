using System;
using System.Collections.Generic;

namespace RetroBread{

	public class VCFactoriesManager:Singleton<VCFactoriesManager>{
		
		// Factory delegates
		public delegate Controller<T> CreateControllerDelegate<T>(T model);
		public delegate View<T> CreateViewDelegate<T>(T model);

		private Dictionary<string, object> controllerFactories;
		private Dictionary<string, object> viewFactories;


		public VCFactoriesManager(){
			controllerFactories = new Dictionary<string, object>();
			viewFactories = new Dictionary<string, object>();
		}


		public void RegisterControllerFactory<T>(string identifier, CreateControllerDelegate<T> factory){
			controllerFactories[identifier] = factory;
		}

		public void RegisterViewFactory<T>(string identifier, CreateViewDelegate<T> factory){
			viewFactories[identifier] = factory;
		}


		public Controller<T> CreateController<T>(string identifier, T model){
			object obj;
			if (controllerFactories.TryGetValue(identifier, out obj)){
				CreateControllerDelegate<T> createController = obj as CreateControllerDelegate<T>;
				if (createController == null) return null;
				return createController(model);
			}
			return null;
		}


		public View<T> CreateView<T>(string identifier, T model){
			object obj;
			if (controllerFactories.TryGetValue(identifier, out obj)){
				CreateViewDelegate<T> createView = obj as CreateViewDelegate<T>;
				if (createView == null) return null;
				return createView(model);
			}
			return null;
		}

	}


}

