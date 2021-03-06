using System;
using ProtoBuf;

namespace RetroBread.Storage{

// Information about ownership, delegation, events, etc
[ProtoContract]
public sealed class HUDObject{

	// Object name
	[ProtoMember(1)]
	public string name;

	// Ownership
	[ProtoMember(2)]
	public int teamId;
	[ProtoMember(3)]
	public int playerId;

	// Delegation
	[ProtoMember(4)]
	public bool attackAndGrabDelegation;

	// Events
	[ProtoMember(8, OverwriteList=true)]
	public GenericEvent[] events;

	// Default Constructor
	public HUDObject(){
		// Nothing to do
	}

}

} // namespace RetroBread.Storage
