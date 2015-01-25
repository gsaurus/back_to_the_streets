
using System;
using UnityEngine;
using System.Collections.Generic;



// Holds the GameObjects used by views
// GameObjects are associated to owner models
// Owner models are responsible for releasing the object from the pool on destruction
public class UnityObjectsPool: Singleton<UnityObjectsPool>{

	// GameObjects per owner id
	private Dictionary<uint, GameObject> gameObjects;
	
	// Constructor
	public UnityObjectsPool(){
		gameObjects = new Dictionary<uint, GameObject>();
	}


	public GameObject GetGameObject(uint modelId, string prefabName = null, Transform parent = null){
		// Try get from the pool
		GameObject obj;
		if (gameObjects.TryGetValue(modelId, out obj)){
			return obj;
		}
		// Doesn't exist
		if (prefabName == null){
			// Can't create it at the momment
			return null;
		}
		// Instantiate
		obj = GameObject.Instantiate(Resources.Load(prefabName)) as GameObject;
		gameObjects[modelId] = obj;
		if (parent != null){
			obj.transform.SetParent(parent);
		}
		return obj;
	}

	public void ReleaseGameObject(uint modelId){
		// Destroy it if it exists on the pool
		GameObject obj;
		if (gameObjects.TryGetValue(modelId, out obj)){
			gameObjects.Remove(modelId);
			GameObject.Destroy(obj);
		}
	}

}


