using System;
using RetroBread;
using ProtoBuf;

[ProtoContract]
public class TankModel : Model<TankModel>{

	public const string TankControllerFactoryId = "tank_tc";
	public const string TankViewFactoryId = "tank_tv";

	// Input model
	[ProtoMember(1)]
	public ModelReference inputModelId;

	[ProtoMember(2)]
	public FixedVector3 position;

	[ProtoMember(3)]
	public int energy;

	[ProtoMember(4)]
	public FixedFloat movingAngle;

	[ProtoMember(5)]
	public FixedFloat shootingAngle;

	[ProtoMember(6)]
	public FixedFloat velocityModule;

	[ProtoMember(7)]
	public FixedFloat maxVelocityModule;


	// Default Constructor
	public TankModel(){
		// Nothing to do
	}

	// Constructor
	public TankModel(ModelReference inputModelId, FixedVector3 position, int energy, FixedFloat initialDirection, FixedFloat maxVelocityModule)
	:base(null, DefaultUpdateOrder.PhysicsUpdateOrder)
	{
		// setup initial velocity
		this.inputModelId = inputModelId;
		this.position = position;
		this.energy = energy;
		this.movingAngle = initialDirection;
		this.shootingAngle = 0;
		this.velocityModule = 0;
		this.maxVelocityModule = maxVelocityModule;
	}

	// Assign copy
	protected override void AssignCopy(TankModel other){
		base.AssignCopy(other);
		if (other == null) return;
		inputModelId = new ModelReference(other.inputModelId);
		position = other.position;
		energy = other.energy;
		movingAngle = other.movingAngle;
		shootingAngle = other.shootingAngle;
		velocityModule = other.velocityModule;
		maxVelocityModule = other.maxVelocityModule;
	}

}

