using System;
using RetroBread;
using ProtoBuf;



[ProtoContract]
public class TankModel{

	[ProtoMember(1)]
	public FixedVector3 position;

	[ProtoMember(2)]
	public int energy;

	[ProtoMember(3)]
	public FixedFloat movingAngle;

	[ProtoMember(4)]
	public FixedFloat shootingAngle;

	[ProtoMember(5)]
	public FixedFloat velocityModule;

	[ProtoMember(6)]
	public int timeToRespawn;

	[ProtoMember(7)]
	public ModelReference inputModelRef;

	// Default Constructor
	public TankModel(){
		// set inactive
		timeToRespawn = -1;
	}

	// Constructor
	public TankModel(FixedVector3 position, int energy, FixedFloat initialDirection, ModelReference inputModelRef){
		// setup initial velocity
		this.position = position;
		this.energy = energy;
		this.movingAngle = initialDirection;
		this.shootingAngle = 0;
		this.velocityModule = 0;
		this.timeToRespawn = 0;
		this.inputModelRef = inputModelRef;
	}

}

