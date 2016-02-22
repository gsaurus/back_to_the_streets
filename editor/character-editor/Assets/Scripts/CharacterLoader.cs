using UnityEngine;
using System.Collections;

public class CharacterLoader : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Caching.CleanCache();
		string url = "file://" + Application.streamingAssetsPath + "/aBundle"; //"/characters/rocha";
		Debug.Log(url);
		WWW www = WWW.LoadFromCacheOrDownload(url, 1);
		if (www == null || www.assetBundle == null) return;
		AssetBundle bundle = www.assetBundle;

		foreach (string name in bundle.GetAllAssetNames()){
			Debug.Log(name);
		}


		GameObject prefab = bundle.LoadAsset("prefab") as GameObject;
//		if (prefab == null){
//			Debug.Log("fail one");
//			prefab = bundle.LoadAsset("assets/rocha/prefab.prefab") as GameObject;
//			if (prefab == null){
//				Debug.Log("Terrible!");
//			}
//		}
//		Debug.Log("Prefab loaded!! " + prefab.name);
		Animator animator = prefab.GetComponent<Animator>();
//		if (animator != null){
//			Debug.Log("animator!");
//		}
		RuntimeAnimatorController controller = animator.runtimeAnimatorController;
		if (controller != null){
			Debug.Log("Yay");
			foreach(AnimationClip clip in controller.animationClips){
				Debug.Log("clip " + clip.name);
			}

		}
		//RuntimeAnimatorController animator = prefab.GetComponent<RuntimeAnimatorController>();

		GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);

		prefab = bundle.LoadAsset("a_simple_sprite") as GameObject;
		GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);

	}
	
	// Update is called once per frame
	void Update() {
	
	}
}
