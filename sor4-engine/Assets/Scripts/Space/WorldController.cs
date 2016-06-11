using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class WorldController:Controller<WorldModel>{



#region Initializations

	static WorldController(){
		// setup imutable stuff
	}


	public WorldController(){
		
	}



	private void HandlePlayerConnections(State state, WorldModel model){

		List<uint> allPlayers;

		if (model.skiers [0] == null) {
			// initialize skiers
			if (StateManager.Instance.IsNetworked) {
				allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers ();
			} else {
				allPlayers = new List<uint> ();
				allPlayers.Add(0);
				int maxPlayers;
				maxPlayers = UnityEngine.Random.Range (3, 7);
				if (maxPlayers == 6) {
					maxPlayers = 5;
				}
				for (uint i = 1; i < maxPlayers; ++i)
					allPlayers.Add(i);
			}
			
			// Create characters for new players
			int playerPosition = StateManager.Instance.IsNetworked ? -1 : UnityEngine.Random.Range (0, allPlayers.Count);
			FixedFloat playerX = 0;
			FixedFloat distanceBetweenPlayers = 2.2f;
			foreach (uint playerId in allPlayers) {
				if (model.skiers[playerId] == null) {
					Model inputModel = new PlayerInputModel (playerId);
					ModelReference inputModelRef = state.AddModel (inputModel);
					playerX = (int)playerId * distanceBetweenPlayers;
					if (playerPosition >= 0) {
						if (playerId == 0) {
							playerX = playerPosition * distanceBetweenPlayers;
						} else if (playerId == playerPosition) {
							playerX = 0;
						}
					}
					model.skiers [playerId] = new SkierModel (playerX, 0, inputModelRef);
				}
			}
		}



		// Remove characters for inactive players
		if (StateManager.Instance.IsNetworked) {
			allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers();
			if (allPlayers.Count > 1) {
				for (int i = 0; i < model.skiers.Length; ++i) {
					if (!allPlayers.Exists (x => x == i)) {
						// Doesn't exist anymore, remove
						model.skiers [i] = null;
					}
				}
			}
		}
	}
	


#endregion
	




	protected override void Update(State state, WorldModel model){

		HandlePlayerConnections(state, model);

	}
	
}

