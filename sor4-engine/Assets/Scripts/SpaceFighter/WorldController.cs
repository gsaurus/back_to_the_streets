
using System;
using UnityEngine;
using System.Collections.Generic;


public class WorldController:Controller<WorldModel>{



	public WorldController(){
		// Setup character animations
		SetupGameCharacters();
	}

	/*private void SetupRocha(){
		SetupCharacter("Rocha");
	}


	private void SetupBlaze(){
		SetupCharacter("Blaze");
	}


	private void SetupCharacter(string charName){
	
		AnimationController idle1Ctr = new AnimationController();
		AnimationController walkCtr = new AnimationController();
		AnimationView idleView = new AnimationView();
		AnimationsVCPool.Instance.RegisterController(charName, "idle1", idle1Ctr);
		AnimationsVCPool.Instance.RegisterController(charName, "walk", walkCtr);
		AnimationsVCPool.Instance.SetDefaultView(charName, idleView);


		List<AnimationTriggerCondition> conditions;
		AnimationTransition transition;
		
		// idle to walk
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new InputAxisMovingCondition());
		transition = new AnimationTransition("walk", conditions, 0.15f);
		idle1Ctr.AddTransition(transition);
		// Force character to be stopped while idle
		idle1Ctr.AddEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetMaxInputVelocity,
			FixedVector3.Zero
		));
		
		// walk to iddle
		conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new NegateCondition(new InputAxisMovingCondition()));
		transition = new AnimationTransition("idle1", conditions, 0.2f);
		walkCtr.AddTransition(transition);
		// Events that allow the character to move
		walkCtr.AddEvent(0, new SingleEntityAnimationEvent<bool>(GameEntityController.SetAutomaticFlip, true));
		walkCtr.AddEvent(0, new SingleEntityAnimationEvent<FixedVector3>(
			GameEntityController.SetMaxInputVelocity,
			new FixedVector3(0.1f, 0, 0.1f)
		));

	}*/




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

//		// Remove characters for inactive players
//		foreach (KeyValuePair<uint, ModelReference> pair in model.players){
//			if (!allPlayers.Exists(x => x == pair.Key)){
//				// Doesn't exist anymore, remove ship
//				playerModel = StateManager.state.GetModel(pair.Value);
//				//worldController.RemovePoint(shipModel, OnShipDestroyed, model);
//				StateManager.state.RemoveModel(playerModel, OnPlayerRemoved, model);
//			}
//		}
//
//		// Create characters for new players
//		foreach(uint playerId in allPlayers){
//			if (!model.players.ContainsKey(playerId)){
//				Model inputModel = new PlayerInputModel(playerId);
//				playerModel = new GameEntityModel(playerId % 2 == 0 ? "Blaze" : "Rocha",
//				                                  "idle1",
//				                                  inputModel,
//				                                  new FixedVector3(0,4,0),
//				                                  physicsModel
//				                                );
//				model.players[playerId] = StateManager.state.AddModel(playerModel);
//			}
//		}

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

		plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3(-9,-4,7),
		                             new FixedVector3(-9,4,7),
		                             new FixedVector3(9,4,7),
		                             new FixedVector3(9,-4,7)
		                             );
		physicsController.AddPlane(physicsModel, plane);
		
		plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3(-9,-4.1,-9),
		                             new FixedVector3(-9,-4.1,9),
		                             new FixedVector3(0,0.0,9),
		                             new FixedVector3(0,0.0,-9)
		                             );
		physicsController.AddPlane(physicsModel, plane);
		
		plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3(0,0,-9),
		                             new FixedVector3(0,0,9),
		                             new FixedVector3(4,0.0,9),
		                             new FixedVector3(4,0.0,-9)
		                             );
		physicsController.AddPlane(physicsModel, plane);
		
		plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3(4,-4.1,9),
		                             new FixedVector3(4,-4.1,-9),
		                             new FixedVector3(4,0,-9),
		                             new FixedVector3(4,0,9)
		                             );
		physicsController.AddPlane(physicsModel, plane);
		
		// Moving plane:
		plane = new MovingPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3(4,-3,-9),
		                             new FixedVector3(4,-3,9),
		                             new FixedVector3(9,-3,9),
		                             new FixedVector3(9,-3,-9)
		                             );
		physicsController.AddPlane(physicsModel, plane);
		
		plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3(4,-2.5,-9),
		                             new FixedVector3(4,-2.5,9),
		                             new FixedVector3(9,-2.5,9),
		                             new FixedVector3(9,-2.5,-9)
		                             );
		physicsController.AddPlane(physicsModel, plane);
		
		plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3(-7.8,-4.1,9),
		                             new FixedVector3(-7.8,-4.1,-9),
		                             new FixedVector3(-7.8,4.1,-9),
		                             new FixedVector3(-7.8,4.1,9)
		                             );
		physicsController.AddPlane(physicsModel, plane);
		
		plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
		                             new FixedVector3(8,-4.1,-9),
		                             new FixedVector3(8,-4.1,9),
		                             new FixedVector3(8,4.1,9),
		                             new FixedVector3(8,4.1,-9)
		                             );
		physicsController.AddPlane(physicsModel, plane);
	}




	private void SetupGameCharacters(){
		// TODO: read from somewhere.. right now, this is hardcoded..
	}

}

