using UnityEngine;
using System.Collections;
using RetroBread;

// 
public class ShooterEntityController: GameEntityController {

	public const float maxEnergy = 1f;
	public const float bulletDamage = 0.2f;
	public const uint maxInvincibilityFrames = 240;
	public const float maxGunPower = 1f;
	public const float gunPowerPerFrame = 0.008f;
	public const float gunPowerPerShot = 0.0f; //0.3f;
	public const float worldLowerBound = -20f;

	// How much damage taken during a frame
	private FixedFloat damageTaken;
	private ModelReference lastHitter;

	public FixedFloat DamageTaken{
		get{
			if (StateManager.state.IsPostUpdating) return 0;
			return damageTaken;
		}
		set{
			if (!StateManager.state.IsPostUpdating){
				damageTaken = value;
			}
		}
	}

	public ModelReference LastHitter{
		get{
			if (StateManager.state.IsPostUpdating) return new ModelReference();
			return lastHitter;
		}
		set{
			if (!StateManager.state.IsPostUpdating){
				lastHitter = value;
			}
		}
	}


	protected override void PostUpdate(GameEntityModel model){
		
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

		if (damageTaken != 0){
			shooterModel.energy -= damageTaken;
			shooterModel.gotHit = true;
			if (shooterModel.energy < 0){
				// energy is left as zero, but not killed yet, animation will handle it
				shooterModel.energy = 0;
				ShooterEntityModel hitterModel = StateManager.state.GetModel(lastHitter) as ShooterEntityModel;
				if (hitterModel != null){
					++hitterModel.totalKills;
				}
			}
			damageTaken = 0;
			ShooterEntityView view = shooterModel.View() as ShooterEntityView;
			if (view != null){
				view.OnHit(shooterModel);
			}
		}else {
			shooterModel.gotHit = false;
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
			PhysicPointController pointController = pointModel.Controller() as PhysicPointController;
			WorldController worldController = world.Controller() as WorldController;
			FixedVector3 respawnPosition = worldController.GetRandomSpawnPosition(world);
			pointController.SetPosition(pointModel, respawnPosition);
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

		PhysicPointModel shooterPointModel = StateManager.state.GetModel(shooterModel.physicsModelId) as PhysicPointModel;
		if (shooterPointModel == null) return;

		// instantiate new bullet
		FixedVector3 bulletInitialPosition = shooterPointModel.position + new FixedVector3(0.3f * (shooterModel.isFacingRight ? 1 : -1), 2.1f, 0f);
		BulletPointModel bulletModel = new BulletPointModel(model.Index, bulletInitialPosition, shooterModel.isFacingRight);
		WorldModel worldModel = StateManager.state.MainModel as WorldModel;
		PhysicWorldModel physicWorldModel =  StateManager.state.GetModel(worldModel.physicsModelId) as PhysicWorldModel;
		PhysicWorldController worldController = physicWorldModel.Controller() as PhysicWorldController;
		worldController.AddPoint(physicWorldModel, bulletModel);
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
