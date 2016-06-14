using RetroBread;
using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class WorldModel:Model<WorldModel>{

	public const string WorldControllerFactoryId = "space_worldc";
	public const string WorldViewFactoryId = "space_worldv";

	public const uint MaxPlayers 	= 20;
	public const uint MaxAsteroids	= 50;
	public const uint MaxBullets	= 2000;

	// Our players list
	[ProtoMember(1, OverwriteList=true)]
	public ShipModel[] ships;

	// Reference to player input models
	// Key: player number; value: player model (ship)
	[ProtoMember(2, OverwriteList=true)]
	public ModelReference[] playersInputModelIds;

	// World asteroids
	[ProtoMember(3, OverwriteList=true)]
	public ShipModel[] asteroids;

	// Bullets in the world
	[ProtoMember(4, OverwriteList=true)]
	public BulletModel[] bullets;

	[ProtoMember(5)]
	public int totalBulletsInGame;



	// Constructors
	public WorldModel():base(WorldControllerFactoryId, WorldViewFactoryId){
		// Default stuff
		ships = new ShipModel[MaxPlayers];
		asteroids = new ShipModel[MaxAsteroids];
		playersInputModelIds = new ModelReference[MaxPlayers];
		bullets = new BulletModel[MaxBullets];
		totalBulletsInGame = 0;
	}

}

