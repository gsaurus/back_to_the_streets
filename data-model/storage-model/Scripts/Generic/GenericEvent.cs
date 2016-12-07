using System;
using ProtoBuf;

namespace RetroBread.Storage{

// Event is an action that happens depending on a certain condition
// E.g. at end of animation, jump to other animation; if get hit, play sound, etc
// Both conditions and events may be composed
[ProtoContract]
public sealed class GenericEvent{

	// Pointers to GenericParameters
	[ProtoMember(1)]
	public int[] conditionIds;

	// Pointers to GenericParameters
	[ProtoMember(2)]
	public int[] eventIds;

	// Pointers to GenericParameters
	[ProtoMember(3)]
	public int[] subjectIds;

	// Default Constructor
	public GenericEvent(){
		// Nothing to do
	}

	// Constructor
	public GenericEvent(int[] subjectIds, int[] conditionIds, int[] eventIds){
		this.subjectIds = subjectIds;
		this.conditionIds = conditionIds;
		this.eventIds = eventIds;
	}
		

}

} // namespace RetroBread.Storage
