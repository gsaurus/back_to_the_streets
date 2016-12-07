using System;
using RetroBread;
using ProtoBuf;

namespace RetroBread.Storage{


// Int Array Wrapper
[ProtoContract]
public sealed class GenericIntsList{
	[ProtoMember(1, OverwriteList=true)]
	public int[] list;

	public GenericIntsList(int[] list){
		this.list = list;
	}
}

// String Array Wrapper
[ProtoContract]
public sealed class GenericStringsList{
	[ProtoMember(1, OverwriteList=true)]
	public string[] list;

	public GenericStringsList(string[] list){
		this.list = list;
	}
}

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

	[ProtoMember(6, OverwriteList=true)]
	public GenericIntsList[] intsListList;

	[ProtoMember(7, OverwriteList=true)]
	public GenericStringsList[] stringsListList;

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