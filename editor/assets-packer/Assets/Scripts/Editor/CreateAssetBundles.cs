using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundles{

//#if UNITY_IPHONE
//	static string platform = "iOS";
//#elif UNITY_ANDROID
//	static string platform = "android";
//#else
//	static string platform = "pc";
//#endif

	static string outputFolder = "AssetBundles";
	static string tempFolder = "/__temp_clip_info__/";

	[MenuItem ("Assets/Export Asset Bundles", false, 500)]
	static void BuildAllAssetBundles(){
		GenerateClipLengths();
		AssetDatabase.Refresh();
		AssetDatabase.RemoveUnusedAssetBundleNames();
		BuildPipeline.BuildAssetBundles(outputFolder, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSXUniversal);
		ClearTempFiles();
	}


	// WARNING: currently clip.averageDuration is not accessible at runtime, outside editor
	// So we pre-compute clip lengths and store them in a bundled text file
	private static void GenerateClipLengths(){

		// Find all prefabs
		GameObject[] prefabs = Resources.FindObjectsOfTypeAll<GameObject>();

		// Iterate prefabs and process those within a bundle and having an Animator component
		Animator animator;
		RuntimeAnimatorController controller;
		AssetImporter importer;
		string assetPath;
		string assetBundleName;
		string infoPath;
		foreach(GameObject prefab in prefabs){
			assetPath = AssetDatabase.GetAssetPath(prefab);
			// check if it's a prefab root
			if (string.IsNullOrEmpty(assetPath) || PrefabUtility.GetPrefabType(prefab) != PrefabType.Prefab || PrefabUtility.FindPrefabRoot(prefab) != prefab) continue; // not a prefab root..
			importer = AssetImporter.GetAtPath(assetPath);
			if (importer == null) continue; // nothing that can be imported..
			assetBundleName = importer.assetBundleName;
			animator = prefab.GetComponent<Animator>();
			controller = null;
			if (animator != null) controller = animator.runtimeAnimatorController;
			if (controller != null && assetBundleName != null){
				
				// Create a file on this bundle using the prefab name
				infoPath = tempFolder + prefab.name + " clipInfo.bytes";
				FileInfo file = new FileInfo(Application.dataPath + infoPath);
				file.Directory.Create();
				FileStream fileSteam = file.Open(FileMode.Create, FileAccess.Write);
				BinaryWriter writer = new BinaryWriter(fileSteam);
				// num clips
				writer.Write((uint)controller.animationClips.Length);
				// write each clip name and size:
				foreach(AnimationClip clip in controller.animationClips){
					writer.Write(clip.name);
					writer.Write(Mathf.CeilToInt(clip.averageDuration / Time.fixedDeltaTime));
				}
				writer.Close();

				// Update assets database
				infoPath = "Assets" + infoPath;
         		AssetDatabase.Refresh();

         		// Set bundle for info file
				importer = AssetImporter.GetAtPath(infoPath);
				importer.assetBundleName = assetBundleName;
			}
		}

	}

	private static void ClearTempFiles(){
		// Clear temporary data
		FileInfo file = new FileInfo(Application.dataPath + tempFolder);
		if (file.Directory.Exists){
			Directory.Delete(file.Directory.FullName, true);
		}
		AssetDatabase.Refresh();
	}

}