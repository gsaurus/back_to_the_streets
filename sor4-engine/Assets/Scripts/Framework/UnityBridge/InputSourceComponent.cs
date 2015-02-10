using UnityEngine;
using System.Collections;

public class InputSourceComponent : MonoBehaviour{

	void Awake(){

#if UNITY_IPHONE || UNITY_ANDROID
		gameObject.AddComponent<TouchScreenInputSource>();
		// TODO: setup...
#else
		gameObject.AddComponent<KeyboardInputSource>();
#endif
		
	}
	
}

