using System;

[Serializable]
public class PhysicPointModel : Model<PhysicPointModel>{

	// Key of default world velocity affector
	[NonSerialized]
	public static readonly string defaultVelocityAffectorName = "defaultVelocityAffectorName";

	// Linear Position
	public FixedVector3 position;

	// Position on the previous frame
	public FixedVector3 lastPosition;

	// Velocity = difference between current and previous positions
	public FixedVector3 GetVelocity(){
		return position - lastPosition;
	}

	// Forces applied to this point, identified by a string (force source)
	public SerializableDictionary<string,FixedVector3> velocityAffectors;


	// Constructor
	public PhysicPointModel(FixedVector3 position, int updatingOrder = 0):base(updatingOrder){
		velocityAffectors = new SerializableDictionary<string,FixedVector3>();
		this.position = this.lastPosition = position;
	}

	// Constructor
	public PhysicPointModel(int updatingOrder = 0):base(updatingOrder){
		velocityAffectors = new SerializableDictionary<string,FixedVector3>();
	}


	// Create controller
	protected override Controller<PhysicPointModel> CreateController(){
		return new PhysicPointController();
	}

	// Default world velocity affector
	public FixedVector3 GetDefaultVelocityAffector(){
		FixedVector3 vec;
		if (velocityAffectors.TryGetValue(defaultVelocityAffectorName, out vec))
			return vec;
		return FixedVector3.Zero;
	}

}

