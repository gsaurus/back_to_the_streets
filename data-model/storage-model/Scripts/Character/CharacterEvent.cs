using System;
using ProtoBuf;

namespace RetroBread.Storage{

// Event is an action that happens depending on a certain condition
// E.g. at end of animation, jump to other animation; if get hit, play sound, etc
// Both conditions and events may be composed
[ProtoContract]
public sealed class CharacterEvent{

	[ProtoMember(1)]
	public int conditionId;

	[ProtoMember(2)]
	public GenericParameter param;

	// Default Constructor
	public CharacterEvent(){
		// Nothing to do
	}

	// Constructor
	public CharacterEvent(int conditionId, GenericParameter param){
		this.conditionId = conditionId;
		this.param = param;
	}
		

}

} // namespace RetroBread.Storage
