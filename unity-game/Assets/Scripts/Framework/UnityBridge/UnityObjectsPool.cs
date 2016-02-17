using System;
using UnityEngine;
using System.Collections.Generic;

namespace RetroBread{


	// Class to hold handy type conversions to/from unity
	public static class UnityConversions{
		
		public static UnityEngine.Vector3 AsVector3(this FixedVector3 src){
			return new UnityEngine.Vector3((float)src.X, (float)src.Y, (float)src.Z);
		}
		
		public static FixedVector3 AsFixedVetor3(this UnityEngine.Vector3 src){
			return new FixedVector3(src.x, src.y, src.z);
		}
	
	}


	// Holds the GameObjects used by views
	// GameObjects are associated to owner models
	// Owner models are responsible for releasing the object from the pool on destruction
	public class UnityObjectsPool: Singleton<UnityObjectsPool>{

		// GameObjects per owner id
		private Dictionary<uint, GameObject> gameObjects;

		private Dictionary<uint, string> prefabNamesByOwner;

		// Cache of loaded prefabs
		private Dictionary<string, UnityEngine.Object> prefabs;
		
		// Constructor
		public UnityObjectsPool(){
			gameObjects = new Dictionary<uint, GameObject>();
			prefabNamesByOwner = new Dictionary<uint, string>();
			prefabs = new Dictionary<string, UnityEngine.Object>();
		}


		public GameObject GetGameObject(uint modelId, string prefabName = null, Transform parent = null){

			// "guess" character name if necessary
			GameEntityModel ownerModel = StateManager.state.GetModel(modelId) as GameEntityModel;
			if (prefabName == null){
				if (ownerModel == null) return null;
				AnimationModel animModel = StateManager.state.GetModel(ownerModel.animationModelId) as AnimationModel;
				if (animModel == null) return null;
				if (animModel.viewModelName != null){
					prefabName = animModel.viewModelName;
				}else{
					prefabName = animModel.characterName;
				}
			}
			
			if (prefabName == null || modelId == ModelReference.InvalidModelIndex){
				// Can't create it at the momment
				return null;
			}

			// Try get from the pool
			GameObject obj;
			string originalPrefabName;
			if (gameObjects.TryGetValue(modelId, out obj)
			    && prefabNamesByOwner.TryGetValue(modelId, out originalPrefabName)
			    && originalPrefabName == prefabName
			){
				return obj;
			}

			// Doesn't exist yet

			// Instantiate it far, far away
			UnityEngine.Object prefab;
			if (!prefabs.TryGetValue(prefabName, out prefab)){
				prefab = Resources.Load(prefabName);
				prefabs.Add(prefabName, prefab);
			}
			obj = GameObject.Instantiate(prefab) as GameObject;
			obj.transform.position = new Vector3(float.MinValue,float.MaxValue, float.MinValue);
			gameObjects[modelId] = obj;
			prefabNamesByOwner[modelId] = prefabName;
			if (parent != null){
				obj.transform.SetParent(parent);
			}

			// If there's a physics point model, translate the newly created obj to there
			if (ownerModel != null){
				PhysicPointModel pointModel = StateManager.state.GetModel(ownerModel.physicsModelId) as PhysicPointModel;
				if (pointModel != null){
					obj.transform.position = pointModel.position.AsVector3();
				}
			}

			return obj;
		}

		public void ReleaseGameObject(uint modelId){
			// Destroy it if it exists on the pool
			GameObject obj;
			if (gameObjects.TryGetValue(modelId, out obj)){
				gameObjects.Remove(modelId);
				prefabNamesByOwner.Remove(modelId);
				GameObject.Destroy(obj);
			}
		}
	

		public void ReleaseUnusedResources(){
			Dictionary<string, UnityEngine.Object> newPrefabsDict = new Dictionary<string, UnityEngine.Object>();
			UnityEngine.Object obj;
			foreach(string prefabName in prefabNamesByOwner.Values){
				if (prefabs.TryGetValue(prefabName, out obj)){
					newPrefabsDict.Add(prefabName, obj);
				}
			}
			prefabs = newPrefabsDict;
			Resources.UnloadUnusedAssets();
		}

		public void ReleaseAll(){
			foreach(GameObject obj in gameObjects.Values){
				GameObject.Destroy(obj);
			}
			gameObjects.Clear();
			prefabNamesByOwner.Clear();
			prefabs.Clear();
			Resources.UnloadUnusedAssets();
		}

	}

}


