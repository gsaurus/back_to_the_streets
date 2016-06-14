using System;
using System.Collections.Generic;
using RetroBread;
using ProtoBuf;


// Bullet is set with initial and final position, and on each tick it's current position is 
// the interpolation in between, based on elapsed frames
[ProtoContract]
public class BulletModel{

	[ProtoMember(1)]
	public ModelReference ownerShip;

	[ProtoMember(2)]
	public FixedFloat originX;
	[ProtoMember(3)]
	public FixedFloat originY;

	[ProtoMember(4)]
	public FixedFloat targetX;
	[ProtoMember(5)]
	public FixedFloat targetY;

	[ProtoMember(6)]
	public int bornFrameNum;
	[ProtoMember(7)]
	public int deathFrameNum;

	[ProtoMember(8)]
	public int type;



	// Default Constructor
	public BulletModel(){

	}

	// Constructor
	public BulletModel(ModelReference ownerShip, int type, FixedFloat x, FixedFloat y, FixedFloat targetX, FixedFloat targetY, int currFrame, int lifeTime){
		this.ownerShip = ownerShip;
		this.type = type;
		this.originX = x;
		this.originY = y;
		this.targetX = targetX;
		this.targetY = targetY;
		this.bornFrameNum = currFrame;
		this.deathFrameNum = currFrame + lifeTime;
	}

}

