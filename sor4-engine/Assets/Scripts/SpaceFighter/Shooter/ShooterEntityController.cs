using UnityEngine;
using System.Collections;

// 
public class ShooterEntityController: GameEntityController {

	public const float maxEnergy = 1f;
	public const uint maxInvincibilityFrames = 120;
	public const float maxGunPower = 1f;
	public const float gunPowerPerFrame = 0.005f;
	public const float gunPowerPerShot = 0.3f;
	public const float worldLowerBound = -20f;

	// How much damage taken during a frame
	private FixedFloat damageTaken;
	

//	public override void Update(GameEntityModel model){
//
//		base.Update(model);
//
//		ShooterEntityModel shooterModel = model as ShooterEntityModel;
//		if (shooterModel == null) return;
//		
//	}

	public override void PostUpdate(GameEntityModel model){
		
		base.PostUpdate(model);
		
		ShooterEntityModel shooterModel = model as ShooterEntityModel;
		if (shooterModel == null) return;

		if (shooterModel.invincibilityFrames > 0){
			--shooterModel.invincibilityFrames;
		}
		if (shooterModel.gunPower < maxGunPower) {
			shooterModel.gunPower += gunPowerPerFrame;
			if (shooterModel.gunPower > maxGunPower){
				shooterModel.gunPower = maxGunPower;
			}
		}

		if (damageTaken > 0){
			shooterModel.energy -= damageTaken;
			shooterModel.gotHit = true;
			if (shooterModel.energy < 0){
				// energy is left as zero, but not killed yet, animation will handle it
				shooterModel.energy = 0;
			}
			damageTaken = 0;
		}

		// if going too low, instantly kill it
		PhysicPointModel pointModel = GetPointModel(model);
		if (pointModel.position.Y < worldLowerBound){
			KillAndRespawn(model);
		}
		
	}



#region affectors (events)

	// Respawn shooter in world
	public static void KillAndRespawn(GameEntityModel model){
		ShooterEntityModel shooterModel = model as ShooterEntityModel;
		if (shooterModel == null) return;

		// respawn point model
		WorldModel world = StateManager.state.MainModel as WorldModel;
		if (world != null){
			PhysicPointModel pointModel = GetPointModel(model);
			PhysicPointController pointController = pointModel.GetController() as PhysicPointController;
			WorldController worldController = world.Controller() as WorldController;
			FixedVector3 respawnPosition = worldController.GetRandomSpawnPosition(world);
			pointController.SetPosition(respawnPosition);
			shooterModel.isFacingRight = respawnPosition.X < 0;
		}

		// reset shooter
		shooterModel.energy = maxEnergy;
		shooterModel.gunPower = 0;
		shooterModel.invincibilityFrames = maxInvincibilityFrames;
		++shooterModel.totalDeaths;
	}


	public static void Shoot(GameEntityModel model){
		ShooterEntityModel shooterModel = model as ShooterEntityModel;
		if (shooterModel == null) return;

		shooterModel.gunPower -= gunPowerPerShot;
		// TODO: instantiate new bullet
		UnityEngine.Debug.Log("Shoot!");
	}

	
#endregion


#region getters (conditions)


	public static bool IsDead(GameEntityModel model){
		ShooterEntityModel shooterModel = model as ShooterEntityModel;
		if (shooterModel == null) return false;
		return shooterModel.energy <= 0;
	}

	public static bool GotHit(GameEntityModel model){
		ShooterEntityModel shooterModel = model as ShooterEntityModel;
		if (shooterModel == null) return false;
		return shooterModel.gotHit;
	}

	public static bool HasEnoughPowerToShoot(GameEntityModel model){
		ShooterEntityModel shooterModel = model as ShooterEntityModel;
		if (shooterModel == null) return false;
		return shooterModel.gunPower >= gunPowerPerShot;
	}
	

#endregion
	

}
