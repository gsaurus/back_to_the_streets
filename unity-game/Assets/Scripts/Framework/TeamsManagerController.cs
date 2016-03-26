using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{


	// 
	public class TeamsManagerController: Controller<TeamsManagerModel> {

		// Collisions matrix
		private static bool[,] teamsCollisionMatrix;

		// Hits matrix
		private static bool[,] teamsHitsMatrix;


		// Setup collisions matrix, should not be modified during gameplay
		public static void SetupCollisionMatrixes(bool[,] teamsCollisionMatrix, bool[,] teamsHitsMatrix){
			TeamsManagerController.teamsCollisionMatrix = teamsCollisionMatrix;
			TeamsManagerController.teamsHitsMatrix = teamsHitsMatrix;
		}



		// Check collisions between all entities of the two given teams
		private void CheckCollisions(List<ModelReference> team1Refs, List<ModelReference> team2Refs){
			GameEntityModel entity1, entity2;
			GameEntityController entity1Controller;
			State gameState = StateManager.state;

			foreach (ModelReference team1EntityRef in team1Refs) {
				entity1 = gameState.GetModel(team1EntityRef) as GameEntityModel;
				if (entity1 != null && !GameEntityController.IsCollidingWithOthers(entity1)) {
					entity1Controller = entity1.Controller() as GameEntityController;
					if (entity1Controller != null) {
						foreach (ModelReference team2EntityRef in team2Refs) {
							entity2 = gameState.GetModel(team2EntityRef) as GameEntityModel;
							if (entity2 != null && entity2 != entity1 && entity1Controller.CollisionCollisionCheck(entity1, entity2)){
								// collision detected for this entity, no need for further checks
								break;
							} // if collision check
						} // foreach team2 entity
					} // entity1 controller != null
				} // entity1 != null & not colliding
			} // foreach entity 1
		
		} // method check collision


		// Check hits between all entities of the two given teams
		private void CheckHits(List<ModelReference> team1Refs, List<ModelReference> team2Refs){
			GameEntityModel entity1, entity2;
			GameEntityController entity1Controller;
			State gameState = StateManager.state;

			foreach (ModelReference team1EntityRef in team1Refs) {
				entity1 = gameState.GetModel(team1EntityRef) as GameEntityModel;
				if (entity1 != null) {
					entity1Controller = entity1.Controller() as GameEntityController;
					if (entity1Controller != null) {
						foreach (ModelReference team2EntityRef in team2Refs) {
							entity2 = gameState.GetModel(team2EntityRef) as GameEntityModel;
							if (entity2 != null && entity2 != entity1){
								entity1Controller.HitCollisionCheck(entity1, entity2);
							} // if entity2 != null
						} // foreach team2 entity
					} // entity1 controller != null
				} // entity1 != null & not colliding
			} // foreach entity 1
		}



		// Update: check hits/collisions between teams, as defined in the hit/collision matrixes
		protected override void Update(TeamsManagerModel model){

			// Clear everyone's collision data
			GameEntityModel entity;
			GameEntityController entityController;
			State gameState = StateManager.state;
			foreach (TeamData teamData in model.teams) {
				foreach (ModelReference modelRef in teamData.entities) {
					entity = gameState.GetModel(modelRef) as GameEntityModel;
					if (entity != null) {
						entityController = entity.Controller() as GameEntityController;
						if (entityController != null) {
							entityController.ClearHitsInformation();
						}
					}
				}
			}


			// Check hit/collisions between teams
			for (int i = 0; i < model.teams.Length; ++i) {
				for (int j = i; j < model.teams.Length; ++j) {
					// check collisions
					if (teamsCollisionMatrix[i, j]) {
						CheckCollisions(model.teams[i].entities, model.teams[j].entities);
					}
					// check hits
					if (teamsHitsMatrix[i, j]) {
						CheckHits(model.teams[i].entities, model.teams[j].entities);
					}
				}
			}


		}


		protected override void PostUpdate(TeamsManagerModel model){
			// Nothing atm
		}


	}



}
