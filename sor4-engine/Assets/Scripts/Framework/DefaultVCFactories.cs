using System;


namespace RetroBread{
	
	public static class DefaultVCFactories{

		// Static constructor
		// Register all default View & Controllers for engine models
		public static void RegisterFactories(){
			RegisterPhysicPointVCFactories();
			RegisterPhysicPlaneVCFactories();
			RegisterPhysicWorldVCFactories();
			RegisterPlayerInputVCFactories();
			RegisterGameEntityVCFactories();
			RegisterAnimationVCFactories();
		}


		// Point
		private static void RegisterPhysicPointVCFactories(){
			VCFactoriesManager.Instance.RegisterControllerFactory<PhysicPointModel>(
				DefaultVCFactoryIds.PhysicPointControllerFactoryId,
				delegate(PhysicPointModel model){
					return new PhysicPointController();
				}
			);
			
			VCFactoriesManager.Instance.RegisterViewFactory<PhysicPointModel>(
				DefaultVCFactoryIds.PhysicPointViewFactoryId,
				delegate(PhysicPointModel model){
					return new PhysicPointView();
				}
			);
		}


		// Plane
		private static void RegisterPhysicPlaneVCFactories(){
			VCFactoriesManager.Instance.RegisterControllerFactory<PhysicPlaneModel>(
				DefaultVCFactoryIds.PhysicPlaneControllerFactoryId,
				delegate(PhysicPlaneModel model){
					return new PhysicPlaneController();
				}
			);
		}

		// World
		private static void RegisterPhysicWorldVCFactories(){
			VCFactoriesManager.Instance.RegisterControllerFactory<PhysicWorldModel>(
				DefaultVCFactoryIds.PhysicWorldControllerFactoryId,
				delegate(PhysicWorldModel model){
					return new PhysicWorldController(model);
				}
			);
		}

		// Player Input
		private static void RegisterPlayerInputVCFactories(){
			VCFactoriesManager.Instance.RegisterControllerFactory<PlayerInputModel>(
				DefaultVCFactoryIds.PlayerInputControllerFactoryId,
				delegate(PlayerInputModel model){
					return new PlayerInputController();
				}
			);
		}


		// Game Entity
		private static void RegisterGameEntityVCFactories(){
			VCFactoriesManager.Instance.RegisterControllerFactory<GameEntityModel>(
				DefaultVCFactoryIds.GameEntityControllerFactoryId,
				delegate(GameEntityModel model){
					return new GameEntityController();
				}
			);
			
			VCFactoriesManager.Instance.RegisterViewFactory<GameEntityModel>(
				DefaultVCFactoryIds.GameEntityViewFactoryId,
				delegate(GameEntityModel model){
					return new GameEntityView();
				}
			);
		}


		// Animation
		private static void RegisterAnimationVCFactories(){
			VCFactoriesManager.Instance.RegisterControllerFactory<AnimationModel>(
				DefaultVCFactoryIds.AnimationControllerFactoryId,
				delegate(AnimationModel model){
					return AnimationsVCPool.Instance.GetController(model.characterName, model.animationName);
				}
			);
			
			VCFactoriesManager.Instance.RegisterViewFactory<AnimationModel>(
				DefaultVCFactoryIds.AnimationViewFactoryId,
				delegate(AnimationModel model){
					return AnimationsVCPool.Instance.GetView(model.characterName, model.animationName);
				}
			);
		}

	}



	
}
