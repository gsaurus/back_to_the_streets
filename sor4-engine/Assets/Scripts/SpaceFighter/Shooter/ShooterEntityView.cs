using UnityEngine;
using System.Collections;


public class ShooterEntityView: GameEntityView {


	// Visual update
	public override void Update(GameEntityModel model, float deltaTime){

		base.Update(model, deltaTime);

		ShooterEntityModel shooterModel = model as ShooterEntityModel;
		if (shooterModel == null) return;

		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index);

		if (shooterModel.invincibilityFrames > 0){
			// let game object look like a ghost

		}

		// TODO: update hud stuff
		
	}
	
	public override bool IsCompatible(GameEntityModel originalModel, GameEntityModel newModel){
		// No local data stored so it's always compatible with any PhysicPointModel
		return true;
	}

}
