using System;
using RetroBread;
using ProtoBuf;



[ProtoContract]
public class SkierModel{

	[ProtoMember(1)]
	public FixedFloat x;

	[ProtoMember(2)]
	public FixedFloat y;

	[ProtoMember(3)]
	public FixedFloat velX;

	[ProtoMember(4)]
	public FixedFloat velY;

	// If you fall (by collision), take a little time to stand up
	[ProtoMember(5)]
	public uint fallenTimer;

	// If you miss a checkpoint, be frozen for a few frames
	[ProtoMember(6)]
	public uint frozenTimer;

	// Used to slowdown the skier when doing tight curves
	[ProtoMember(7)]
	public FixedFloat frictionX;
	[ProtoMember(8)]
	public FixedFloat frictionY;

	[ProtoMember(9)]
	public FixedFloat targetVelX;
	[ProtoMember(10)]
	public FixedFloat targetVelY;
	
	[ProtoMember(11)]
	public ModelReference inputModelRef;

	// Default Constructor
	public SkierModel(){
		// set inactive
		fallenTimer = 0;
		frozenTimer = 0;
	}

	// Constructor
	public SkierModel(FixedFloat x, FixedFloat y, ModelReference inputModelRef){
		this.x = x;
		this.y = y;
		// moving straight down
		this.velX = 0;
		this.velY = 0;
		this.targetVelX = 0;
		this.targetVelY = -1.0f;

		// initial full friction, so it will accelerate at the start
		this.frictionX = 1.0f;
		this.frictionY = 1.0f;

		this.fallenTimer = 0;
		this.frozenTimer = 0;
		this.inputModelRef = inputModelRef;
	}

}

