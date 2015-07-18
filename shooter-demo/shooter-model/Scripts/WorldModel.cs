using RetroBread;
using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class WorldModel:Model<WorldModel>{

	public const string WorldControllerFactoryId = "tank_worldc";
	public const string WorldViewFactoryId = "tank_worldv";

	// Reference to player models
	// Key: player number; value: player model (tank)
	[ProtoMember(1)]
	public Dictionary<uint, ModelReference> players = new Dictionary<uint, ModelReference>();

	[ProtoMember(2)]
	public int[] map;

	[ProtoMember(3)]
	public int width;

	[ProtoMember(4)]
	public int height;



	// Constructor
	public WorldModel(int width, int height):base(WorldControllerFactoryId){
		// Nothing to do
		this.width = width;
		this.height = height;
		map = new int[width * height];
	}


	protected override void AssignCopy(WorldModel other){
		base.AssignCopy(other);
		players = new Dictionary<uint, ModelReference>(other.players.Count);
		foreach(KeyValuePair<uint, ModelReference> pair in other.players){
			players.Add(pair.Key, new ModelReference(pair.Value));
		}

		this.map = other.map;
		this.width = other.width;
		this.height = other.height;

	}

}

