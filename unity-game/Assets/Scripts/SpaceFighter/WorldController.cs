using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class WorldController:Controller<WorldModel>{

	public const float gravityY = -0.009f;

	public const uint totalGameFrames = 7200; // 2 minutes


	private static bool[,] teamsCollisionMatrix = new bool[,]
		{
			{ true, true  },
			{ true,  true }
		};

	private static bool[,] teamsHitMatrix = new bool[,]
		{
			{ false, true  },
			{ true,  false }
		};

	static WorldController(){
		// Setup character animations
		SetupGameCharacters();

		// Setup teams hit/collision matrixes
		TeamsManagerController.SetupCollisionMatrixes(teamsCollisionMatrix, teamsHitMatrix);
	}


	public WorldController(){
		// Nothing to do
	}


	public FixedVector3 GetRandomSpawnPosition(WorldModel model){
		FixedVector3 res = new FixedVector3((40 + StateManager.state.Random.NextFloat(-4f, 4f)) * (model.lastSpawnWasLeft ? 1 : -1),9,0);
		model.lastSpawnWasLeft = !model.lastSpawnWasLeft;
		return res;
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
			teamsManagerModel = new TeamsManagerModel(2);
			model.teamsModelId = StateManager.state.AddModel(teamsManagerModel);


			// Create some dummy enemy
			FixedVector3 initialPosition = GetRandomSpawnPosition(model);
			playerModel = new ShooterEntityModel(StateManager.state,
				"happy char",
				"Standing",
				CharacterLoader.GetCharacterSkinName("happy char", 0),
				physicsModel,
				null, // no input
				initialPosition,
				new FixedVector3(0, 0.5, 0), // step tolerance
				ShooterEntityController.maxEnergy,
				ShooterEntityController.maxInvincibilityFrames,
				0
			);
			// Model initial state
			ShooterEntityModel playerEntity = (ShooterEntityModel)playerModel;
			playerEntity.isFacingRight = initialPosition.X < 0;
			teamsManagerModel.teams[1].entities.Add(StateManager.state.AddModel(playerModel));

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
		foreach(uint playerId in allPlayers){
			if (!model.players.ContainsKey(playerId)){
				Model inputModel = new PlayerInputModel(playerId);
				FixedVector3 initialPosition = GetRandomSpawnPosition(model);
				playerModel = new ShooterEntityModel(StateManager.state,
					"happy char",		//"soldier", //playerId % 2 == 0 ? "Blaze" : "Rocha",
                	"Standing", 		//"soldierIdleRelaxed",
					CharacterLoader.GetCharacterSkinName("happy char", playerId),
                	physicsModel,
                	inputModel,
                    initialPosition,
                	new FixedVector3(0, 0.5, 0), // step tolerance
                    ShooterEntityController.maxEnergy,
                    ShooterEntityController.maxInvincibilityFrames,
                    0
            	);
				// Model initial state
				ShooterEntityModel playerEntity = (ShooterEntityModel)playerModel;
				playerEntity.isFacingRight = initialPosition.X < 0;
				model.players[playerId] = StateManager.state.AddModel(playerModel);
				teamsManagerModel.teams[0].entities.Add(model.players[playerId]);

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

				Transform t1 = obj.transform.Find("armorBody");
				Transform t2 = obj.transform.Find("armorArms");
				if (t1 != null && t2 != null) {
					SkinnedMeshRenderer[] comps = new SkinnedMeshRenderer[2];
					comps[0] = t1.gameObject.GetComponent<SkinnedMeshRenderer>();
					comps[1] = t2.gameObject.GetComponent<SkinnedMeshRenderer>();

					foreach (SkinnedMeshRenderer c in comps) {
						c.material.color = (isOwnPlayer ? Color.blue : Color.red);
					}
				}
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



	private void PopulatePhysicsWorld(PhysicWorldModel physicsModel, PhysicWorldController physicsController){
		// TODO: read from somewhere.. right now it's ardcoded
		PhysicPlaneModel plane;

		// Static planes

		for (int flip = 1; flip >= -1 ; flip -= 2){

			plane = new PhysicPlaneModel(new FixedVector3(-7.65*flip,3.9,-1*flip),
			                             new FixedVector3(-7.65*flip,3.9,1*flip),
			                             new FixedVector3(-1.5*flip,3.9,1*flip),
			                             new FixedVector3(-1.5*flip,3.9,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
			plane = new PhysicPlaneModel(new FixedVector3(-11.85*flip,1.25,-1*flip),
			                             new FixedVector3(-11.85*flip,1.25,1*flip),
			                             new FixedVector3(-7.65*flip,3.9,1*flip),
			                             new FixedVector3(-7.65*flip,3.9,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
			plane = new PhysicPlaneModel(new FixedVector3(-17.75*flip,1.25,-1*flip),
			                             new FixedVector3(-17.75*flip,1.25,1*flip),
			                             new FixedVector3(-11.85*flip,1.25,1*flip),
			                             new FixedVector3(-11.85*flip,1.25,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);


			plane = new PhysicPlaneModel(new FixedVector3(-17.75*flip,8.95,-1*flip),
			                             new FixedVector3(-17.75*flip,8.95,1*flip),
			                             new FixedVector3(-11.85*flip,8.95,1*flip),
			                             new FixedVector3(-11.85*flip,8.95,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
			plane = new PhysicPlaneModel(new FixedVector3(-11.85*flip,8.95,-1*flip),
			                             new FixedVector3(-11.85*flip,8.95,1*flip),
			                             new FixedVector3(-7.65*flip,11.62,1*flip),
			                             new FixedVector3(-7.65*flip,11.62,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);


			plane = new PhysicPlaneModel(new FixedVector3(-46.05*flip,8.95,-1*flip),
			                             new FixedVector3(-46.05*flip,8.95,1*flip),
			                             new FixedVector3(-23.00*flip,8.95,1*flip),
			                             new FixedVector3(-23.00*flip,8.95,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
			plane = new PhysicPlaneModel(new FixedVector3(-46.0*flip,-11.5,-1*flip),
			                             new FixedVector3(-46.0*flip,-11.5,1*flip),
			                             new FixedVector3(-46.0*flip,8.8,1*flip),
			                             new FixedVector3(-46.0*flip,8.8,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
			plane = new PhysicPlaneModel(new FixedVector3(-23.05*flip,8.8,-1*flip),
			                             new FixedVector3(-23.05*flip,8.8,1*flip),
			                             new FixedVector3(-23.05*flip,-11.5,1*flip),
			                             new FixedVector3(-23.05*flip,-11.5,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
		}



		// Moving planes:
		plane = new MovingPlaneModel(new FixedVector3[]{new FixedVector3(-23.75,1.25,-1), new FixedVector3(-23.75,8.95,-1)},
		                             new FixedVector3(-23.75,1.25,-1),
		                             new FixedVector3(-23.75,1.25,1),
		                             new FixedVector3(-17.75,1.25,1),
		                             new FixedVector3(-17.75,1.25,-1)
		                             );
		physicsController.AddPlane(physicsModel, plane);
		plane = new MovingPlaneModel(new FixedVector3[]{new FixedVector3(17.75,8.95,-1), new FixedVector3(17.75,1.25,-1)},
									 new FixedVector3(17.75,1.25,-1),
									 new FixedVector3(17.75,1.25,1),
									 new FixedVector3(23.75,1.25,1),
									 new FixedVector3(23.75,1.25,-1)
		);
		physicsController.AddPlane(physicsModel, plane);

		plane = new MovingPlaneModel(new FixedVector3[]{new FixedVector3(-7.65,11.62,-1), new FixedVector3(1.65,11.62,-1)},
							 		 new FixedVector3(-7.65,11.62,-1),
									 new FixedVector3(-7.65,11.62,1),
									 new FixedVector3(-1.65,11.62,1),
									 new FixedVector3(-1.65,11.62,-1)
		);
		physicsController.AddPlane(physicsModel, plane);
		
		
	}
	
	
	
	
	private static void SetupGameCharacters(){

		CharacterLoader.LoadCharacter("happy char");

		// hardcoded:
		//SetupCharacter("soldier");
	}



//	private static void SetupCharacter(string charName){
//		
//		AnimationController idle1Ctr = new AnimationController();
//		AnimationController walkCtr = new AnimationController();
//		AnimationController jumpCtr = new AnimationController();
//		AnimationController fallCtr = new AnimationController();
//		AnimationController fireCtr = new AnimationController();
//		AnimationController walkFireCtr = new AnimationController();
//		AnimationController hitCtr = new AnimationController();
//		AnimationController dieCtr = new AnimationController();
//		AnimationView idleView = new AnimationView();
//		AnimationsVCPool.Instance.RegisterController(charName, "soldierIdleRelaxed", idle1Ctr);
//		AnimationsVCPool.Instance.RegisterController(charName, "soldierSprint", walkCtr);
//		AnimationsVCPool.Instance.RegisterController(charName, "soldierJump", jumpCtr);
//		AnimationsVCPool.Instance.RegisterController(charName, "soldierFalling", fallCtr);
//		AnimationsVCPool.Instance.RegisterController(charName, "soldierIdle", fireCtr);
//		AnimationsVCPool.Instance.RegisterController(charName, "soldierRun", walkFireCtr);
//		AnimationsVCPool.Instance.RegisterController(charName, "soldierHitBack", hitCtr);
//		AnimationsVCPool.Instance.RegisterController(charName, "soldierDieBack", dieCtr);
//		AnimationsVCPool.Instance.SetDefaultView(charName, idleView);
//		
//
//		AnimationTriggerCondition condition;
//		List<AnimationTriggerCondition> conditions;
//		AnimationTransitionEvent transition;
//
//		// force walk move character against ground
//		SingleEntityAnimationEvent<FixedVector3> zeroVelEvent = new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityController.SetAnimationVelocity,
//			FixedVector3.Zero
//		);
//		walkCtr.AddKeyframeEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityController.SetAnimationVelocity,
//			new FixedVector3(0,-0.1, 0)
//			));
//		idle1Ctr.AddKeyframeEvent(0, zeroVelEvent);
//		jumpCtr.AddKeyframeEvent(0, zeroVelEvent);
//		fallCtr.AddKeyframeEvent(0, zeroVelEvent);
//		fireCtr.AddKeyframeEvent(0, zeroVelEvent);
//		walkFireCtr.AddKeyframeEvent(0, zeroVelEvent);
//		hitCtr.AddKeyframeEvent(0, zeroVelEvent);
//		dieCtr.AddKeyframeEvent(0, zeroVelEvent);
//
//		// input velocities
//		SingleEntityAnimationEvent<FixedVector3> zeroInputVel;
//		zeroInputVel = new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityController.SetMaxInputVelocity,
//			FixedVector3.Zero
//		);
//		idle1Ctr.AddKeyframeEvent(0, zeroInputVel);
//		dieCtr.AddKeyframeEvent(0, zeroInputVel);
//		fireCtr.AddKeyframeEvent(0, zeroInputVel);
//		hitCtr.AddKeyframeEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityController.SetMaxInputVelocity,
//			new FixedVector3(0.025f, 0, 0.0f)
//		));
//		walkCtr.AddKeyframeEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityController.SetMaxInputVelocity,
//			new FixedVector3(0.1f, 0, 0.0f)
//		));
//		walkFireCtr.AddKeyframeEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityController.SetMaxInputVelocity,
//			new FixedVector3(0.1f, 0, 0.0f)
//			));
//
//
//		
//		// idle to walk
//		condition = new InputAxisMovingCondition();
//		transition = new AnimationTransitionEvent(condition, "soldierSprint", 0.2f);
//		idle1Ctr.AddGeneralEvent(transition);
//		
//		// walk to iddle
//		condition = new NegateCondition(new InputAxisMovingCondition());
//		transition = new AnimationTransitionEvent(condition, "soldierIdleRelaxed", 0.125f);
//		walkCtr.AddGeneralEvent(transition);
//		// Events that allow the character to move
//		walkCtr.AddKeyframeEvent(0, new SingleEntityAnimationEvent<bool>(null, GameEntityController.SetAutomaticFlip, true));
//
//
//		// iddle, walk to jump
//		condition = new InputButtonCondition(InputButtonConditionType.pressed, true, 1);
//		transition = new AnimationTransitionEvent(condition, "soldierJump", 0.3f);
//		idle1Ctr.AddGeneralEvent(transition);
//		walkCtr.AddGeneralEvent(transition);
//		// only jump after shoot bullet leaving
////		conditions = new List<AnimationTriggerCondition>();
////		conditions.Add(new InputButtonCondition(InputButtonConditionType.pressed, true, 0));
////		conditions.Add(new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 3));
////		transition = new AnimationTransition("soldierJump", conditions, 0.3f);
//		fireCtr.AddGeneralEvent(transition);
//		walkFireCtr.AddGeneralEvent(transition);
//		// Events to push the character up
//		jumpCtr.AddKeyframeEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityController.SetMaxInputVelocity,
//			new FixedVector3(0.025f, 0, 0.0f)
//		));
//		jumpCtr.AddKeyframeEvent(2, new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityController.SetMaxInputVelocity,
//			new FixedVector3(0.1f, 0, 0.0f)
//		));
//		jumpCtr.AddKeyframeEvent(2, new SingleEntityAnimationEvent<bool>(null, GameEntityController.SetAutomaticFlip, false));
//		jumpCtr.AddKeyframeEvent(2, new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityController.AddImpulse,
//			new FixedVector3(0.0f, 0.2f, 0.0f)
//		));
//
//		// iddle, walk to fall
//		condition = new NegateCondition(new EntityBoolCondition(GameEntityController.IsGrounded));
//		transition = new AnimationTransitionEvent(condition, "soldierFalling", 0.2f);
//		idle1Ctr.AddGeneralEvent(transition);
//		walkCtr.AddGeneralEvent(transition);
//		// Events that allow the character to move in air
//		fallCtr.AddKeyframeEvent(0, new SingleEntityAnimationEvent<bool>(null, GameEntityController.SetAutomaticFlip, true));
//		fallCtr.AddKeyframeEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
//			null,
//			GameEntityController.SetMaxInputVelocity,
//			new FixedVector3(0.1f, 0, 0.0f)
//		));
//
//		// fall to idle
//		conditions = new List<AnimationTriggerCondition>();
//		conditions.Add(new EntityBoolCondition(GameEntityController.IsGrounded));
//		conditions.Add (new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 4));
//		condition = new ConditionsList(conditions);
//		transition = new AnimationTransitionEvent(condition, "soldierIdleRelaxed", 0.1f);
//		fallCtr.AddGeneralEvent(transition);
//		jumpCtr.AddGeneralEvent(transition);
//
//		// jump to fall (rough version on this demo, cose no really jump anim exists)
//		condition = new NegateCondition(new EntityBoolCondition(GameEntityController.IsGrounded));
//		transition = new AnimationTransitionEvent(condition, "soldierFalling", 0.2f);
//		jumpCtr.AddGeneralEvent(transition); // TODO: err.. supposed to be more complex than this, like moving down
//
//
//		// Shoot!
//		conditions = new List<AnimationTriggerCondition>();
//		conditions.Add(new InputButtonCondition(InputButtonConditionType.pressed, true, 0));
//		conditions.Add(new EntityBoolCondition(ShooterEntityController.HasEnoughPowerToShoot));
//		condition = new ConditionsList(conditions);
//		transition = new AnimationTransitionEvent(condition, "soldierIdle", 0.05f);
//		idle1Ctr.AddGeneralEvent(transition);
//		transition = new AnimationTransitionEvent(condition, "soldierRun", 0.05f);
//		walkCtr.AddGeneralEvent(transition);
//		jumpCtr.AddGeneralEvent(transition);
//		fallCtr.AddGeneralEvent(transition);
//		// keep shooting!
//		conditions = new List<AnimationTriggerCondition>();
//		conditions.Add(new InputButtonCondition(InputButtonConditionType.pressed, true, 0));
//		conditions.Add(new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 12));
//		conditions.Add(new EntityBoolCondition(ShooterEntityController.HasEnoughPowerToShoot));
//		conditions.Add(new NegateCondition(new InputAxisMovingCondition()));
//		conditions.Add(new EntityBoolCondition(GameEntityController.IsGrounded));
//		condition = new ConditionsList(conditions);
//		transition = new AnimationTransitionEvent(condition, "soldierIdle", 0.05f);
//		fireCtr.AddGeneralEvent(transition);
//		walkFireCtr.AddGeneralEvent(transition);
//		conditions = new List<AnimationTriggerCondition>();
//		conditions.Add(new InputButtonCondition(InputButtonConditionType.pressed, true, 0));
//		conditions.Add(new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 12));
//		conditions.Add(new EntityBoolCondition(ShooterEntityController.HasEnoughPowerToShoot));
//		conditions.Add(new InputAxisMovingCondition());
//		condition = new ConditionsList(conditions);
//		transition = new AnimationTransitionEvent(condition, "soldierRun", 0.05f);
//		fireCtr.AddGeneralEvent(transition);
//		walkFireCtr.AddGeneralEvent(transition);
//		conditions = new List<AnimationTriggerCondition>();
//		conditions.Add(new InputButtonCondition(InputButtonConditionType.pressed, true, 0));
//		conditions.Add(new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 12));
//		conditions.Add(new EntityBoolCondition(ShooterEntityController.HasEnoughPowerToShoot));
//		conditions.Add(new NegateCondition(new InputAxisMovingCondition()));
//		conditions.Add(new NegateCondition(new EntityBoolCondition(GameEntityController.IsGrounded)));
//		condition = new ConditionsList(conditions);
//		transition = new AnimationTransitionEvent(condition, "soldierRun", 0.05f);
//		fireCtr.AddGeneralEvent(transition);
//		walkFireCtr.AddGeneralEvent(transition);
//
//		// fire event
//		fireCtr.AddKeyframeEvent(3, new SimpleEntityAnimationEvent(null, ShooterEntityController.Shoot));
//		walkFireCtr.AddKeyframeEvent(3, new SimpleEntityAnimationEvent(null, ShooterEntityController.Shoot));
//
//		// back to idle
//		conditions = new List<AnimationTriggerCondition>();
//		conditions.Add(new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 18));
//		conditions.Add(new InputButtonCondition(InputButtonConditionType.pressed, false, 0));
//		condition = new ConditionsList(conditions);
//		transition = new AnimationTransitionEvent(condition, "soldierIdleRelaxed", 0.15f);
//		fireCtr.AddGeneralEvent(transition);
//		conditions = new List<AnimationTriggerCondition>();
//		conditions.Add(new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 18));
//		conditions.Add(new InputButtonCondition(InputButtonConditionType.pressed, false, 0));
//		conditions.Add(new EntityBoolCondition(GameEntityController.IsGrounded));
//		condition = new ConditionsList(conditions);
//		transition = new AnimationTransitionEvent(condition, "soldierSprint", 0.15f);
//		walkFireCtr.AddGeneralEvent(transition);
//		conditions = new List<AnimationTriggerCondition>();
//		conditions.Add(new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 12));
//		conditions.Add(new InputButtonCondition(InputButtonConditionType.pressed, false, 0));
//		conditions.Add(new NegateCondition(new EntityBoolCondition(GameEntityController.IsGrounded)));
//		condition = new ConditionsList(conditions);
//		transition = new AnimationTransitionEvent(condition, "soldierFalling", 0.15f);
//		walkFireCtr.AddGeneralEvent(transition);
//
//		// hit trigger
//		condition = new EntityBoolCondition(ShooterEntityController.GotHit);
//		transition = new AnimationTransitionEvent(condition, "soldierHitBack", 0.1f);
//		idle1Ctr.AddGeneralEvent(transition);
//		walkCtr.AddGeneralEvent(transition);
//		jumpCtr.AddGeneralEvent(transition);
//		fallCtr.AddGeneralEvent(transition);
//		fireCtr.AddGeneralEvent(transition);
//		hitCtr.AddGeneralEvent(transition);
//		// hit event
//		condition = new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 6);
//		transition = new AnimationTransitionEvent(condition, "soldierIdleRelaxed", 0.15f);
//		hitCtr.AddGeneralEvent(transition);
//
//		// Die!!
//		condition = new EntityBoolCondition(ShooterEntityController.IsDead);
//		transition = new AnimationTransitionEvent(condition, "soldierDieBack", 0.01f);
//		hitCtr.AddGeneralEvent(transition);
//		// Forcing that headless bug to stop!...
//		idle1Ctr.AddGeneralEvent(transition);
//		walkCtr.AddGeneralEvent(transition);
//		jumpCtr.AddGeneralEvent(transition);
//		fireCtr.AddGeneralEvent(transition);
//		walkFireCtr.AddGeneralEvent(transition);
//		// once die animation finishes, respawn
//		dieCtr.AddKeyframeEvent(60, new SimpleEntityAnimationEvent(null, ShooterEntityController.KillAndRespawn));
//		condition = new AnimationFrameCondition(ArithmeticConditionOperatorType.equal, 60);
//		transition = new AnimationTransitionEvent(condition, "soldierIdleRelaxed", 0f);
//		dieCtr.AddGeneralEvent(transition);
//
//
//	}
	

	public bool CheckGameOver(WorldModel worldModel){
		if (StateManager.state == null) return false;
		if (StateManager.state.Keyframe == totalGameFrames){
			GameObject networkObject = GameObject.Find("Network");
			if (networkObject == null) return true;
			NetworkChat networkChat = networkObject.GetComponent<NetworkChat>();
			if (networkChat == null) return true;

			List<Eppy.Tuple<NetworkPlayerData,ShooterEntityModel>> sortedPlayers = new List<Eppy.Tuple<NetworkPlayerData,ShooterEntityModel>>();
			ShooterEntityModel playerModel;
			NetworkPlayerData playerData;
			foreach (KeyValuePair<uint, ModelReference> pair in worldModel.players){
				playerModel = StateManager.state.GetModel(pair.Value) as ShooterEntityModel;
				if (playerModel == null) continue;
				playerData = NetworkCenter.Instance.GetPlayerData(pair.Key);
				if (playerData == null) continue;
				sortedPlayers.Add(new Eppy.Tuple<NetworkPlayerData,ShooterEntityModel>(playerData, playerModel));
			}
			sortedPlayers.Sort(delegate(Eppy.Tuple<NetworkPlayerData,ShooterEntityModel> p1, Eppy.Tuple<NetworkPlayerData,ShooterEntityModel> p2){
					return p2.Item2.GetBalance().CompareTo(p1.Item2.GetBalance());
				}
			);
			if (sortedPlayers.Count == 0) return true;

			networkChat.AddBotTextMessage("###################################");
			networkChat.AddBotTextMessage("########## FINAL SCORE!! ##########");
			int difference;

			for (int i = 0 ; i < sortedPlayers.Count ; ++i){
				playerModel = sortedPlayers[i].Item2;
				difference = playerModel.GetBalance();
				networkChat.AddBotTextMessage("# " + (i+1) + ": " + "(" + (difference > 0 ? "+" : "") + difference + ") "
				                             + sortedPlayers[i].Item1.playerName);
				networkChat.AddBotTextMessage("\tKills: " + playerModel.totalKills + "\tDeaths: " + playerModel.totalDeaths);
			}
			networkChat.AddBotTextMessage("###################################");
			return true;
		}
		return false;
	}
	
}

