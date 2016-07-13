using System;
using ProtoBuf;

namespace RetroBread.Storage{

// HUD data contains information about what animations to trigger on character variables updates
[ProtoContract]
public sealed class HUD{

	// Bundle where the HUD assets are
	[ProtoMember(1)]
	public string bundleName;

	// Name of the root prefab, the one containing all hud elements
	[ProtoMember(2)]
	public string mainPrefabName;

//	[ProtoMember(3, OverwriteList=true)]
//	public Box[] boxes;


	// Default Constructor
	public HUD(){
		// Nothing to do
	}

	// Constructor
	public HUD(string bundleName, string prefabName){
		this.bundleName = bundleName;
		this.mainPrefabName = prefabName;
	}
		

}

} // namespace RetroBread.Storage
