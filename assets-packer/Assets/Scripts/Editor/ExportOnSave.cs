using UnityEngine;
using UnityEditor;
using System.Collections;

public class ExportOnSave:UnityEditor.AssetModificationProcessor {

	static string[] OnWillSaveAssets(string[] assets){
		// Before saving, export asset bundles
		Debug.Log("Go Save!");
		//AssetDatabase.RemoveUnusedAssetBundleNames();
		//BuildPipeline.BuildAssetBundles("AssetBundles");
		return assets;
	}

}
