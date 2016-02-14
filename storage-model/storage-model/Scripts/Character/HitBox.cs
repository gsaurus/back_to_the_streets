using System;
using ProtoBuf;

namespace RetroBread.Storage{

// A Hitbox may last for a few frames
// It is parameterized for different hit effects
[ProtoContract]
public sealed class HitBox{

	[ProtoMember(1)]
	public int boxId;

	[ProtoMember(2)]
	public int startFrame;

	[ProtoMember(3)]
	public int endFrame;

	[ProtoMember(4)]
	public GenericParameter param;

	// Default Constructor
	public HitBox(){
		// Nothing to do
	}

	// Constructor
	public HitBox(int boxId, int startFrame, int endFrame, GenericParameter param){
		this.boxId = boxId;
		this.startFrame = startFrame;
		this.endFrame = endFrame;
		this.param = param;
	}
		

}

} // namespace RetroBread.Storage
