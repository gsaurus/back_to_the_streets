using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class WorldController:Controller<WorldModel>{

	public const uint totalGameFrames = 7200; // 2 minutes

	public FixedFloat maxTankVelocity = 0.05f;

	static WorldController(){
		// setup imutable stuff
	}


	public WorldController(){
		// Nothing to do
	}



	private void HandlePlayerConnections(WorldModel model){
		List<uint> allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers();

		// Remove characters for inactive players
		for (int i = 0 ; i < model.tanks.Length ; ++i){
			if (!allPlayers.Exists(x => x == i)){
				// Doesn't exist anymore, remove
				model.tanks[i] = null;
			}
		}
		
		// Create characters for new players
		foreach(uint playerId in allPlayers){
			if (model.tanks[playerId] == null){
				Model inputModel = new PlayerInputModel(playerId, 1, 1);
				ModelReference inputModelRef = StateManager.state.AddModel(inputModel);
				model.tanks[playerId] = new TankModel(InitialPositionForTankId(playerId), 3, 0, inputModelRef);
			}
			
		}
	}


	private FixedVector3 InitialPositionForTankId(uint tankId){
		// ...
		switch (tankId) {
			case 0: return new FixedVector3(1,1,0);
			case 1: return new FixedVector3(WorldModel.MaxWidth - 1, 1, 0);
			case 2: return new FixedVector3(WorldModel.MaxWidth - 1, WorldModel.MaxHeight - 1, 0);
			case 3: return new FixedVector3(1,WorldModel.MaxHeight - 1, 0);
			case 4: return new FixedVector3(WorldModel.MaxWidth * 0.5f, 1, 0);
			case 5: return new FixedVector3(WorldModel.MaxWidth * 0.5f,WorldModel.MaxHeight - 1, 0);
			case 6: return new FixedVector3(1, WorldModel.MaxHeight * 0.5f,0);
			case 7: return new FixedVector3(WorldModel.MaxWidth - 1, WorldModel.MaxHeight * 0.5f,0);
		}
		return new FixedVector3(1,1,0);
	}



	private void UpdateTankPositions(WorldModel world){
		foreach (TankModel tank in world.tanks){
			if (tank != null){
				UpdateTankPosition(world, tank);
			}
		}
	}


	private void UpdateTankPosition(WorldModel world, TankModel tank){
		PlayerInputModel inputModel = StateManager.state.GetModel(tank.inputModelRef) as PlayerInputModel;
		if (inputModel == null || inputModel.axis[0] == FixedVector3.Zero) {
			// Not moving, get out
			return;
		}
		inputModel.axis[0].Normalize();
		FixedVector3 targetPosition = tank.position + new FixedVector3(inputModel.axis[0].X * maxTankVelocity, inputModel.axis[0].Z * maxTankVelocity, 0);
		tank.position = CollisionResponse(tank.position, targetPosition, 0.48f);
	}


	private FixedVector3 CollisionResponse(FixedVector3 origin, FixedVector3 target, FixedFloat radius){
		// If no collision, all good
		return target;
	}


	protected override void Update(WorldModel model){
		
		HandlePlayerConnections(model);

		UpdateTankPositions(model);

	}
	
}

