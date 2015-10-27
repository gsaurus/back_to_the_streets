using System;
using RetroBread;
using ProtoBuf;

[ProtoContract]
public class BulletModel{

	[ProtoMember(1)]
	public FixedVector3 position;

	[ProtoMember(2)]
	public FixedVector3 velocity;

	// energy.. currently used to tell how many times it can bounce on walls
	[ProtoMember(3)]
	public int energy;

	// Default Constructor
	public BulletModel(){
		// Nothing to do
	}


	// Constructor
	public BulletModel(FixedVector3 position, FixedVector3 velocity, int energy){
		this.position = position;
		this.velocity = velocity;
		this.energy = energy;
	}

}

