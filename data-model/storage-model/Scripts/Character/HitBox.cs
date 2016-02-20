using System;
using ProtoBuf;

namespace RetroBread.Storage{

// A Hitbox may last for a few frames
// It is parameterized for different hit effects
[ProtoContract]
public sealed class HitBox{

	[ProtoMember(1, OverwriteList=true)]
	public int[] boxIds;

	[ProtoMember(2)]
	public int paramId;

	// Default Constructor
	public HitBox(){
		// Nothing to do
	}

	// Constructor
	public HitBox(int[] boxIds, int paramId){
		this.boxIds = boxIds;
		this.paramId = paramId;
	}
		

}

} // namespace RetroBread.Storage
