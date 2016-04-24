using UnityEngine;
using System.Collections;
using RetroBread;


public class Entity2DView: GameEntityView {


	// Visual update
	protected override void Update(GameEntityModel model, float deltaTime){

		base.Update(model, deltaTime);

		// Post transformation into 2D

		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index);
		if (obj == null) return; // can't work without a game object

		Vector3 originalRotation = obj.transform.eulerAngles;
		obj.transform.eulerAngles = new Vector3(90, originalRotation.y, originalRotation.z);
	}
	
	public override bool IsCompatible(GameEntityModel originalModel, GameEntityModel newModel){
		// No local data stored so it's always compatible with any PhysicPointModel
		return true;
	}


}
