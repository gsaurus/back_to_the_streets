using UnityEditor;

public class CreateAssetBundles{

	[MenuItem ("Assets/Export Asset Bundles", false, 500)]
	static void BuildAllAssetBundles(){
		AssetDatabase.RemoveUnusedAssetBundleNames();
		BuildPipeline.BuildAssetBundles("AssetBundles");
	}

}