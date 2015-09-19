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

	[ProtoMember(1)]
	public int[] map;

	[ProtoMember(2)]
	public TankModel[] tanks;

	[ProtoMember(3)]
	public BulletModel[] bullets;

	// Reference to player input models
	// Key: player number; value: player model (tank)
	[ProtoMember(4)]
	public ModelReference[] playersInputModelIds;





	// Constructor
	public WorldModel():base(WorldControllerFactoryId, WorldViewFactoryId){
		// Nothing to do
		map = new int[MaxWidth * MaxHeight];
		tanks = new TankModel[MaxPlayers];
		playersInputModelIds = new ModelReference[MaxPlayers];
		bullets = new BulletModel[MaxPlayers*MaxBulletsPerPlayer];

		// TODO: receive map from ctr or read from somewhere
		// For now use an hardcoded one
		map = new int[]{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 1, 0, 0, 0, 3, 3, 3, 0, 2, 0, 0, 0,
			0, 1, 0, 0, 0, 3, 3, 3, 0, 0, 0, 0, 0,
			0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 1, 1, 1, 1, 2, 0, 2, 0, 1, 0, 3, 3,
			0, 0, 0, 1, 1, 2, 0, 2, 0, 0, 0, 3, 3,
			0, 0, 0, 1, 1, 2, 2, 2, 0, 0, 0, 3, 3,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
			0, 2, 2, 2, 0, 0, 0, 1, 1, 0, 0, 0, 1,
			0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 2, 2,
			0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
		};
	}

}

