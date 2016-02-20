using System;
using ProtoBuf;

namespace RetroBread.Storage{

// Event is an action that happens depending on a certain condition
// E.g. at end of animation, jump to other animation; if get hit, play sound, etc
// Both conditions and events may be composed
[ProtoContract]
public sealed class CharacterEvent{

	// Pointers to GenericParameters
	[ProtoMember(1)]
	public int[] conditionIds;

	// Pointers to GenericParameters
	[ProtoMember(2)]
	public int[] eventIds;

	// Default Constructor
	public CharacterEvent(){
		// Nothing to do
	}

	// Constructor
	public CharacterEvent(int[] conditionIds, int[] eventIds){
		this.conditionIds = conditionIds;
		this.eventIds = eventIds;
	}
		

}

} // namespace RetroBread.Storage
