using System;
using RetroBread;
using ProtoBuf;

[ProtoContract]
public class BulletModel : Model<BulletPointModel>{

	public const string BulletControllerFactoryId = "tank_bc";
	public const string BulletViewFactoryId = "tank_bv";

	[ProtoMember(1)]
	public ModelReference ownerId;

	[ProtoMember(2)]
	public FixedVector3 position;

	[ProtoMember(3)]
	public FixedVector3 velocity;

	[ProtoMember(4)]
	public int damage;

	// Default Constructor
	public BulletModel(){
		// Nothing to do
	}


	// Constructor
	public BulletModel(ModelReference ownerId, FixedVector3 position, FixedVector3 velocity, int damage)
	:base(null, DefaultUpdateOrder.PhysicsUpdateOrder)
	{
		// setup initial velocity
		this.ownerId = ownerId;
		this.position = position;
		this.velocity = velocity;
		this.damage = damage;
	}

	// Assign copy
	protected override void AssignCopy(BulletModel other){
		base.AssignCopy(other);
		if (other == null) return;
		ownerId = new ModelReference(other.ownerId);
		position = other.position;
		velocity = other.velocity;
		damage = other.damage;
	}

}

