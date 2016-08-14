using UnityEngine;
using System.Collections;
using RetroBread;


public class EntryPoint : MonoBehaviour
{

	// Use this for initialization
	void Start(){
		
		// Clear cach on startup, asset bundles may have been modified since last run
		Caching.CleanCache();
		// Framerate control
		Application.targetFrameRate = 60;

		RetroBread.Debug.Instance = new UnityDebug();
		SorVCFactories.RegisterFactories(true);

	}
	
	void LateUpdate(){
		StateManager.Instance.Update(Time.deltaTime);
	}
		
}

