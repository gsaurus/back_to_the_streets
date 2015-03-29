
using System;
using UnityEngine;
using System.Collections.Generic;


public class WorldController:Controller<WorldModel>{



	public WorldController(){
		// Setup character animations
		SetupGameCharacters();
	}



	public override void Update(WorldModel model){

		// Get physics model. If doesn't exist, create it
		PhysicWorldModel physicsModel;
		PhysicWorldController physicsController;
		if (model.physicsModelId == ModelReference.InvalidModelIndex){
			// create world with map name and gravity
			physicsModel = new PhysicWorldModel("map 1", new FixedVector3(0,-0.008,0));
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
				playerModel = new GameEntityModel("soldier", //playerId % 2 == 0 ? "Blaze" : "Rocha",
				                                  "soldierIdle",
				                                  inputModel,
				                                  new FixedVector3(40 * (playerId % 2 == 0 ? -1 : 1),9,0),
				                                  physicsModel
				                                );
				// Model initial state
				GameEntityModel playerEntity = (GameEntityModel)playerModel;
				playerEntity.isFacingRight = playerId % 2 == 0;
				model.players[playerId] = StateManager.state.AddModel(playerModel);
			}

			GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.players[playerId]);
			if (obj != null){
				SkinnedMeshRenderer[] comps = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (SkinnedMeshRenderer c in comps){
					c.material.color = (playerId % 2 == 0 ? Color.blue : Color.red);
				}
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

			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(-7.65*flip,3.9,-1*flip),
			                             new FixedVector3(-7.65*flip,3.9,1*flip),
			                             new FixedVector3(-1.5*flip,3.9,1*flip),
			                             new FixedVector3(-1.5*flip,3.9,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(-11.85*flip,1.25,-1*flip),
			                             new FixedVector3(-11.85*flip,1.25,1*flip),
			                             new FixedVector3(-7.65*flip,3.9,1*flip),
			                             new FixedVector3(-7.65*flip,3.9,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(-17.75*flip,1.25,-1*flip),
			                             new FixedVector3(-17.75*flip,1.25,1*flip),
			                             new FixedVector3(-11.85*flip,1.25,1*flip),
			                             new FixedVector3(-11.85*flip,1.25,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);


			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(-17.75*flip,8.95,-1*flip),
			                             new FixedVector3(-17.75*flip,8.95,1*flip),
			                             new FixedVector3(-11.85*flip,8.95,1*flip),
			                             new FixedVector3(-11.85*flip,8.95,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(-11.85*flip,8.95,-1*flip),
			                             new FixedVector3(-11.85*flip,8.95,1*flip),
			                             new FixedVector3(-7.65*flip,11.62,1*flip),
			                             new FixedVector3(-7.65*flip,11.62,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);


			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(-45.65*flip,8.95,-1*flip),
			                             new FixedVector3(-45.65*flip,8.95,1*flip),
			                             new FixedVector3(-23.75*flip,8.95,1*flip),
			                             new FixedVector3(-23.75*flip,8.95,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(-45.65*flip,-11.5,-1*flip),
			                             new FixedVector3(-45.65*flip,-11.5,1*flip),
			                             new FixedVector3(-45.65*flip,8.95,1*flip),
			                             new FixedVector3(-45.65*flip,8.95,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(-23.75*flip,8.95,-1*flip),
			                             new FixedVector3(-23.75*flip,8.95,1*flip),
			                             new FixedVector3(-23.75*flip,-11.5,1*flip),
			                             new FixedVector3(-23.75*flip,-11.5,-1*flip)
			                             );
			physicsController.AddPlane(physicsModel, plane);
		}



		// Moving planes:
		plane = new MovingPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3[]{new FixedVector3(-23.75,1.25,-1), new FixedVector3(-23.75,8.95,-1)},
		                             new FixedVector3(-23.75,1.25,-1),
		                             new FixedVector3(-23.75,1.25,1),
		                             new FixedVector3(-17.75,1.25,1),
		                             new FixedVector3(-17.75,1.25,-1)
		                             );
		physicsController.AddPlane(physicsModel, plane);
		plane = new MovingPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3[]{new FixedVector3(17.75,8.95,-1), new FixedVector3(17.75,1.25,-1)},
									 new FixedVector3(17.75,1.25,-1),
									 new FixedVector3(17.75,1.25,1),
									 new FixedVector3(23.75,1.25,1),
									 new FixedVector3(23.75,1.25,-1)
		);
		physicsController.AddPlane(physicsModel, plane);

		plane = new MovingPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3[]{new FixedVector3(-7.65,11.62,-1), new FixedVector3(1.65,11.62,1)},
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
		AnimationView idleView = new AnimationView();
		AnimationsVCPool.Instance.RegisterController(charName, "soldierIdle", idle1Ctr);
		AnimationsVCPool.Instance.RegisterController(charName, "soldierRun", walkCtr);
		AnimationsVCPool.Instance.SetDefaultView(charName, idleView);
		
		
		List<AnimationTriggerCondition> conditions;
		AnimationTransition transition;
		
		// idle to walk
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new InputAxisMovingCondition());
		transition = new AnimationTransition("soldierRun", conditions, 0.15f);
		idle1Ctr.AddTransition(transition);
		// Force character to be stopped while idle
		idle1Ctr.AddEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetMaxInputVelocity,
			FixedVector3.Zero
			));
		
		// walk to iddle
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new NegateCondition(new InputAxisMovingCondition()));
		transition = new AnimationTransition("soldierIdle", conditions, 0.2f);
		walkCtr.AddTransition(transition);
		// Events that allow the character to move
		walkCtr.AddEvent(0, new SingleEntityAnimationEvent<bool>(GameEntityController.SetAutomaticFlip, true));
		walkCtr.AddEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetMaxInputVelocity,
			new FixedVector3(0.25f, 0, 0.0f)
			));
		
	}
	
}

