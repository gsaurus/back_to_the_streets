using System;


[Serializable]
public class BulletPointModel : PhysicPointModel{

	public uint lifetimeFrames;

	// Constructor
	public BulletPointModel(FixedVector3 position, bool moveRight, int updatingOrder = DefaultUpdateOrder.PhysicsUpdateOrder
	):base(null, updatingOrder)
	{
		// setup initial velocity
		velocityAffectors[defaultVelocityAffectorName] = new FixedVector3(2f * (moveRight ? 1 : -1), 0f, 0f);
		velocityAffectors["anti_gravity"] = new FixedVector3(0f,-WorldController.gravityY, 0);
		this.position = position;
		lifetimeFrames = 0;
	}


	// Create controller
	protected override Controller<PhysicPointModel> CreateController(){
		return new BulletPointController();
	}

	// Create view
	protected override View<PhysicPointModel> CreateView(){
		return new BulletPointView();
	}

}

