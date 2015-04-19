using System;
using RetroBread;

// Shooter Model
[Serializable]
public class ShooterEntityModel: GameEntityModel {

	public const string ShooterEntityControllerFactoryId	= "my_sec";
	public const string ShooterEntityViewFactoryId			= "my_sev";
	
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
		State state,
		string characterName,
		string animationName,
		PhysicWorldModel worldModel,
		Model inputModel,
		FixedVector3 position,
		FixedVector3 stepTolerance,
		FixedFloat initialEnergy,
		uint initialInvincibility,
		FixedFloat initialGunPower
	):base(state,
	       characterName,
	       animationName,
	       worldModel,
	       inputModel,
	       position,
	       stepTolerance,
	       ShooterEntityControllerFactoryId,
	       ShooterEntityViewFactoryId,
	       DefaultUpdateOrder.EntitiesUpdateOrder
	){
		energy = initialEnergy;
		invincibilityFrames = initialInvincibility;
		gunPower = initialGunPower;
		gotHit = false;
	}
	


	public int GetBalance(){
		return (int)(totalKills - totalDeaths);
	}
	

}
