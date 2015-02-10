
using System;

[Serializable]
public class SpaceModel:Model<SpaceModel>{

	// Key: player number; value: ship model
	public SerializableDictionary<uint, ModelReference> ships = new SerializableDictionary<uint, ModelReference>();

	public ModelReference worldModelId = new ModelReference();

	// Main state doesn't need a view
//	protected override View<SpaceModel> CreateView(){
//		return null;
//	}
	
	// Create controller
	protected override Controller<SpaceModel> CreateController(){
		return new SpaceController();
	}
}

