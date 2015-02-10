
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

		GameEntityModel ownerModel = StateManager.state.GetModel(modelId) as GameEntityModel;

		// "guess" character name if necessary
		if (prefabName == null){
			if (ownerModel == null) return null;
			AnimationModel animModel = StateManager.state.GetModel(ownerModel.animationModelId) as AnimationModel;
			if (animModel == null) return null;
			prefabName = animModel.characterName;
		}

		if (prefabName == null || modelId == ModelReference.InvalidModelIndex){
			// Can't create it at the momment
			return null;
		}
		// Instantiate
		obj = GameObject.Instantiate(Resources.Load(prefabName)) as GameObject;
		gameObjects[modelId] = obj;
		if (parent != null){
			obj.transform.SetParent(parent);
		}

		// If there's a physics point model, translate the newly created obj to there
		if (ownerModel != null){
			PhysicPointModel pointModel = StateManager.state.GetModel(ownerModel.physicsModelId) as PhysicPointModel;
			if (pointModel != null){
				obj.transform.position = (Vector3) pointModel.position;
			}
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


