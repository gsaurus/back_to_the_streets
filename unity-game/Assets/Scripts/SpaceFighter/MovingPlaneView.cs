using System;
using System.Collections.Generic;
using UnityEngine;
using RetroBread;

public class MovingPlaneView: View<PhysicPlaneModel>{

	protected override void Update(PhysicPlaneModel model, float deltaTime){
		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index, "moving_platform_prefab");
		if (obj == null){
			RetroBread.Debug.LogWarning("failed on getting view moving_platform_prefab");
			return;
		}
		obj.transform.position = model.origin.AsVector3() + new Vector3(3f,-1.17f,0f);
	}


}

