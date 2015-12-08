using RetroBread;
using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class WorldModel:Model<WorldModel>{

	public const string WorldControllerFactoryId = "ski_worldc";
	public const string WorldViewFactoryId = "ski_worldv";

	public const uint MaxPlayers = 6;

	// Our players list
	[ProtoMember(1, OverwriteList=true)]
	public SkierModel[] skiers;

	// Reference to player input models
	// Key: player number; value: player model (skier)
	[ProtoMember(2, OverwriteList=true)]
	public ModelReference[] playersInputModelIds;


	// Constructors
	public WorldModel():base(WorldControllerFactoryId, WorldViewFactoryId){
		// Default stuff
		skiers = new SkierModel[MaxPlayers];
		playersInputModelIds = new ModelReference[MaxPlayers];
	}

}

