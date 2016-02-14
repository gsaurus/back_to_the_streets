using System;
using System.Collections.Generic;
using UnityEngine;
using RetroBread;

public class MovingPlaneView: View<PhysicPlaneModel>{

	protected override void Update(PhysicPlaneModel model, float deltaTime){
		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index, "moving_platform_prefab");
		obj.transform.position = model.origin.AsVector3() + new Vector3(3f,-1.17f,0f);
	}


}

