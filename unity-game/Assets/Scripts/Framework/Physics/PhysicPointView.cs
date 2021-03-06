
using System;
using UnityEngine;
using System.Collections.Generic;



namespace RetroBread{



	// 
	public class PhysicPointView:View<PhysicPointModel>{
		private float minInterpolationFactor = 0.005f;
		// below min and above max distance, it teleports
		private float minDistanceToInterpolate = 0.01f;	// TODO: based on world coordinates system..
		private float maxDistanceToInterpolate = 5.0f; 		// TODO: based on world coordinates system..		



		protected void UpdateGameObjectPosition(GameObject obj, PhysicPointModel model, float deltaTime){
			// Decide on interpolation based on the last position variation against current object position
			float oldDistance = Vector3.Distance(obj.transform.position, model.lastPosition.AsVector3());
			if (oldDistance < minDistanceToInterpolate || oldDistance > maxDistanceToInterpolate) {
				// Too close or too far away, just teleport
				obj.transform.position = model.position.AsVector3();
			}else {
				// Something changed abruptaly, interpolate
				// TODO: better interpolation algorithm?
				//UnityEngine.Debug.Log("Interpolate: " + oldDistance);
				float interpolationFactor = 1f - (oldDistance-minDistanceToInterpolate) / (maxDistanceToInterpolate - minDistanceToInterpolate);
				interpolationFactor = Mathf.Pow(interpolationFactor, 3);
				if (interpolationFactor < minInterpolationFactor) interpolationFactor = minInterpolationFactor;
				obj.transform.position = Vector3.Lerp(obj.transform.position, model.position.AsVector3(), interpolationFactor);
			}
		}


		// Visual update
		protected override void Update(PhysicPointModel model, float deltaTime){

			GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.ownerId);
			if (obj == null) return; // can't work without a game object
			if (obj.transform.parent != null){
				// not a root object, parent will update it
				return;
			}

			UpdateGameObjectPosition(obj, model, deltaTime);

		}

		public override bool IsCompatible(PhysicPointModel originalModel, PhysicPointModel newModel){
			// No local data stored so it's always compatible with any PhysicPointModel
			return true;
		}

	}


}

