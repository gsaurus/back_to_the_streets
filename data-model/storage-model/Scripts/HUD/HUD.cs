using System;
using ProtoBuf;

namespace RetroBread.Storage{

// HUD data contains information about what animations to trigger on character variables updates
[ProtoContract]
public sealed class HUD{

	// Note: Bundle name where the HUD assets are is the same name as the storage data file name

	// Name of the root prefab, the one containing all hud elements
	[ProtoMember(1)]
	public string mainPrefabName;

	// Parameters, used in events
	[ProtoMember(2, OverwriteList=true)]
	public GenericParameter[] genericParameters;

	// Objects information
	[ProtoMember(3, OverwriteList=true)]
	public HUDObject[] objects;


	// Default Constructor
	public HUD(){
		// Nothing to do
	}

	// Constructor
	public HUD(string prefabName){
		this.mainPrefabName = prefabName;
	}
		

}

} // namespace RetroBread.Storage
