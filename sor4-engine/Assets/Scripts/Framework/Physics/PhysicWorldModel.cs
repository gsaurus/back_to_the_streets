using System;
using System.Collections.Generic;


namespace RetroBread{


[Serializable]
public class PhysicWorldModel: Model<PhysicWorldModel> {

	// Gravity acceleration
	public FixedVector3 gravity;

	// This is used to load the world static properties on the controller
	public string worldName;

	// Id of the point models to wich we want to detect collisions against planes
	public SerializableList<uint> pointModels = new SerializableList<uint>();

	// Id of the dynamic plane models to wich we want to detect collisions against points
	public SerializableList<uint> planeModels = new SerializableList<uint>();
	
	// Constructor with name 	
	public PhysicWorldModel(string name, FixedVector3 gravity, int updateOrder = DefaultUpdateOrder.WorldUpdateOrder):base(updateOrder){
		worldName = name;
		this.gravity = gravity;
	}

	protected override Controller<PhysicWorldModel> CreateController()
	{
		return new PhysicWorldController(this);
	}
	
}



}

