using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class VersusWorldController:Controller<WorldModel>{

	public const float gravityY = -0.020f;

	public const uint totalGameFrames = 7200; // 2 minutes

	// Team 0: weapons, world objects, etc
	// Team 1: gang 1
	// Team 2: gamg 2
	private static bool[,] teamsCollisionMatrix = new bool[,]
		{
			{ false, false, false },
			{ false, false, false },
			{ false, false, false }
		};

	private static bool[,] teamsHitMatrix = new bool[,]
		{
			{ false, false, false },
			{ false, false, true  },
			{ false, true,  false }
		};

	static VersusWorldController(){
		// Setup character animations
		SetupGameData();

		// Setup teams hit/collision matrixes
		TeamsManagerController.SetupCollisionMatrixes(teamsCollisionMatrix, teamsHitMatrix);
	}


	public VersusWorldController(){
		// Nothing to do
	}


	public FixedVector3 GetRandomSpawnPosition(WorldModel model){
		return new FixedVector3(StateManager.state.Random.NextFloat(-4f, 4f), 0.0001, StateManager.state.Random.NextFloat(3f, 4.5f));
//		FixedVector3 res = new FixedVector3((40 + StateManager.state.Random.NextFloat(-4f, 4f)) * (model.lastSpawnWasLeft ? 1 : -1),9,0);
//		model.lastSpawnWasLeft = !model.lastSpawnWasLeft;
//		return res;
	}
	


	protected override void Update(WorldModel model){

		// Get physics model. If doesn't exist, create it
		PhysicWorldModel physicsModel;
		PhysicWorldController physicsController;
		TeamsManagerModel teamsManagerModel;
		Model playerModel;

		if (model.physicsModelId == ModelReference.InvalidModelIndex){
			// create world with map name and gravity
			physicsModel = new PhysicWorldModel("map 1", new FixedVector3(0,gravityY,0));
			model.physicsModelId = StateManager.state.AddModel(physicsModel);
			physicsController = physicsModel.Controller() as PhysicWorldController;
			// populate world
			PopulatePhysicsWorld(physicsModel, physicsController);

		}else {
			physicsModel = StateManager.state.GetModel(model.physicsModelId) as PhysicWorldModel;
			physicsController = physicsModel.Controller() as PhysicWorldController;
		}



		// Teams manager
		if (model.teamsModelId == ModelReference.InvalidModelIndex) {
			teamsManagerModel = new TeamsManagerModel(3);
			model.teamsModelId = StateManager.state.AddModel(teamsManagerModel);


			// Create some dummy enemy
			FixedVector3 initialPosition = GetRandomSpawnPosition(model);
			playerModel = new GameEntityModel(
				StateManager.state,
				physicsModel,
				new PhysicPointModel(
					null,
					initialPosition,
					new FixedVector3(0, 0.5, 0),
					DefaultVCFactoryIds.PhysicPointControllerFactoryId,
					SorVCFactories.Point2DViewFactoryId,
					DefaultUpdateOrder.PhysicsUpdateOrder
				),
				new AnimationModel(
					null,
					"Axel_HD",
					"idle",
					CharacterLoader.GetCharacterSkinName("Axel_HD", 0)
				),
				null, // no input
				DefaultVCFactoryIds.GameEntityControllerFactoryId,
				SorVCFactories.Entity2DViewFactoryId,
				DefaultUpdateOrder.EntitiesUpdateOrder
			);
			// Model initial state
			GameEntityModel playerEntity = (GameEntityModel)playerModel;
			playerEntity.isFacingRight = initialPosition.X < 0;
			teamsManagerModel.teams[2].entities.Add(StateManager.state.AddModel(playerModel));

		} else {
			teamsManagerModel = StateManager.state.GetModel(model.teamsModelId) as TeamsManagerModel;
		}


		List<uint> allPlayers;
		if (StateManager.Instance.IsNetworked) {
			allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers();
		} else {
			allPlayers = new List<uint>();
			allPlayers.Add(0);
		}

		// Remove characters for inactive players
		foreach (KeyValuePair<uint, ModelReference> pair in model.players){
			if (!allPlayers.Exists(x => x == pair.Key)){
				// Doesn't exist anymore, remove ship
				playerModel = StateManager.state.GetModel(pair.Value);
				//worldController.RemovePoint(shipModel, OnShipDestroyed, model);
				StateManager.state.RemoveModel(playerModel, OnPlayerRemoved, model);
			}
		}

		// Create characters for new players
		NetworkSorPlayerData playerData;
		foreach(uint playerId in allPlayers){
			if (!model.players.ContainsKey(playerId)){

				if (StateManager.Instance.IsNetworked){
					playerData = NetworkCenter.Instance.GetPlayerData<NetworkSorPlayerData>(playerId);
				}else{
					playerData = NetworkCenter.Instance.GetPlayerData<NetworkSorPlayerData>();
				}
				if (playerData == null){
					RetroBread.Debug.LogWarning("Player " + playerId + " have invalid player data!");
					continue;
				}
				RetroBread.Debug.Log("Player " + playerId + " have character " + playerData.selectedCharacter);
				Model inputModel = new PlayerInputModel(playerId);
				FixedVector3 initialPosition = GetRandomSpawnPosition(model);
				playerModel = new GameEntityModel(
					StateManager.state,
					physicsModel,
					new PhysicPointModel(
						null,
						initialPosition,
						new FixedVector3(0, 0.5, 0),
						DefaultVCFactoryIds.PhysicPointControllerFactoryId,
						SorVCFactories.Point2DViewFactoryId, // DefaultVCFactoryIds.PhysicPointViewFactoryId,
						DefaultUpdateOrder.PhysicsUpdateOrder
					),
					new AnimationModel(
						null,
						"Axel_HD",
						"idle",
						CharacterLoader.GetCharacterSkinName("Axel_HD", 0)
					),
					inputModel,
					DefaultVCFactoryIds.GameEntityControllerFactoryId,
					SorVCFactories.Entity2DViewFactoryId, //DefaultVCFactoryIds.GameEntityViewFactoryId,
					DefaultUpdateOrder.EntitiesUpdateOrder
            	);
				// Model initial state
				GameEntityModel playerEntity = (GameEntityModel)playerModel;
				playerEntity.isFacingRight = initialPosition.X < 0;
				model.players[playerId] = StateManager.state.AddModel(playerModel);
				// hardcoded energy
				playerEntity.customVariables["energy"] = 100;
				teamsManagerModel.teams[1 + playerId % 2].entities.Add(model.players[playerId]);

			}

			GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.players[playerId]);
			// using the name as a hack around having a variable in a script to tell the object is "initialized"
			// should be done in a better way.. but whatever, will do for the demo
			if (obj != null && !obj.name.EndsWith("[initiated]")){

				bool isOwnPlayer;
				if (StateManager.Instance.IsNetworked) {
					isOwnPlayer = playerId == NetworkCenter.Instance.GetPlayerNumber();
				} else {
					isOwnPlayer = true; // warning: local multiplayer would be different, playerId == 0?, actually different camera
				}

				if (isOwnPlayer){
					// Add camera tracking to own player :)
					obj.AddComponent<CameraTracker>();
					
					// TODO: add HUD tracking to this player
					// Note: I'm doing this here at the moment, but if I need to derive GameEntity
					// this can go inside it's view.
					// However I can create a new model just to keep player stats (kills, energy, etc)
					// and use the respective view to display the stats in the HUD
				}
				obj.name += "[initiated]";
			}

		}

		// check end of the game
		if (CheckGameOver(model)){
			// game over
			return;
		}

	}
	

	private void OnPlayerRemoved(Model model, object mainModelObj){
		WorldModel mainModel = mainModelObj as WorldModel;
		if (model == null || mainModel == null) return;
		uint key = 0;
		foreach (KeyValuePair<uint,ModelReference> pair in mainModel.players){
			if (pair.Value == model.Index){
				key = pair.Key;
				break;
			}
		}
		mainModel.players.Remove(key);

		// Remove from the team
		TeamsManagerModel teamsManagerModel = StateManager.state.GetModel(mainModel.teamsModelId) as TeamsManagerModel;
		teamsManagerModel.teams[0].entities.Remove(model.Index);
	}





	private void ProducePlaneFromQuad(PhysicWorldModel physicsModel, PhysicWorldController physicsController, GameObject quadObj){
		MeshFilter meshFilter = quadObj.GetComponent<MeshFilter>();
		if (meshFilter == null) return;
		Vector3[] vertices = meshFilter.mesh.vertices;
		if (vertices.Length < 4) return;

		PhysicPlaneModel plane = new PhysicPlaneModel(
//			null, "debug_planes",
			quadObj.transform.TransformPoint(vertices[0]).AsFixedVetor3(),
			quadObj.transform.TransformPoint(vertices[3]).AsFixedVetor3(),
			quadObj.transform.TransformPoint(vertices[1]).AsFixedVetor3(),
			quadObj.transform.TransformPoint(vertices[2]).AsFixedVetor3()
		);
		physicsController.AddPlane(physicsModel, plane);
	}


	private void PopulatePhysicsWorld(PhysicWorldModel physicsModel, PhysicWorldController physicsController){
		// TODO: read from somewhere.. right now it's ardcoded
		// TODO: do it in assets packer, then load from file
		GameObject[] quadObjects = GameObject.FindGameObjectsWithTag("quad");
		foreach (GameObject quadObj in quadObjects){
			ProducePlaneFromQuad(physicsModel, physicsController, quadObj);
		}

	}
	
	
	
	
	private static void SetupGameData(){

		// TODO: setup based on level info
		HUDLoader.LoadHud("hud_bundle");

		// TODO: setup based on players added
		CharacterLoader.LoadCharacter("Axel_HD");
		CharacterLoader.LoadCharacter("happy char");
		CharacterLoader.LoadCharacter("Axel_px");

	}



	public bool CheckGameOver(WorldModel worldModel){
		if (StateManager.state == null) return false;

		return false;
	}
	
}

