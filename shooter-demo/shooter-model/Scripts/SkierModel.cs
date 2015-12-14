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

	[ProtoMember(7)]
	public ModelReference inputModelRef;

	// Default Constructor
	public SkierModel(){
		// set inactive
		fallenTimer = 0;
		frozenTimer = 0;
	}

	// Constructor
	public SkierModel(FixedFloat x, FixedFloat y, ModelReference inputModelRef){
		// setup initial velocity
		this.x = x;
		this.y = y;
		this.velX = 0;
		this.velY = 0;
		this.fallenTimer = 0;
		this.frozenTimer = 0;
		this.inputModelRef = inputModelRef;
	}

}

