using System;
using RetroBread;
using ProtoBuf;

namespace RetroBread.Storage{

// Two points defines a box
[ProtoContract]
public sealed class Box{

	[ProtoMember(1)]
	public FixedVector3 pointOne;

	[ProtoMember(2)]
	public FixedVector3 pointTwo;

	// Default Constructor
	public Box(){
		// Nothing to do
	}


	// Constructor
	public Box(FixedVector3 pointOne, FixedVector3 pointTwo){
		this.pointOne = pointOne;
		this.pointTwo = pointTwo;
	}
		

}


} // namespace RetroBread.Storage
