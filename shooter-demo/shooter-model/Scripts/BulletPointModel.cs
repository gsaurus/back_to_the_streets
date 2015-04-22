using System;
using RetroBread;
using ProtoBuf;

[ProtoContract]
public class BulletPointModel : PhysicPointModel{

	public const string BulletPointControllerFactoryId = "my_bpc";
	public const string BulletPointViewFactoryId = "my_bpv";

	[ProtoMember(1)]
	public ModelReference shooterId;

	[ProtoMember(2)]
	public uint lifetimeFrames;

	// Default Constructor
	public BulletPointModel(){
		// Nothing to do
	}


	// Constructor
	public BulletPointModel(ModelReference shooterId, FixedVector3 position, bool moveRight)
	:base(null, BulletPointControllerFactoryId, BulletPointViewFactoryId, DefaultUpdateOrder.PhysicsUpdateOrder)
	{
		// setup initial velocity
		this.shooterId = shooterId;
		velocityAffectors[defaultVelocityAffectorName] = new FixedVector3(1.2f * (moveRight ? 1 : -1), 0f, 0f);
		this.position = position;
		lifetimeFrames = 0;
	}

	protected override void AssignCopy(PhysicPointModel other){
		base.AssignCopy(other);
		BulletPointModel otherBullet = other as BulletPointModel;
		if (otherBullet == null) return;
		shooterId = new ModelReference(otherBullet.shooterId);
		lifetimeFrames = otherBullet.lifetimeFrames;

	}

}

