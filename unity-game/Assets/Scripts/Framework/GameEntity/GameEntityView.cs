using UnityEngine;
using System.Collections;



namespace RetroBread{


	public class GameEntityView: View<GameEntityModel> {


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

	}



}