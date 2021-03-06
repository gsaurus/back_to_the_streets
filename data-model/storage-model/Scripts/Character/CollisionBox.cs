using System;
using ProtoBuf;

namespace RetroBread.Storage{

// A collision box varies along the animation frames
// So, in fact, it's a list of boxes
[ProtoContract]
public sealed class CollisionBox{

	[ProtoMember(1, OverwriteList=true)]
	public int[] boxIds;

	// Default Constructor
	public CollisionBox(){
		// Nothing to do
	}

	// Constructor
	public CollisionBox(int[] boxIds){
		this.boxIds = boxIds;
	}
		

}

} // namespace RetroBread.Storage
