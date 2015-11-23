using RetroBread;
using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class WorldModel:Model<WorldModel>{

	public const string WorldControllerFactoryId = "tank_worldc";
	public const string WorldViewFactoryId = "tank_worldv";

	public const int MaxWidth = 13;
	public const int MaxHeight = 13;
	public const int MaxPlayers = 8;

	// TODO: other public options to setup the world, and proper constructor
	// Such constructor will be called from the server only

	[ProtoMember(1, OverwriteList=true)]
	public int[] map;

	[ProtoMember(2, OverwriteList=true)]
	public TankModel[] tanks;

	[ProtoMember(3, OverwriteList=true)]
	public BulletModel[] bullets;

	// Reference to player input models
	// Key: player number; value: player model (tank)
	[ProtoMember(4, OverwriteList=true)]
	public ModelReference[] playersInputModelIds;

	// World settings
	[ProtoMember(5)] public uint tankEnergy = 1;
	[ProtoMember(6)] public uint bulletEnery = 1;
	[ProtoMember(7)] public uint numBullets = 2;
	[ProtoMember(8)] public FixedFloat tankVel = 0.04f;
	[ProtoMember(9)] public FixedFloat bulletVel = 0.12f;
	[ProtoMember(10)] public FixedFloat tankRotation = 0.07f;
	[ProtoMember(11)] public FixedFloat turretRotation = 0.06f;




	// Constructors
	public WorldModel():base(WorldControllerFactoryId, WorldViewFactoryId){
		// Default stuff
		map = new int[MaxWidth * MaxHeight];
		tanks = new TankModel[MaxPlayers];
		playersInputModelIds = new ModelReference[MaxPlayers];
		bullets = new BulletModel[MaxPlayers*numBullets];
	}

	public WorldModel(int[] map,
	                  uint tankEnergy,
	                  uint bulletEnery,
	                  uint numBullets,
	                  float tankVel,
	                  float bulletVel,
	                  float tankRotation,
	                  float turretRotation
     ):base(WorldControllerFactoryId, WorldViewFactoryId){
		this.map = map;
		this.tankEnergy = tankEnergy;
		this.bulletEnery = bulletEnery;
		this.numBullets = numBullets;
		this.tankVel = tankVel;
		this.bulletVel = bulletVel;
		this.tankRotation = tankRotation;
		this.turretRotation = turretRotation;

		map = new int[MaxWidth * MaxHeight];
		tanks = new TankModel[MaxPlayers];
		playersInputModelIds = new ModelReference[MaxPlayers];
		bullets = new BulletModel[MaxPlayers*numBullets];
	}

	public int MapValue(int x, int y){
		return map[y * MaxWidth + x];
	}

	public void SetMapValue(int x, int y, int value){
		map[y * MaxWidth + x] = value;
	}

	public BulletModel CreateBulletForPlayer(uint playerId){
		uint initialIndex = playerId * numBullets;
		for (uint i = initialIndex ; i < initialIndex + numBullets ; ++i){
			//Debug.Log("index " + i + ", initial index " + initialIndex + ", playerId: " + playerId + ", bullets size: " + bullets.Length);
			if (bullets[i] == null){
				bullets[i] = new BulletModel();
				return bullets[i];
			}
		}
		return null;
	}

}

