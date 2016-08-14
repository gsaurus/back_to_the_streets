using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace RetroBread{


	public class GameEntityView: View<GameEntityModel> {

		public delegate Vector3 ConvertGameToViewCoordinates(Vector3 coordinates);


		// Visual update
		protected override void Update(GameEntityModel model, float deltaTime){
			
			GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index);
			if (obj == null) return; // can't work without a game object

			// be Grabbed / ungrabbed
			if (model.parentEntity != null && model.parentEntity != ModelReference.InvalidModelIndex){
				// setup parent
				if (obj.transform.parent == null){
					GameObject parentObj = UnityObjectsPool.Instance.GetGameObject(model.parentEntity);
					if (parentObj != null){
						GameEntityModel parentModel = StateManager.state.GetModel(model.parentEntity) as GameEntityModel;
						if (parentModel != null) {
							for (int anchorId = 0 ; anchorId < parentModel.anchoredEntities.Count ; ++anchorId){
								if (parentModel.anchoredEntities[anchorId] == model.Index){
									GameObject parentAnchorObj = UnityObjectsPool.Instance.GetAnchorObject(model.parentEntity, anchorId);
									if (parentAnchorObj != null){
										obj.transform.SetParent(parentAnchorObj.transform);
										obj.transform.rotation = Quaternion.identity;
										obj.transform.localScale = Vector3.one;
									}
								}
							}
						}
					}
				}
				// No need to setup relative position, that's logical only, and follow the view's position (if done correctly)
			}
			else if (obj.transform.parent != null){
				obj.transform.SetParent(null);
				obj.transform.rotation = Quaternion.identity;
				obj.transform.localScale = Vector3.one;
			}

			// TODO: flip that works for all rotation angles
			if (obj.transform.rotation.y != 0) {
				obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y, Mathf.Abs(obj.transform.localScale.z) * (model.isFacingRight ? 1 : -1));
			}else {
				obj.transform.localScale = new Vector3(Mathf.Abs(obj.transform.localScale.x) * (model.isFacingRight ? 1 : -1), obj.transform.localScale.y, obj.transform.localScale.z);
			}
			
		}
		
		public override bool IsCompatible(GameEntityModel originalModel, GameEntityModel newModel){
			// No local data stored so it's always compatible
			return true;
		}






		public static void SpawnAtSelf(GameEntityModel model, string prefabName, int lifetime, FixedVector3 offset, bool localSpace, ConvertGameToViewCoordinates gameToViewCoordinates){

			// No visual spawns if state is being remade
			if (StateManager.Instance.IsRewindingState) return;

			GameObject obj = UnityObjectsPool.Instance.FireAndForget(model, prefabName, lifetime);
			GameObject selfObj = UnityObjectsPool.Instance.GetGameObject(model.Index);
			if (obj != null && selfObj != null) {
				if (localSpace) {
					obj.transform.SetParent(selfObj.transform);
					obj.transform.localPosition = gameToViewCoordinates(offset.AsVector3());
				} else {
					obj.transform.position = selfObj.transform.position + gameToViewCoordinates(offset.AsVector3());
				}

			}
		}


		private static void SpawnAtIntersection(List<HitInformation> hits, GameEntityModel model, string prefabName, int lifetime, FixedVector3 offset, bool localSpace, ConvertGameToViewCoordinates gameToViewCoordinates){

			GameObject selfObj = UnityObjectsPool.Instance.GetGameObject(model.Index);

			// For each hit, spawn randomly within the intersection box
			bool spawnAtLeft;
			float randomValue;
			Vector3 spawnLocation;
			foreach (HitInformation info in hits) {
				GameEntityModel otherModel = StateManager.state.GetModel(info.entityId) as GameEntityModel;
				if (otherModel != null) {
					spawnAtLeft = !otherModel.isFacingRight;
					randomValue = UnityEngine.Random.Range(0f, 1f);
					randomValue = randomValue * randomValue * randomValue;
					if (spawnAtLeft) {
						randomValue = 1 - randomValue;
					}
					// Get a suitable location to spawn
					spawnLocation.x = (float)(info.intersection.pointOne.X + (info.intersection.pointTwo.X - info.intersection.pointOne.X) * randomValue);
					randomValue = UnityEngine.Random.Range(0f, 1f);
					randomValue = 1 - (randomValue * randomValue);
					randomValue *= UnityEngine.Random.Range(0, 2) == 0 ? 0.5f : -0.5f;
					spawnLocation.y = (float)(info.intersection.Center().Y + (info.intersection.pointTwo.Y - info.intersection.pointOne.Y) * randomValue);
					float z1 = (float)GameEntityController.GetPointModel(model).position.Z;
					float z2 = (float)GameEntityController.GetPointModel(otherModel).position.Z;
					spawnLocation.z = Mathf.Min(z1, z2) - 0.1f;

					// Spawn the object
					GameObject obj = UnityObjectsPool.Instance.FireAndForget(model, prefabName, lifetime);
					if (obj != null) {
						spawnLocation += offset.AsVector3();
						obj.transform.position = gameToViewCoordinates(spawnLocation);
						if (localSpace) {
							obj.transform.SetParent(selfObj.transform);
						}
					}
				}
			}

		}

		public static void SpawnAtHitIntersection(GameEntityModel model, string prefabName, int lifetime, FixedVector3 offset, bool localSpace, ConvertGameToViewCoordinates gameToViewCoordinates){

			// No visual spawns if state is being remade
			if (StateManager.Instance.IsRewindingState) return;

			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller.lastHits.Count == 0) return;
			SpawnAtIntersection(controller.lastHits, model, prefabName, lifetime, offset, localSpace, gameToViewCoordinates); 
		}


		public static void SpawnAtHurtIntersection(GameEntityModel model, string prefabName, int lifetime, FixedVector3 offset, bool localSpace, ConvertGameToViewCoordinates gameToViewCoordinates){

			// No visual spawns if state is being remade
			if (StateManager.Instance.IsRewindingState) return;

			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller.lastHurts.Count == 0) return;
			SpawnAtIntersection(controller.lastHurts, model, prefabName, lifetime, offset, localSpace, gameToViewCoordinates); 
		}


	}



}