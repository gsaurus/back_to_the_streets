
using System;
using UnityEngine;
using System.Collections.Generic;


public class SpaceController:Controller<SpaceModel>{



	private void SetupRocha(){
		AnimationController idle1Ctr = new AnimationController();
		AnimationsVCPool.Instance.RegisterController("Rocha", "idle1", idle1Ctr);
		AnimationController walkCtr = new AnimationController();
		AnimationsVCPool.Instance.RegisterController("Rocha", "walk", walkCtr);
		AnimationView idleView = new AnimationView();
		AnimationsVCPool.Instance.SetDefaultView("Rocha", idleView);

		List<AnimationTriggerCondition> conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 180));
		AnimationTransition transition = new AnimationTransition("walk", conditions);
		idle1Ctr.AddTransition(transition);
		transition = new AnimationTransition("idle1", conditions);
		walkCtr.AddTransition(transition);
	}


	private void SetupBlaze(){
		AnimationController idle1Ctr = new AnimationController();
		AnimationsVCPool.Instance.RegisterController("Blaze", "idle1", idle1Ctr);
		AnimationController walkCtr = new AnimationController();
		AnimationsVCPool.Instance.RegisterController("Blaze", "walk", walkCtr);
		AnimationView idleView = new AnimationView();
		AnimationsVCPool.Instance.SetDefaultView("Blaze", idleView);

		List<AnimationTriggerCondition> conditions = new List<AnimationTriggerCondition>();
		conditions.Add(new AnimationFrameCondition(ArithmeticConditionOperatorType.greater, 180));
		AnimationTransition transition = new AnimationTransition("walk", conditions);
		idle1Ctr.AddTransition(transition);
		transition = new AnimationTransition("idle1", conditions);
		walkCtr.AddTransition(transition);
	}


	public override void Update(SpaceModel model){

		PhysicWorldModel worldModel;
		PhysicWorldController worldController;
		if (model.worldModelId == StateManager.invalidModelIndex){
			// create world
			worldModel = new PhysicWorldModel("test", new FixedVector3(0,-0.008,0));
			StateManager.state.AddModel(worldModel, OnWorldCreated, model);
			worldController = worldModel.GetController() as PhysicWorldController;

			SetupRocha();
			SetupBlaze();

			return; // nothing else until world is created
		}

		worldModel = StateManager.state.GetModel(model.worldModelId) as PhysicWorldModel;
		worldController = worldModel.GetController() as PhysicWorldController;

		if (worldModel.planeModels.Count == 0) {
			// for now add some "dynamic" planes
			PhysicPlaneModel plane;
			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(-9,-4.1,-9),
			                             new FixedVector3(-9,-4.1,9),
			                             new FixedVector3(0,0.0,9),
			                             new FixedVector3(0,0.0,-9)
			                             );
			worldController.AddPlane(plane);

			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(0,0,-9),
			                             new FixedVector3(0,0,9),
			                             new FixedVector3(4,0.0,9),
			                             new FixedVector3(4,0.0,-9)
			                             );
			worldController.AddPlane(plane);

			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(4,-4.1,9),
			                             new FixedVector3(4,-4.1,-9),
			                             new FixedVector3(4,0,-9),
			                             new FixedVector3(4,0,9)
			                             );
			worldController.AddPlane(plane);

			// Moving plane:
			plane = new MovingPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(4,-3,-9),
			                             new FixedVector3(4,-3,9),
			                             new FixedVector3(9,-3,9),
			                             new FixedVector3(9,-3,-9)
			                             );
			worldController.AddPlane(plane);

			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(4,-2.5,-9),
			                             new FixedVector3(4,-2.5,9),
			                             new FixedVector3(9,-2.5,9),
			                             new FixedVector3(9,-2.5,-9)
			                             );
			worldController.AddPlane(plane);

			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(-7.8,-4.1,9),
			                             new FixedVector3(-7.8,-4.1,-9),
			                             new FixedVector3(-7.8,4.1,-9),
			                             new FixedVector3(-7.8,4.1,9)
			                             );
			worldController.AddPlane(plane);

			plane = new PhysicPlaneModel(PhysicWorldController.PhysicsUpdateOrder,
			                             new FixedVector3(8,-4.1,-9),
			                             new FixedVector3(8,-4.1,9),
			                             new FixedVector3(8,4.1,9),
			                             new FixedVector3(8,4.1,-9)
			                             );
			worldController.AddPlane(plane);
		}

		List<uint> allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers();
		ShipModel shipModel;

		// Remove ships for inactive players
		foreach (KeyValuePair<uint, uint> pair in model.ships){
			if (!allPlayers.Exists(x => x == pair.Key)){
				// Doesn't exist anymore, remove ship
				shipModel = StateManager.state.GetModel(pair.Value) as ShipModel;
				//worldController.RemovePoint(shipModel, OnShipDestroyed, model);
				StateManager.state.RemoveModel(shipModel, OnShipDestroyed, model);
			}
		}

		// Create ships for new players
		foreach(uint playerId in allPlayers){
			if (!model.ships.ContainsKey(playerId)){
				shipModel = new ShipModel(playerId); 
				//worldController.AddPoint(shipModel, OnShipCreated, model);
				StateManager.state.AddModel(shipModel, OnShipCreated, model);
			}
		}

	}


	private void OnShipCreated(Model model, object mainModelObj){
		ShipModel ship = model as ShipModel;
		SpaceModel mainModel = mainModelObj as SpaceModel;
		if (ship == null || mainModel == null) return;
		mainModel.ships[ship.player] = ship.Index;
	}


	private void OnShipDestroyed(Model model, object mainModelObj){
		ShipModel ship = model as ShipModel;
		SpaceModel mainModel = mainModelObj as SpaceModel;
		if (ship == null || mainModel == null) return;
		mainModel.ships.Remove(ship.player);
	}

	private void OnWorldCreated(Model model, object mainModelObj){
		SpaceModel mainModel = mainModelObj as SpaceModel;
		mainModel.worldModelId = model.Index;
	}


	public override void PostUpdate(SpaceModel model){
		// Nothing to do here
	}

}

