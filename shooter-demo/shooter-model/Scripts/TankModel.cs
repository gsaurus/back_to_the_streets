using System;
using RetroBread;
using ProtoBuf;



[ProtoContract]
public class TankModel{

	[ProtoMember(1)]
	public FixedVector3 position;

	[ProtoMember(2)]
	public uint energy;

	[ProtoMember(3)]
	public FixedFloat orientationAngle;

	[ProtoMember(4)]
	public FixedFloat turretAngle;

	[ProtoMember(5)]
	public FixedFloat turretTargetAngle;

	[ProtoMember(6)]
	public int timeToRespawn;

	[ProtoMember(7)]
	public ModelReference inputModelRef;

	[ProtoMember(8)]
	public bool movingBackwards;

	// Default Constructor
	public TankModel(){
		// set inactive
		timeToRespawn = -1;
	}

	// Constructor
	public TankModel(FixedVector3 position, uint energy, FixedFloat initialDirection, ModelReference inputModelRef){
		// setup initial velocity
		this.position = position;
		this.energy = energy;
		this.orientationAngle = initialDirection;
		this.turretAngle = 0;
		this.turretTargetAngle = 0;
		this.timeToRespawn = 0;
		this.inputModelRef = inputModelRef;
		this.movingBackwards = false;
	}

}

