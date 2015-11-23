using System;
using RetroBread;

public static class TanksVCFactories{

	// Static constructor
	// Register all default View & Controllers for engine models
	public static void RegisterFactories(){
		RegisterWorldVCFactories();
	}

	// World
	private static void RegisterWorldVCFactories(){
		VCFactoriesManager.Instance.RegisterControllerFactory<WorldModel>(
			WorldModel.WorldControllerFactoryId,
			delegate(WorldModel model){
				return new WorldController();
			}
		);

		VCFactoriesManager.Instance.RegisterViewFactory<WorldModel>(
			WorldModel.WorldViewFactoryId,
			delegate(WorldModel model){
				// For now use debug view
				return new SpriteWorldView(model);
			}
		);
	}


}



