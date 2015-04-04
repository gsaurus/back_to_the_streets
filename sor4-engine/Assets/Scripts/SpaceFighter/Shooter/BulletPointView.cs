
using System;
using UnityEngine;
using System.Collections.Generic;




// 
public class BulletPointView:PhysicPointView{

	// Visual update
	public override void Update(PhysicPointModel model, float deltaTime){

		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index, "bullet");
		if (obj == null) return; // can't work without a game object

		UpdateGameObjectPosition(obj, model, deltaTime);

		BulletPointModel bulletModel = model as BulletPointModel;
		if (bulletModel == null) return;

		float scaleX = bulletModel.lifetimeFrames*2f;
		if (scaleX > 3) scaleX = 3;
		obj.transform.localScale = new Vector3(scaleX, obj.transform.localScale.y, obj.transform.localScale.z);

	}
	
	public override void OnDestroy (PhysicPointModel model)
	{
		base.OnDestroy (model);
		// TODO: throw sparckles
		UnityObjectsPool.Instance.ReleaseGameObject(model.Index);
	}

}

