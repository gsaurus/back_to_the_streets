using UnityEngine;
using System.Collections;
using RetroBread;


public class Entity2DView: GameEntityView {

	private void UpdateShadow(GameEntityModel model, GameObject entityObject){
		
		GameObject shadowObj = CharacterLoader.GetCharacterShadow(model, entityObject);
		PhysicPointModel pointModel = GameEntityController.GetPointModel(model);
		if (shadowObj == null || pointModel == null) return;

		Vector3 newShadowPosition;
		if (pointModel.collisionInpact.Y < 0) {
			newShadowPosition = pointModel.position.AsVector3();
		} else {
			// Raycast on physics
			WorldModel world = StateManager.state.MainModel as WorldModel;
			if (world == null)
				return;
			PhysicWorldModel physicsModel = StateManager.state.GetModel(world.physicsModelId) as PhysicWorldModel;
			if (physicsModel == null)
				return;
			PhysicWorldController physicsController = physicsModel.Controller() as PhysicWorldController;
			if (physicsController == null)
				return;

			PhysicPointModel entityPoint = GameEntityController.GetPointModel(model);
			FixedVector3 intersection = physicsController.Raycast(physicsModel, entityPoint.position, FixedVector3.Down);

			newShadowPosition = intersection.AsVector3();
		}
		// TODO: coordinates conversion from a setup file
		shadowObj.transform.position = PhysicPoint2DView.ConvertGameToViewCoordinates(newShadowPosition);
		shadowObj.transform.localPosition = new Vector3(0, shadowObj.transform.localPosition.y, 0.02f);
	}


	// Visual update
	protected override void Update(GameEntityModel model, float deltaTime){

		base.Update(model, deltaTime);

		// Post transformation into 2D
		if (model.parentEntity == null) {
			GameObject entityObject = UnityObjectsPool.Instance.GetGameObject(model.Index);
			if (entityObject == null)
				return; // can't work without a game object

			Vector3 originalRotation = entityObject.transform.eulerAngles;
			entityObject.transform.eulerAngles = new Vector3(90, originalRotation.y, originalRotation.z);

			// Shadow
			UpdateShadow(model, entityObject);
		}

	}
	
	public override bool IsCompatible(GameEntityModel originalModel, GameEntityModel newModel){
		// No local data stored so it's always compatible with any PhysicPointModel
		return true;
	}


}
