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
	public const int MaxPlayers = 10;
	public const int MaxBulletsPerPlayer = 2;

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





	// Constructors
	public WorldModel():base(WorldControllerFactoryId, WorldViewFactoryId){
		// Nothing to do
		map = new int[MaxWidth * MaxHeight];
		tanks = new TankModel[MaxPlayers];
		playersInputModelIds = new ModelReference[MaxPlayers];
		bullets = new BulletModel[MaxPlayers*MaxBulletsPerPlayer];
	}

	public WorldModel(int[] map):this(){
		this.map = map;
	}

	public int MapValue(int x, int y){
		return map[y * MaxWidth + x];
	}

}

