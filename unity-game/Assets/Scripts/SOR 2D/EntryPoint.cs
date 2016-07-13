using UnityEngine;
using System.Collections;
using RetroBread;


public class EntryPoint : MonoBehaviour
{

	static EntryPoint(){
		// Clear cach on startup, asset bundles may have been modified since last run
		Caching.CleanCache();
	}

	// Use this for initialization
	void Start(){
		RetroBread.Debug.Instance = new UnityDebug();
		SorVCFactories.RegisterFactories(true);
	}
	
	void LateUpdate(){
		StateManager.Instance.Update(Time.deltaTime);
	}
		
}

