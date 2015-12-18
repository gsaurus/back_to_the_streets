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

	[ProtoMember(3)]
	public FixedFloat lastTrackX;
	[ProtoMember(4)]
	public FixedFloat maxYKnown;

	[ProtoMember(5)]
	public FixedFloat nextTrackX;

	[ProtoMember(6)]
	public FixedFloat nextTrackY;



	// Constructors
	public WorldModel():base(WorldControllerFactoryId, WorldViewFactoryId){
		// Default stuff
		skiers = new SkierModel[MaxPlayers];
		playersInputModelIds = new ModelReference[MaxPlayers];
		lastTrackX = FixedFloat.Zero;
		maxYKnown = FixedFloat.Zero;
		nextTrackX = FixedFloat.Zero;
		nextTrackY = FixedFloat.Zero;
	}

}

