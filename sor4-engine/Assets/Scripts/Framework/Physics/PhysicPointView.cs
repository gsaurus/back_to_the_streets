
using System;
using UnityEngine;
using System.Collections.Generic;




// 
public class PhysicPointView:View<PhysicPointModel>{
	private float minInterpolationFactor = 0.1f;
	private float minDistanceToInterpolate = 0.1f;	// TODO: based on world coordinates system..
	private float maxDistanceToInterpolate = 5.0f; 	// TODO: based on world coordinates system..		

	// Visual update
	public override void Update(PhysicPointModel model, float deltaTime){

		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.ownerId);
		if (obj == null) return; // can't work without a game object
		Transform transform = obj.transform;

		// Decide on interpolation based on the last position variation against current object position
		float oldDistance = Vector3.Distance(transform.position, model.lastPosition);
		if (oldDistance < minDistanceToInterpolate || oldDistance > maxDistanceToInterpolate) {
			// Too close or too far away, just teleport
			transform.position = model.position;
		}else {
			// Something changed abruptaly, interpolate
			// TODO: better interpolation algorithm?
			float interpolationFactor = 1f - (oldDistance-minDistanceToInterpolate) / (maxDistanceToInterpolate - minDistanceToInterpolate);
			interpolationFactor = Mathf.Pow(interpolationFactor, 3);
			if (interpolationFactor < minInterpolationFactor) interpolationFactor = minInterpolationFactor;
			transform.position = Vector3.Lerp(transform.position, model.position, interpolationFactor);
		}

	}

	public override bool IsCompatible(PhysicPointModel originalModel, PhysicPointModel newModel){
		// No local data stored so it's always compatible with any PhysicPointModel
		return true;
	}

}

