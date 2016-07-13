using System;
using UnityEngine;
using System.Collections.Generic;

namespace RetroBread{


	// Class to hold handy type conversions to/from unity
	public static class UnityExtensions{
		
		public static UnityEngine.Vector3 AsVector3(this FixedVector3 src){
			return new UnityEngine.Vector3((float)src.X, (float)src.Y, (float)src.Z);
		}
		
		public static FixedVector3 AsFixedVetor3(this UnityEngine.Vector3 src){
			return new FixedVector3(src.x, src.y, src.z);
		}

		//Depth-first search
	     public static Transform FindDeepChild(this Transform aParent, string aName){
	         foreach(Transform child in aParent){
	             if(child.name == aName )
	                 return child;
	             var result = child.FindDeepChild(aName);
	             if (result != null)
	                 return result;
	         }
	         return null;
	     }
	
	}



	// Holds the GameObjects used by views
	// GameObjects are associated to owner models
	// Owner models are responsible for releasing the object from the pool on destruction
	public class UnityObjectsPool: Singleton<UnityObjectsPool>{

		// Class to hold GameObject, and cache it's internal anchor objects
		private class GameObjectData{
			public GameObject obj;
			public List<GameObject> anchors;

			// Constructor given an object and list of anchor names.
			// Try to find anchors by child name in the given obj, and cache them
			public GameObjectData(GameObject obj, List<string> anchorNames){
				this.obj = obj;
				anchors = new List<GameObject>(anchorNames != null ? anchorNames.Count : 0);
				Transform childsTransform;
				GameObject childsGameObject;
				if (anchorNames != null){
					foreach (string anchorName in anchorNames){
						childsTransform = obj.transform.FindDeepChild(anchorName);
						childsGameObject = childsTransform != null ? childsTransform.gameObject : null;
						anchors.Add(childsGameObject);
					}
				}
			}

		}



		// GameObjects per owner id
		private Dictionary<uint, GameObjectData> gameObjects;

		private Dictionary<uint, string> prefabNamesByOwner;

		// Cache of loaded prefabs
		private Dictionary<string, UnityEngine.Object> prefabs;

		
		// Constructor
		public UnityObjectsPool(){
			gameObjects = new Dictionary<uint, GameObjectData>();
			prefabNamesByOwner = new Dictionary<uint, string>();
			prefabs = new Dictionary<string, UnityEngine.Object>();
		}


		public GameObject GetGameObject(uint modelId, string prefabName = null, Transform parent = null){

			// "guess" character name if necessary
			GameEntityModel ownerModel = StateManager.state.GetModel(modelId) as GameEntityModel;
			AnimationModel animModel = null;
			if (ownerModel != null){
				animModel = StateManager.state.GetModel(ownerModel.animationModelId) as AnimationModel;
			}
			if (prefabName == null){
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
			GameObjectData objData;
			string originalPrefabName;
			if (gameObjects.TryGetValue(modelId, out objData)
			    && prefabNamesByOwner.TryGetValue(modelId, out originalPrefabName)
			    && originalPrefabName == prefabName
			){
				return objData.obj;
			}

			// Doesn't exist yet

			string characterName = null;
			if (animModel != null){
				characterName = animModel.characterName;
			}

			// Instantiate it far, far away
			UnityEngine.Object prefab;
			if (!prefabs.TryGetValue(prefabName, out prefab)){
				prefab = CharacterLoader.LoadViewModel(characterName, prefabName);
				if (prefab == null) {
					Debug.LogError("Failed to load prefab: " + prefabName);
				}
				prefabs.Add(prefabName, prefab);
			}
			GameObject obj = GameObject.Instantiate(prefab) as GameObject;
			obj.transform.position = new Vector3(float.MinValue,float.MaxValue, float.MinValue);
			if (animModel != null){
				gameObjects[modelId] = new GameObjectData(obj, CharacterLoader.GetCharacterAnchorNames(animModel.characterName));
			}else{
				gameObjects[modelId] = new GameObjectData(obj, null);
			}
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


		public GameObject GetAnchorObject(uint modelId, int anchorId){
			GameObjectData objData;
			if (gameObjects.TryGetValue(modelId, out objData)){
				if (objData != null && objData.anchors != null && anchorId < objData.anchors.Count){
					return objData.anchors[anchorId];
				}
			}
			return null;
		}

		public void ReleaseGameObject(uint modelId){
			// Destroy it if it exists on the pool
			GameObjectData objData;
			if (gameObjects.TryGetValue(modelId, out objData)){
				gameObjects.Remove(modelId);
				prefabNamesByOwner.Remove(modelId);
				GameObject.Destroy(objData.obj);
			}
		}
	

		public void ReleaseUnusedResources(){
			Dictionary<string, UnityEngine.Object> newPrefabsDict = new Dictionary<string, UnityEngine.Object>();
			UnityEngine.Object obj;
			List<string> anchorNames;
			foreach(string prefabName in prefabNamesByOwner.Values){
				if (prefabs.TryGetValue(prefabName, out obj)){
					newPrefabsDict.Add(prefabName, obj);
				}
			}
			prefabs = newPrefabsDict;
			Resources.UnloadUnusedAssets();
		}

		public void ReleaseAll(){
			foreach(GameObjectData objData in gameObjects.Values){
				GameObject.Destroy(objData.obj);
			}
			gameObjects.Clear();
			prefabNamesByOwner.Clear();
			prefabs.Clear();
		}

	}

}


