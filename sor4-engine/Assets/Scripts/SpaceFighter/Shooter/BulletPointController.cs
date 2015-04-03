using System;
using System.Collections.Generic;

public class BulletPointController: PhysicPointController{
	
	public const uint totalBulletLifetimeFrames = 180;


	// Update natural physics 
	public override void Update(PhysicPointModel model){
		base.Update(model);

		BulletPointModel bulletModel = model as BulletPointModel;
		if (bulletModel == null) return;

		// TODO: check collisions against players

		if (++bulletModel.lifetimeFrames > totalBulletLifetimeFrames){
			// This bullet went too far
			UnityEngine.Debug.Log("kill bullet by lifetime");
			StateManager.state.RemoveModel(bulletModel);
		}

	}
	


	// Collision reaction.
	public override bool OnCollision(PhysicWorldModel world, PhysicPointModel pointModel, PhysicPlaneModel planeModel, FixedVector3 intersection){
		// On Collision, kill bullet
		UnityEngine.Debug.Log("bullet collision");
		StateManager.state.RemoveModel(pointModel);
		return true;
	}


}

