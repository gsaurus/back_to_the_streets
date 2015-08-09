using System;


namespace RetroBread{
	
	public static class DefaultVCFactories{

		// Static constructor
		// Register all default View & Controllers for engine models
		public static void RegisterFactories(){
			RegisterPlayerInputVCFactories();
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

	}



	
}
