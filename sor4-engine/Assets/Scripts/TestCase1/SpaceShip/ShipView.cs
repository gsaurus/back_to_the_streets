
using System;
using UnityEngine;


public class ShipView:View<ShipModel>{

	public ShipView(){

	}
	
	
	public override void Update(ShipModel model, float deltaTime){
		string prefabName;
		if (model.player == NetworkCenter.Instance.GetPlayerNumber()){
			prefabName = "Rocha";
		}else {
			prefabName = "Blaze";
		}
		// force creation of prefab, if not existing yet
		UnityObjectsPool.Instance.GetGameObject(model.Index, prefabName);
		
	}
	
	
	public override void OnDestroy(ShipModel model){
		UnityObjectsPool.Instance.ReleaseGameObject(model.Index);
	}




//	GameObject ship;
//
//	public ShipView(ShipModel model){
//		ship = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//		GameObject turret = GameObject.CreatePrimitive(PrimitiveType.Cube);
//		turret.transform.localScale = new Vector3(0.11f, 0.8f, 1.0f);
//		turret.transform.position = new Vector3(0f,0.3f, -0.2f);
//		turret.transform.SetParent(ship.transform);
//		if (model.player == NetworkCenter.Instance.GetPlayerNumber()){
//			ship.renderer.material.color = new Color(0,255,0);
//			turret.renderer.material.color = new Color(0,180,100);
//		}else {
//			ship.renderer.material.color = new Color(255,0,0);
//			turret.renderer.material.color = new Color(180,100,0);
//		}
//		if (model.player % 2 == 0) {
//			ship.transform.localScale = new Vector3(1.0f, -1.0f, 1.0f);
//		}
//	}
//
//
//	public override void Update(PhysicPointModel model, float deltaTime){
//
//		ship.transform.position = (Vector3)model.position;
//
//	}
//
//
//	public override void OnDestroy(){
//		GameObject.Destroy(ship);
//	}

	 

}

