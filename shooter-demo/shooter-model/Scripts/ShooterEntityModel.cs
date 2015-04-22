using System;
using RetroBread;
using ProtoBuf;

// Shooter Model
[ProtoContract]
public class ShooterEntityModel: GameEntityModel {

	public const string ShooterEntityControllerFactoryId	= "my_sec";
	public const string ShooterEntityViewFactoryId			= "my_sev";
	
	// Shooter's energy
	[ProtoMember(1)]
	public FixedFloat energy;

	// How much damage taken during a frame
	//public FixedFloat damageTaken;

	// bool telling the entity got hit on the last frame
	[ProtoMember(2)]
	public bool gotHit;

	// if > 0, shooter is invincible
	[ProtoMember(3)]
	public uint invincibilityFrames;

	// if == 0, can't shoot bullets
	// power increases with time, and shooting takes power
	[ProtoMember(4)]
	public FixedFloat gunPower;

	// round stats
	[ProtoMember(5)] public uint totalDeaths;
	[ProtoMember(6)] public uint totalKills;


	// Default Constructor
	public ShooterEntityModel(){
		// Nothing to do
	}

	// Constructor
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
