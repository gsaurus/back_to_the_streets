using System;
using RetroBread;
using ProtoBuf;

[ProtoContract]
public class BulletModel{

	[ProtoMember(1)]
	public FixedVector3 position;

	[ProtoMember(2)]
	public FixedVector3 velocity;

	[ProtoMember(3)]
	public int damage;

	// Default Constructor
	public BulletModel(){
		// Nothing to do
	}


	// Constructor
	public BulletModel(FixedVector3 position, FixedVector3 velocity, int damage){
		this.position = position;
		this.velocity = velocity;
		this.damage = damage;
	}

}
