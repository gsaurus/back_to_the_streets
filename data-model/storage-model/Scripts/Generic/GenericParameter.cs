using System;
using RetroBread;
using ProtoBuf;

namespace RetroBread.Storage{

// Something parametherised with a type and arguments of various possible types
[ProtoContract]
public sealed class GenericParameter{

	[ProtoMember(1)]
	public int type;

	[ProtoMember(2, OverwriteList=true)]
	public int[] intsList;

	[ProtoMember(3, OverwriteList=true)]
	public FixedFloat[] floatsList;

	[ProtoMember(4, OverwriteList=true)]
	public string[] stringsList;

	[ProtoMember(5, OverwriteList=true)]
	public bool[] boolsList;

	// Default Constructor
	public GenericParameter(){
		// Nothing to do
	}


	// Constructor
	public GenericParameter(int type){
		this.type = type;
	}
		

}


} // namespace RetroBread.Storage