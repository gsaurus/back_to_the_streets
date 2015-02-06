using System;


[Serializable]
public struct CollisionFlags{
	public bool ground;
	public bool left;
	public bool right;
	public bool far;
	public bool near;

	public void Clear(){
		ground = left = right = far = near = false;
	}
}


[Serializable]
public class PhysicPointModel : Model<PhysicPointModel>{

	// Key of default world velocity affector
	[NonSerialized]
	public static readonly string defaultVelocityAffectorName = "defaultVelocityAffectorName";

	// Id of the model that owns this point, used to get the respective gameobject from the owner
	public ModelReference ownerId;

	// Linear Position
	public FixedVector3 position;

	// Position on the previous frame
	public FixedVector3 lastPosition;

	// Store here the direction of last collisions:
	public CollisionFlags collisionFlags;

	// Velocity = difference between current and previous positions
	public FixedVector3 GetVelocity(){
		return position - lastPosition;
	}

	// Forces applied to this point, identified by a string (force source)
	public SerializableDictionary<string,FixedVector3> velocityAffectors;


	// Constructor
	public PhysicPointModel(ModelReference ownerId, FixedVector3 position, int updatingOrder = 0):base(updatingOrder){
		this.ownerId = ownerId;
		velocityAffectors = new SerializableDictionary<string,FixedVector3>();
		this.position = this.lastPosition = position;
	}

	// Constructor
	public PhysicPointModel(ModelReference ownerId, int updatingOrder = 0):base(updatingOrder){
		this.ownerId = ownerId;
		velocityAffectors = new SerializableDictionary<string,FixedVector3>();
	}


	// Create controller
	protected override Controller<PhysicPointModel> CreateController(){
		return new PhysicPointController();
	}

	// Create view
	protected override View<PhysicPointModel> CreateView(){
		return new PhysicPointView();
	}

	// Default world velocity affector
	public FixedVector3 GetDefaultVelocityAffector(){
		FixedVector3 vec;
		if (velocityAffectors.TryGetValue(defaultVelocityAffectorName, out vec))
			return vec;
		return FixedVector3.Zero;
	}

}

