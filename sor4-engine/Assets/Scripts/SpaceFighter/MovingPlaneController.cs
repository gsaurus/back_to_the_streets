using System;
using System.Collections.Generic;
using RetroBread;


public class MovingPlaneController: PhysicPlaneController{

	private FixedVector3[] points;
	private static readonly FixedFloat deltaTime = FixedFloat.One / 200;


	public MovingPlaneController(FixedVector3[] points) {
		this.points = points;
	}
	


	// Update natural physics 
	public override void Update(PhysicPlaneModel model){
		base.Update(model);
		MovingPlaneModel movingModel = model as MovingPlaneModel;

		FixedVector3 pt1 = points[movingModel.movingState % points.Length];
		FixedVector3 pt2 = points[(movingModel.movingState +1) % points.Length];

		movingModel.blendFactor += deltaTime;
		if (movingModel.blendFactor >= 1){
			movingModel.origin = pt2;
			if (movingModel.blendFactor > 1.2){
				++movingModel.movingState;
				movingModel.blendFactor = 0;
			}
		}else {
			FixedFloat square = movingModel.blendFactor * movingModel.blendFactor * movingModel.blendFactor;
			FixedFloat oneMinusXSquared = (1 - movingModel.blendFactor)*(1 - movingModel.blendFactor) * (1 - movingModel.blendFactor);
			FixedFloat easedBlend = square / (square + oneMinusXSquared);
			movingModel.origin = FixedVector3.Lerp(pt1, pt2, easedBlend);
		}

	}

}

