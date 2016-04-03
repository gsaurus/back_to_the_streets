using UnityEngine;
using System.Collections;



namespace RetroBread{


	public class GameEntityView: View<GameEntityModel> {


		// Visual update
		protected override void Update(GameEntityModel model, float deltaTime){
			
			GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index);
			if (obj == null) return; // can't work without a game object

			// TODO: flip that works for all rotation angles
			if (obj.transform.rotation.y != 0) {
				obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y, Mathf.Abs(obj.transform.localScale.z) * (model.isFacingRight ? 1 : -1));
			}else {
				obj.transform.localScale = new Vector3(Mathf.Abs(obj.transform.localScale.x) * (model.isFacingRight ? 1 : -1), obj.transform.localScale.y, obj.transform.localScale.z);
			}

//			// Grab / ungrab
//			if (model.parentEntity != null && model.parentEntity != ModelReference.InvalidModelIndex){
//				GameEntityModel parentModel
//				GameObject parentObj = UnityObjectsPool.Instance.GetGameObject(model.parentEntity);
//				if (parentObj != null){
//					Transform anchorTransform = parentObj.transform.FindChild(
//					obj.transform.SetParent(parentObj);
//				}
//			}
//			else if (obj.transform.parent != null){
//				obj.transform.SetParent(null);
//			}
			
		}
		
		public override bool IsCompatible(GameEntityModel originalModel, GameEntityModel newModel){
			// No local data stored so it's always compatible with any PhysicPointModel
			return true;
		}

	}



}