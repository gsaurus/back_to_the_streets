using System;


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

	// Step tolerance, used to try to climb small steps
	public FixedVector3 stepTolerance;

	// Collision impact is the maximum difference of point VS plane velocities
	public FixedVector3 collisionInpact;

	// Velocity = difference between current and previous positions
	public FixedVector3 GetVelocity(){
		return position - lastPosition;
	}

	// Forces applied to this point, identified by a string (force source)
	public SerializableDictionary<string,FixedVector3> velocityAffectors;


	// Constructor
	public PhysicPointModel(ModelReference ownerId, FixedVector3 position, FixedVector3 stepTolerance, int updatingOrder = DefaultUpdateOrder.PhysicsUpdateOrder):base(updatingOrder){
		this.ownerId = ownerId;
		velocityAffectors = new SerializableDictionary<string,FixedVector3>();
		this.position = this.lastPosition = position;
		this.stepTolerance = stepTolerance;
	}

	// Constructor
	public PhysicPointModel(ModelReference ownerId, int updatingOrder = DefaultUpdateOrder.PhysicsUpdateOrder):base(updatingOrder){
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

