using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class WorldController:Controller<WorldModel>{

	public const uint totalGameFrames = 9997200; // 2 minutes


#region Initializations

	static WorldController(){
		// setup imutable stuff
	}


	public WorldController(){
		// Nothing to do
	}



	private void HandlePlayerConnections(WorldModel model){
		List<uint> allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers();

		// Remove characters for inactive players
		for (int i = 0 ; i < model.skiers.Length ; ++i){
			if (!allPlayers.Exists(x => x == i)){
				// Doesn't exist anymore, remove
				model.skiers[i] = null;
			}
		}
		
		// Create characters for new players
		foreach(uint playerId in allPlayers){
			if (model.skiers[playerId] == null){
				Model inputModel = new PlayerInputModel(playerId);
				ModelReference inputModelRef = StateManager.state.AddModel(inputModel);
				model.skiers[playerId] = new SkierModel(playerId * 1.0f, 0, inputModelRef);
			}
			
		}
	}
	


#endregion
	


#region Tanks

	private void UpdateSkiers(WorldModel world){

		SkierModel skier;
		// For each tank update it's position and orientation
		for (uint skierId = 0 ; skierId < world.skiers.Length ; ++skierId){
			skier = world.skiers[skierId];
			if (skier != null){

				PlayerInputModel inputModel = StateManager.state.GetModel(skier.inputModelRef) as PlayerInputModel;
				if (inputModel != null){

					// Use input to update skier direction

				}

			}

			// Update skier position

		}

	}


#endregion



	protected override void Update(WorldModel model){
		
		HandlePlayerConnections(model);
		UpdateSkiers(model);
	}
	
}

