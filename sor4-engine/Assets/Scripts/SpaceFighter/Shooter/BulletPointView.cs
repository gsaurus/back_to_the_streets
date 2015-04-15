using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;



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
		base.OnDestroy(model);

		BulletPointModel bulletModel = model as BulletPointModel;
		if (bulletModel == null) return;

		// Throw sparckles
		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index, "bullet");
		if (obj == null) return;

		if (bulletModel.lifetimeFrames < BulletPointController.totalBulletLifetimeFrames){
			Vector3 sparksPosition = (Vector3)model.position;
			sparksPosition += new Vector3(0f, UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f));
			GameObject.Instantiate(Resources.Load("sparks"), sparksPosition, Quaternion.identity);
		}

		UnityObjectsPool.Instance.ReleaseGameObject(model.Index);
	}

}

