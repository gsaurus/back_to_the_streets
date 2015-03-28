using System;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlaneView: View<PhysicPlaneModel>{

	public override void Update(PhysicPlaneModel model, float deltaTime){
		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index, "moving_platform_prefab");
		obj.transform.position = (Vector3)model.origin + new Vector3(3f,-1.17f,0f);
	}


}

