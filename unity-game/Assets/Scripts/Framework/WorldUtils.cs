using System;
using System.Collections;

namespace RetroBread{


	public static class WorldUtils{

		// Get an entity given it's team and player number inside that team
		public static GameEntityModel GetEntityFromTeam(int teamId, int playerNumber){
			// Get world model, then get teams model, find the respective team and then inside it find the respective player
			WorldModel world = StateManager.state.MainModel as WorldModel;
			TeamsManagerModel teamsModel = StateManager.state.GetModel(world.teamsModelId) as TeamsManagerModel;
			if (teamsModel.teams.Length > teamId) {
				TeamData teamData = teamsModel.teams[teamId];
				if (teamData.entities.Count > playerNumber) {
					return StateManager.state.GetModel(teamData.entities[(int)playerNumber]) as GameEntityModel;
				}
			}
			// entity not found
			return null;
		}


		// Find any entity interacting with the one from the required team, except if it matches the given exception reference
		public static GameEntityModel GetInteractionEntityWithEntityFromTeam(int teamId, int playerNumber, ModelReference exception){
			GameEntityModel originalEntity = GetEntityFromTeam(teamId, playerNumber);
			ModelReference interactionReference = new ModelReference();
			foreach (ModelReference entityRef in originalEntity.anchoredEntities) {
				if (entityRef != exception) {
					interactionReference = entityRef;
					break;
				}
			}

			if (interactionReference == ModelReference.InvalidModelIndex && originalEntity.parentEntity != exception) {
				interactionReference = originalEntity.parentEntity;
			}
			if (interactionReference == ModelReference.InvalidModelIndex){
				// Need to check with controller to see last hitter / hitten entities
				GameEntityController controller = originalEntity.Controller() as GameEntityController;
				foreach (HitInformation hitInfo in controller.lastHurts) {
					if (hitInfo.entityId != exception) {
						interactionReference = hitInfo.entityId;
						break;
					}
				}
				if (interactionReference == ModelReference.InvalidModelIndex) {
					foreach (HitInformation hitInfo in controller.lastHits) {
						if (hitInfo.entityId != exception) {
							interactionReference = hitInfo.entityId;
							break;
						}
					}
				}
			}

			if (interactionReference != ModelReference.InvalidModelIndex) {
				return StateManager.state.GetModel(interactionReference) as GameEntityModel;
			}
			return null;

		}

	}
}

