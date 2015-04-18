using System;
using RetroBread;

[Serializable]
public class BulletPointModel : PhysicPointModel{

	public const string BulletPointControllerFactoryId = "my_bpc";
	public const string BulletPointViewFactoryId = "my_bpv";


	public ModelReference shooterId;

	public uint lifetimeFrames;

	// Constructor
	public BulletPointModel(ModelReference shooterId, FixedVector3 position, bool moveRight, int updatingOrder = DefaultUpdateOrder.PhysicsUpdateOrder)
	:base(null, BulletPointControllerFactoryId, BulletPointViewFactoryId, updatingOrder)
	{
		// setup initial velocity
		this.shooterId = shooterId;
		velocityAffectors[defaultVelocityAffectorName] = new FixedVector3(1.2f * (moveRight ? 1 : -1), 0f, 0f);
		this.position = position;
		lifetimeFrames = 0;
	}

}

