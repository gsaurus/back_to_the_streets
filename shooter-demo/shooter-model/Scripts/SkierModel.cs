using System;
using RetroBread;
using ProtoBuf;



[ProtoContract]
public class SkierModel{

	[ProtoMember(1)]
	public FixedFloat y;

	[ProtoMember(2)]
	public FixedFloat x;

	[ProtoMember(3)]
	public FixedFloat orientationAngle;

	// If you fall (by collision), take a little time to stand up
	[ProtoMember(4)]
	public uint fallenTimer;

	[ProtoMember(5)]
	public ModelReference inputModelRef;

	// Default Constructor
	public SkierModel(){
		// set inactive
		fallenTimer = 0;
	}

	// Constructor
	public SkierModel(FixedFloat y, FixedFloat x, FixedFloat orientationAngle, ModelReference inputModelRef){
		// setup initial velocity
		this.y = y;
		this.x = x;
		this.orientationAngle = orientationAngle;
		this.fallenTimer = 0;
		this.inputModelRef = inputModelRef;
	}

}

