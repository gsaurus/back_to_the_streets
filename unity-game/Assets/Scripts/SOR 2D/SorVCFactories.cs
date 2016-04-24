using System;
using RetroBread;

public static class SorVCFactories{


	public const string Entity2DViewFactoryId = "_rb_gev2d";
	public const string Point2DViewFactoryId = "_rb_ppv2d";

	// Static constructor
	// Register all default View & Controllers for engine models
	public static void RegisterFactories(bool versusMode){
		if (versusMode){
			RegisterWorldVCFactoriesForVersusMode();
		}else{
			Debug.LogError("Only versus-mode is suported atm");
		}
		RegisterEntity2DVCFactories();
		RegisterPoint2DVCFactories();
	}
		
	// World
	private static void RegisterWorldVCFactoriesForVersusMode(){
		VCFactoriesManager.Instance.RegisterControllerFactory<WorldModel>(
			WorldModel.WorldControllerFactoryId,
			delegate(WorldModel model){
				return new VersusWorldController();
			}
		);
		// TODO: register view for quakes, etc?
	}
		


	// Entities in 2D
	private static void RegisterEntity2DVCFactories(){

		VCFactoriesManager.Instance.RegisterViewFactory<GameEntityModel>(
			Entity2DViewFactoryId,
			delegate(GameEntityModel model){
				return new Entity2DView();
			}
		);
	}



	// Physic Point in 2D
	private static void RegisterPoint2DVCFactories(){

		VCFactoriesManager.Instance.RegisterViewFactory<PhysicPointModel>(
			Point2DViewFactoryId,
			delegate(PhysicPointModel model){
				return new PhysicPoint2DView();
			}
		);
	}


}



