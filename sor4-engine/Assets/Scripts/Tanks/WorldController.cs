using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class WorldController:Controller<WorldModel>{

	public const uint totalGameFrames = 7200; // 2 minutes

	static WorldController(){
		// setup imutable stuff
	}


	public WorldController(){
		// Nothing to do
	}



	private void HandlePlayerConnections(){
		List<uint> allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers();

		// eh, we handle it later..
//		Model playerModel;
//		
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
//				FixedVector3 initialPosition = GetRandomSpawnPosition(model);
//				playerModel = new ShooterEntityModel(StateManager.state,
//				                                     "soldier", //playerId % 2 == 0 ? "Blaze" : "Rocha",
//				                                     "soldierIdleRelaxed",
//				                                     physicsModel,
//				                                     inputModel,
//				                                     initialPosition,
//				                                     new FixedVector3(0, 0.5, 0), // step tolerance
//				                                     ShooterEntityController.maxEnergy,
//				                                     ShooterEntityController.maxInvincibilityFrames,
//				                                     0
//				                                     );
//				// Model initial state
//				ShooterEntityModel playerEntity = (ShooterEntityModel)playerModel;
//				playerEntity.isFacingRight = initialPosition.X < 0;
//				model.players[playerId] = StateManager.state.AddModel(playerModel);
//			}
//			
//		}
	}



	protected override void Update(WorldModel model){
		
		HandlePlayerConnections();

	}
	
}

