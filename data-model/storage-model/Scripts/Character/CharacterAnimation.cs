using System;
using ProtoBuf;

namespace RetroBread.Storage{

// A character contains all the animations data,
// including data that may be duplicated between events and animations
// So hitboxes, collision boxes point to boxes in the character,
// animations point to events in the character,
// and events point to conditions in the character model
[ProtoContract]
public sealed class CharacterAnimation{

	[ProtoMember(1)]
	public string name;

	[ProtoMember(2)]
	public int numFrames;

	[ProtoMember(3, OverwriteList=true)]
	public HitBox[] hitBoxes;

	[ProtoMember(4, OverwriteList=true)]
	public CollisionBox[] collisionBoxes;

	[ProtoMember(5, OverwriteList=true)]
	public GenericEvent[] events;

	// Default Constructor
	public CharacterAnimation(){
		// Nothing to do
	}

	// Constructor
	public CharacterAnimation(string name, int numFrames){
		this.name = name;
		this.numFrames = numFrames;
	}
		

}

} // namespace RetroBread.Storage
