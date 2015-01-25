
using System;
using UnityEngine;


public class BulletView:View<BulletModel>{

	GameObject cube;

	public BulletView(BulletModel model){
		cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.localScale = new Vector3(0.1f, 0.4f, 1.0f);
		if (model.player == NetworkCenter.Instance.GetPlayerNumber()){
			cube.renderer.material.color = Color.green;
		}else{
			cube.renderer.material.color = Color.yellow;
		}
	}


	public override void Update(BulletModel model, float deltaTime){

		cube.transform.position = (Vector3) model.position;

	}

	public override void OnDestroy(BulletModel model){
		GameObject.Destroy(cube);
	}

}

