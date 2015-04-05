using UnityEngine;
using System;
using System.Collections;

// Shooter Model
[Serializable]
public class ShooterEntityModel: GameEntityModel {
	
	// Shooter's energy
	public FixedFloat energy;

	// How much damage taken during a frame
	//public FixedFloat damageTaken;

	// bool telling the entity got hit on the last frame
	public bool gotHit;

	// if > 0, shooter is invincible
	public uint invincibilityFrames;

	// if == 0, can't shoot bullets
	// power increases with time, and shooting takes power
	public FixedFloat gunPower;

	// round stats
	public uint totalDeaths;
	public uint totalKills;

	

	public ShooterEntityModel(
		string characterName,
		string animationName,
		PhysicWorldModel worldModel,
		Model inputModel,
		FixedVector3 position,
		FixedVector3 stepTolerance,
		FixedFloat initialEnergy,
		uint initialInvincibility,
		FixedFloat initialGunPower,
		int updatingOrder = DefaultUpdateOrder.EntitiesUpdateOrder
	):base(characterName, animationName, worldModel, inputModel, position, stepTolerance, updatingOrder)
	{
		energy = initialEnergy;
		invincibilityFrames = initialInvincibility;
		gunPower = initialGunPower;
		gotHit = false;
	}


	protected override View<GameEntityModel> CreateView(){
		return new ShooterEntityView();
	}
	
	
	protected override Controller<GameEntityModel> CreateController(){
		return new ShooterEntityController();
	}


	public int GetBalance(){
		return (int)(totalKills - totalDeaths);
	}
	

}
