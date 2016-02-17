using System;
using ProtoBuf;

namespace RetroBread.Storage{

// A character contains all the animations data,
// including data that may be duplicated between events and animations
// So hitboxes, collision boxes point to boxes in the character,
// animations point to events in the character,
// and events point to conditions in the character model
[ProtoContract]
public sealed class Character{

	// Logical character name
	[ProtoMember(1)]
	public string name;

	[ProtoMember(2, OverwriteList=true)]
	public CharacterEvent[] events;

	[ProtoMember(3, OverwriteList=true)]
	public GenericParameter[] conditions;

	[ProtoMember(4, OverwriteList=true)]
	public Box[] boxes;

	[ProtoMember(5, OverwriteList=true)]
	public CharacterAnimation[] animations;

	// Names of transforms in model used as anchors for attached objects
	[ProtoMember(6, OverwriteList=true)]
	public string[] viewAnchors;

	// Name of the 2D or 3D model(s) of the character (may have multiple skins)
	[ProtoMember(7, OverwriteList=true)]
	public string[] viewModels;

	// Default Constructor
	public Character(){
		// Nothing to do
	}

	// Constructor
	public Character(string name){
		this.name = name;
	}
		

}

} // namespace RetroBread.Storage
