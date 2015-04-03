
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

	}
	
	public override void OnDestroy (PhysicPointModel model)
	{
		base.OnDestroy (model);
		// TODO: throw sparckles
		UnityObjectsPool.Instance.ReleaseGameObject(model.Index);
	}

}

