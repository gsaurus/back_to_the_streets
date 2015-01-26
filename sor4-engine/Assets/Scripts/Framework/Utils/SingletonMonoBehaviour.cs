using UnityEngine;
using System;
using System.Collections;

// Singleton pattern for MonoBehaviours, persistent across scenes
public class SingletonMonoBehaviour<T>: MonoBehaviour where T : MonoBehaviour{
	
	private static T instance;
	
	public static T Instance {
		get {
			if (instance == null)	{			
				instance = (T)MonoBehaviour.FindObjectOfType(typeof(T));
				if (instance == null){
					GameObject container = new GameObject();
					container.name = "__" + typeof(T).ToString();
					instance = container.AddComponent<T>();
				}
				MonoBehaviour.DontDestroyOnLoad(instance.gameObject);
			}
			return instance;
		}
	}

}

