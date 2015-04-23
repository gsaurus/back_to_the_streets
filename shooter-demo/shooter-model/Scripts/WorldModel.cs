using RetroBread;
using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class WorldModel:Model<WorldModel>{

	public const string WorldControllerFactoryId = "my_worldc";

	// Reference to player models
	// Key: player number; value: player model
	[ProtoMember(1)]
	public Dictionary<uint, ModelReference> players = new Dictionary<uint, ModelReference>();

	// Reference to physics model
	[ProtoMember(2)]
	public ModelReference physicsModelId = new ModelReference();

	// used to decide where to spawn next player
	[ProtoMember(3)]
	public bool lastSpawnWasLeft;


	// Constructor
	public WorldModel():base(WorldControllerFactoryId){
		// Nothing to do
	}


	protected override void AssignCopy(WorldModel other){
		base.AssignCopy(other);

		lastSpawnWasLeft = other.lastSpawnWasLeft;
		physicsModelId = new ModelReference(other.physicsModelId);
		players = new Dictionary<uint, ModelReference>(other.players.Count);
		foreach(KeyValuePair<uint, ModelReference> pair in other.players){
			players.Add(pair.Key, new ModelReference(pair.Value));
		}

	}

}

