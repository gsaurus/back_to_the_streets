using RetroBread;
using System;

[Serializable]
public class WorldModel:Model<WorldModel>{

	public const string WorldControllerFactoryId = "my_worldc";

	// Reference to player models
	// Key: player number; value: player model
	public SerializableDictionary<uint, ModelReference> players = new SerializableDictionary<uint, ModelReference>();

	// Reference to physics model
	public ModelReference physicsModelId = new ModelReference();

	// used to decide where to spawn next player
	public bool lastSpawnWasLeft;


	// Constructor
	public WorldModel():base(WorldControllerFactoryId)
	{
		// Nothing to do
	}

}

