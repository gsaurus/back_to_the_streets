using UnityEditor;

public class CreateAssetBundles{

//#if UNITY_IPHONE
//	static string platform = "iOS";
//#elif UNITY_ANDROID
//	static string platform = "android";
//#else
//	static string platform = "pc";
//#endif

	static string outputFolder = "AssetBundles";

	[MenuItem ("Assets/Export Asset Bundles", false, 500)]
	static void BuildAllAssetBundles(){
		AssetDatabase.RemoveUnusedAssetBundleNames();
		BuildPipeline.BuildAssetBundles(outputFolder);
	}

}