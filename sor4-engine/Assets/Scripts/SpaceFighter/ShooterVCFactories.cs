using System;
using RetroBread;

public static class ShooterVCFactories{

	// Static constructor
	// Register all default View & Controllers for engine models
	public static void RegisterFactories(){
		RegisterMovingPlaneVCFactories();
		RegisterWorldVCFactories();
		RegisterBulletVCFactories();
		RegisterShooterVCFactories();
	}


	// Moving plane
	private static void RegisterMovingPlaneVCFactories(){
		VCFactoriesManager.Instance.RegisterControllerFactory<PhysicPlaneModel>(
			MovingPlaneModel.MovingPlaneControllerFactoryId,
			delegate(PhysicPlaneModel model){
			MovingPlaneModel movingModel = model as MovingPlaneModel;
				if (movingModel == null) return null;
				return new MovingPlaneController(movingModel.path);
			}
		);
		
		VCFactoriesManager.Instance.RegisterViewFactory<PhysicPlaneModel>(
			MovingPlaneModel.MovingPlaneViewFactoryId,
			delegate(PhysicPlaneModel model){
				return new MovingPlaneView();
			}
		);
	}


	// World
	private static void RegisterWorldVCFactories(){
		VCFactoriesManager.Instance.RegisterControllerFactory<WorldModel>(
			WorldModel.WorldControllerFactoryId,
			delegate(WorldModel model){
				return new WorldController();
			}
		);
	}


	// Bullet
	private static void RegisterBulletVCFactories(){
		VCFactoriesManager.Instance.RegisterControllerFactory<PhysicPointModel>(
			BulletPointModel.BulletPointControllerFactoryId,
			delegate(PhysicPointModel model){
				return new BulletPointController();
			}
		);
		
		VCFactoriesManager.Instance.RegisterViewFactory<PhysicPointModel>(
			BulletPointModel.BulletPointViewFactoryId,
			delegate(PhysicPointModel model){
				return new BulletPointView();
			}
		);
	}


	// Shooter
	private static void RegisterShooterVCFactories(){
		VCFactoriesManager.Instance.RegisterControllerFactory<GameEntityModel>(
			ShooterEntityModel.ShooterEntityControllerFactoryId,
			delegate(GameEntityModel model){
				return new ShooterEntityController();
			}
		);
		
		VCFactoriesManager.Instance.RegisterViewFactory<GameEntityModel>(
			ShooterEntityModel.ShooterEntityViewFactoryId,
			delegate(GameEntityModel model){
				return new ShooterEntityView();
			}
		);
	}



}



