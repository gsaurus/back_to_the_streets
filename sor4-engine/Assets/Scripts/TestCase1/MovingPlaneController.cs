using System;
using System.Collections.Generic;

public class MovingPlaneController: PhysicPlaneController{


	// Update natural physics 
	public override void Update(PhysicPlaneModel model){
		base.Update(model);
		MovingPlaneModel movingModel = model as MovingPlaneModel;
		switch (movingModel.movingState){
			case 0:
				// go up
				movingModel.origin.Y += 0.05;
				if (movingModel.origin.Y > 3){
					movingModel.movingState = 1;
				}
				break; 
			case 1:
				// go left
				movingModel.origin.X -= 0.05;
				if (movingModel.origin.X < 0){
					movingModel.movingState = 2;
				}
				break;
			case 2:
				// go down right
				if (movingModel.origin.X <= 4) movingModel.origin.X += 0.05;
				if (movingModel.origin.Y >= -3) movingModel.origin.Y -= 0.05;
				if (movingModel.origin.Y < -3 && movingModel.origin.X > 4){
					movingModel.movingState = 3;
				}
				break;
			case 3:
				// go up left
				if (movingModel.origin.X > 0) movingModel.origin.X -= 0.05;
				if (movingModel.origin.Y < 3) movingModel.origin.Y += 0.05;
				if (movingModel.origin.X <= 0 && movingModel.origin.Y >= 3){
					movingModel.movingState = 4;
				}
				break;
			case 4:
				// go right
				movingModel.origin.X += 0.05;
				if (movingModel.origin.X > 4){
					movingModel.movingState = 5;
				}
				break;
			case 5:
				// go up
				movingModel.origin.Y -= 0.05;
				if (movingModel.origin.Y <= -3){
					movingModel.movingState = 0;
				}
				break;
		}

	}

}

