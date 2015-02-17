using UnityEngine;
using System.Collections;


public class GameEntityView: View<GameEntityModel> {


	// Visual update
	public override void Update(GameEntityModel model, float deltaTime){
		
		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index);
		if (obj == null) return; // can't work without a game object

		obj.transform.localScale = new Vector3(Mathf.Abs(obj.transform.localScale.x) * (model.isFacingRight ? 1 : -1), obj.transform.localScale.y, obj.transform.localScale.z);
		
	}
	
	public override bool IsCompatible(GameEntityModel originalModel, GameEntityModel newModel){
		// No local data stored so it's always compatible with any PhysicPointModel
		return true;
	}

}
