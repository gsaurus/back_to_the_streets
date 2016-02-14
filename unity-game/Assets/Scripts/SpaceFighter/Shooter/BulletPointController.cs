using System;
using System.Collections.Generic;
using RetroBread;

public class BulletPointController: PhysicPointController{
	
	public const uint totalBulletLifetimeFrames = 12;


	// Update natural physics 
	protected override void Update(PhysicPointModel model){
		base.Update(model);

		BulletPointModel bulletModel = model as BulletPointModel;
		if (bulletModel == null) return;

		// Antigravity
		FixedVector3 defaultVelocityAffector = bulletModel.GetDefaultVelocityAffector();
		SetDefaultVelocityAffector(model, new FixedVector3(defaultVelocityAffector.X,0, 0));


		// Check collisions against players
		WorldModel worldModel = StateManager.state.MainModel as WorldModel;
		// find the first target being hit, and hit only that one
		PhysicPointModel choosenPlayerPointModel = null;
		ShooterEntityController choosenPlayerController = null;
		FixedFloat minDeltaX = 9999;
		foreach (ModelReference playerId in worldModel.players.Values){
			if (playerId != bulletModel.shooterId){
				ShooterEntityModel playerModel = StateManager.state.GetModel(playerId) as ShooterEntityModel;
				if (playerModel == null || playerModel.invincibilityFrames > 0 || playerModel.energy <= 0) continue;
				PhysicPointModel playerPhysics = StateManager.state.GetModel(playerModel.physicsModelId) as PhysicPointModel;
				if (playerPhysics == null) continue;
				ShooterEntityController playerController = playerModel.Controller() as ShooterEntityController;
				if (playerController == null) continue;
				// check bounding box
				FixedFloat deltaX = FixedFloat.Abs(playerPhysics.position.X - bulletModel.lastPosition.X); 
				if (deltaX > minDeltaX) continue;
				if (   (bulletModel.lastPosition.X < playerPhysics.position.X && bulletModel.position.X > playerPhysics.position.X)
				    || (bulletModel.lastPosition.X > playerPhysics.position.X && bulletModel.position.X < playerPhysics.position.X)
				){
					if (bulletModel.position.Y > playerPhysics.position.Y && bulletModel.position.Y < playerPhysics.position.Y + 2.6){
						// potentially Hit!!
						minDeltaX = deltaX; 
						choosenPlayerPointModel = playerPhysics;
						choosenPlayerController = playerController;
					}
				}
			}
		}
		if (choosenPlayerController != null){
			choosenPlayerController.DamageTaken += ShooterEntityController.bulletDamage;
			choosenPlayerController.LastHitter = bulletModel.shooterId;
			bulletModel.position.X = choosenPlayerPointModel.position.X + UnityEngine.Random.Range(0.1f, 0.6f) * (bulletModel.lastPosition.X < choosenPlayerPointModel.position.X ? -1 : 1);
			RemoveFromWorld(bulletModel);
		}


		if (++bulletModel.lifetimeFrames > totalBulletLifetimeFrames){
			// This bullet went too far
			RemoveFromWorld(bulletModel);
		}

	}
	

	protected void RemoveFromWorld(PhysicPointModel pointModel){
		WorldModel worldModel = StateManager.state.MainModel as WorldModel;
		PhysicWorldModel physicWorldModel =  StateManager.state.GetModel(worldModel.physicsModelId) as PhysicWorldModel;
		PhysicWorldController worldController = physicWorldModel.Controller() as PhysicWorldController;
		worldController.RemovePoint(physicWorldModel, pointModel);
	}


	// Collision reaction.
	public override bool OnCollision(PhysicWorldModel world, PhysicPointModel pointModel, PhysicPlaneModel planeModel, FixedVector3 intersection){
		// On Collision, kill bullet
		pointModel.position = intersection;
		RemoveFromWorld(pointModel);
		return true;
	}


}

