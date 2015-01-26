
using System;
using UnityEngine;


public class BulletController:Controller<BulletModel>{



	public override void Update(BulletModel model){

		// Move
		model.position.Y += 0.3f * (model.player % 2 != 0 ? 1f : -1f);

	}




	private bool CheckCollisionWithShip(BulletModel model, ShipModel ship){
		return false;
		//FixedFloat half = 0.5f;
		//Debug.Log("Model: (" + model.x.ToFloat() + ", " + model.y.ToFloat() + "); ship: (" + ship.x.ToFloat() + ", " + ship.y.ToFloat() + ");");

		//return  FixedFloat.Sqrt((model.position.X - ship.position.X)*(model.position.X - ship.position.X) + (model.position.Y - ship.position.Y)*(model.position.Y - ship.position.Y)) <= half;
		//return model.x > ship.x-half && model.x < ship.x+half && model.y > ship.y-half && model.y < ship.y+half;
	}


	public override void PostUpdate(BulletModel model){

		// Detect collisions
		SpaceModel mainModel = StateManager.state.MainModel as SpaceModel;
		if (mainModel != null) {
			ShipModel ship;
			uint isPlayerUpOrDown = model.player % 2;
			foreach (uint shipId in mainModel.ships.Values){
				ship = StateManager.state.GetModel(shipId) as ShipModel;
				if (ship != null && ship.player % 2 != isPlayerUpOrDown){
					if (CheckCollisionWithShip(model, ship)){
						StateManager.state.RemoveModel(model);
						// TODO: hit ship, destroy it if necessary
						return;
					}
				}
			}
		}

		// Destroy if out of bounds
		if (model.position.Y < -5 || model.position.Y > 5) {
			StateManager.state.RemoveModel(model);
		}

	}
	
}

