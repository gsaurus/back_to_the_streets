
using System;
using UnityEngine;
using System.Collections.Generic;


public class WorldController:Controller<WorldModel>{

	public const float gravityY = -0.009f;

	public WorldController(){
		// Setup character animations
		SetupGameCharacters();
	}


	public FixedVector3 GetRandomSpawnPosition(WorldModel model){
		FixedVector3 res = new FixedVector3((40 + StateManager.state.Random.NextFloat(-4f, 4f)) * (model.lastSpawnWasLeft ? 1 : -1),9,0);
		model.lastSpawnWasLeft = !model.lastSpawnWasLeft;
		return res;
	}



	public override void Update(WorldModel model){

		// Get physics model. If doesn't exist, create it
		PhysicWorldModel physicsModel;
		PhysicWorldController physicsController;
		if (model.physicsModelId == ModelReference.InvalidModelIndex){
			// create world with map name and gravity
			physicsModel = new PhysicWorldModel("map 1", new FixedVector3(0,gravityY,0));
			model.physicsModelId = StateManager.state.AddModel(physicsModel);
			physicsController = physicsModel.GetController() as PhysicWorldController;
			// populate world
			PopulatePhysicsWorld(physicsModel, physicsController);

		}else {
			physicsModel = StateManager.state.GetModel(model.physicsModelId) as PhysicWorldModel;
			physicsController = physicsModel.GetController() as PhysicWorldController;
		}

		List<uint> allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers();
		Model playerModel;

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
				playerModel = new ShooterEntityModel("soldier", //playerId % 2 == 0 ? "Blaze" : "Rocha",
				                                	 "soldierIdle",
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
			}

			GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.players[playerId]);
			// using the name as a hack around having a variable in a script to tell the object is "initialized"
			// should be done in a better way.. but whatever, will do for the demo
			if (obj != null && !obj.name.EndsWith("[initiated]")){
				SkinnedMeshRenderer[] comps = obj.GetComponentsInChildren<SkinnedMeshRenderer>();

				bool isOwnPlayer = playerId == NetworkCenter.Instance.GetPlayerNumber();
				foreach (SkinnedMeshRenderer c in comps){
					c.material.color = (isOwnPlayer ? Color.blue : Color.red);
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
	
	
	
	
	private void SetupGameCharacters(){
		// TODO: read from somewhere.. right now, this is hardcoded..
		SetupCharacter("soldier");
	}


	private void SetupCharacter(string charName){
		
		AnimationController idle1Ctr = new AnimationController();
		AnimationController walkCtr = new AnimationController();
		AnimationController jumpCtr = new AnimationController();
		AnimationController fallCtr = new AnimationController();
		AnimationController fireCtr = new AnimationController();
		AnimationView idleView = new AnimationView();
		AnimationsVCPool.Instance.RegisterController(charName, "soldierIdle", idle1Ctr);
		AnimationsVCPool.Instance.RegisterController(charName, "soldierRun", walkCtr);
		AnimationsVCPool.Instance.RegisterController(charName, "soldierJump", jumpCtr);
		AnimationsVCPool.Instance.RegisterController(charName, "soldierFalling", fallCtr);
		AnimationsVCPool.Instance.RegisterController(charName, "soldierFiring", fireCtr);
		AnimationsVCPool.Instance.SetDefaultView(charName, idleView);
		
		
		List<AnimationTriggerCondition> conditions;
		AnimationTransition transition;

		// force walk move character against ground
		SingleEntityAnimationEvent<FixedVector3> zeroVelEvent = new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetAnimationVelocity,
			FixedVector3.Zero
		);
		walkCtr.AddEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetAnimationVelocity,
			new FixedVector3(0,-0.1, 0)
			));
		idle1Ctr.AddEvent(0, zeroVelEvent);
		jumpCtr.AddEvent(0, zeroVelEvent);
		fallCtr.AddEvent(0, zeroVelEvent);
		fireCtr.AddEvent(0, zeroVelEvent);


		
		// idle to walk
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new InputAxisMovingCondition());
		transition = new AnimationTransition("soldierRun", conditions, 0.1f);
		idle1Ctr.AddTransition(transition);
		// Force character to be stopped while idle
		idle1Ctr.AddEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetMaxInputVelocity,
			FixedVector3.Zero
			));
		
		// walk to iddle
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new NegateCondition(new InputAxisMovingCondition()));
		transition = new AnimationTransition("soldierIdle", conditions, 0.05f);
		walkCtr.AddTransition(transition);
		// Events that allow the character to move
		walkCtr.AddEvent(0, new SingleEntityAnimationEvent<bool>(GameEntityController.SetAutomaticFlip, true));
		walkCtr.AddEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetMaxInputVelocity,
			new FixedVector3(0.1f, 0, 0.0f)
		));


		// iddle, walk to jump
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new InputButtonCondition(InputButtonConditionType.pressed, true, 0));
		transition = new AnimationTransition("soldierJump", conditions, 0.05f);
		idle1Ctr.AddTransition(transition);
		walkCtr.AddTransition(transition);
		// Events to push the character up
		jumpCtr.AddEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetMaxInputVelocity,
			new FixedVector3(0.025f, 0, 0.0f)
		));
		jumpCtr.AddEvent(2, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetMaxInputVelocity,
			new FixedVector3(0.1f, 0, 0.0f)
			));
		jumpCtr.AddEvent(2, new SingleEntityAnimationEvent<bool>(GameEntityController.SetAutomaticFlip, false));
		jumpCtr.AddEvent(2, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.AddImpulse,
			new FixedVector3(0.0f, 0.2f, 0.0f)
		));

		// iddle, walk to fall
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new NegateCondition(new EntityBoolCondition(GameEntityController.IsGrounded)));
		transition = new AnimationTransition("soldierFalling", conditions, 0.05f);
		idle1Ctr.AddTransition(transition);
		walkCtr.AddTransition(transition);
		// Events that allow the character to move in air
		fallCtr.AddEvent(0, new SingleEntityAnimationEvent<bool>(GameEntityController.SetAutomaticFlip, true));
		fallCtr.AddEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetMaxInputVelocity,
			new FixedVector3(0.1f, 0, 0.0f)
		));

		// fall to idle
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new EntityBoolCondition(GameEntityController.IsGrounded));
		transition = new AnimationTransition("soldierIdle", conditions, 0.05f);
		fallCtr.AddTransition(transition);

		// jump to fall (rough version on this demo, cose no really jump anim exists)
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new NegateCondition(new EntityBoolCondition(GameEntityController.IsGrounded)));
		transition = new AnimationTransition("soldierFalling", conditions, 0.05f);
		jumpCtr.AddTransition(transition); // TODO: err.. supposed to be more complex than this, like moving down


		// Shoot!
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new InputButtonCondition(InputButtonConditionType.pressed, true, 1));
		conditions.Add(new EntityBoolCondition(ShooterEntityController.HasEnoughPowerToShoot));
		transition = new AnimationTransition("soldierFiring", conditions, 0.01f);
		idle1Ctr.AddTransition(transition);
		walkCtr.AddTransition(transition);
		jumpCtr.AddTransition(transition);
		fallCtr.AddTransition(transition);
		// fire event
		fireCtr.AddEvent(2, new SimpleEntityAnimationEvent(ShooterEntityController.Shoot));
//		fireCtr.AddEvent(2, new SingleEntityAnimationEvent<FixedVector3>(
//			GameEntityController.SetAnimationVelocity,
//			new FixedVector3(-0.05,-0.01, 0)
//		));
//		fireCtr.AddEvent(6, new SingleEntityAnimationEvent<FixedVector3>(
//			GameEntityController.SetAnimationVelocity,
//			FixedVector3.Zero
//			));
		// back to idle
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 12));
		transition = new AnimationTransition("soldierIdle", conditions, 0.05f);
		fireCtr.AddTransition(transition);


	}
	
}

