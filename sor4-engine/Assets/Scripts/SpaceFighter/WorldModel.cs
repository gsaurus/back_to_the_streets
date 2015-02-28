
using System;

[Serializable]
public class WorldModel:Model<WorldModel>{

	// Reference to player models
	// Key: player number; value: player model
	public SerializableDictionary<uint, ModelReference> players = new SerializableDictionary<uint, ModelReference>();

	// Reference to physics model
	public ModelReference physicsModelId = new ModelReference();
	
	// Create controller
	protected override Controller<WorldModel> CreateController(){
		return new WorldController();
	}
}

