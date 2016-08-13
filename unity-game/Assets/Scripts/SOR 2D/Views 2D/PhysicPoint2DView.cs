
using System;
using UnityEngine;
using System.Collections.Generic;



namespace RetroBread{



	// 
	public class PhysicPoint2DView:View<PhysicPointModel>{
		private float minInterpolationFactor = 0.005f;
		// below min and above max distance, it teleports
		private float minDistanceToInterpolate = 0.01f;	// TODO: based on world coordinates system..
		private float maxDistanceToInterpolate = 5.0f; 		// TODO: based on world coordinates system..		


		protected Vector2 AsPosition2D(Vector3 pos3D){
			return new Vector2(pos3D.x, pos3D.z + pos3D.y);
		}


		public static Vector3 ConvertGameToViewCoordinates(Vector3 coordinates){
			float forcedZ = ((int)(coordinates.z * 100)) / 100.0f;
			return new Vector3(coordinates.x, -100 * forcedZ, coordinates.y + coordinates.z);
		}


		protected void UpdateGameObjectPosition(GameObject obj, PhysicPointModel model, float deltaTime){

			// Convert model position to 2D
			Vector3 pos3D = model.position.AsVector3();
			Vector2 oldPos = AsPosition2D(model.lastPosition.AsVector3());
			Vector2 targetPos = AsPosition2D(model.position.AsVector3());
			Vector2 currentPos = new Vector2(obj.transform.localPosition.x, obj.transform.localPosition.z);
			Vector2 finalTarget;

			// Decide on interpolation based on the last position variation against current object position
			float oldDistance = Vector2.Distance(currentPos, oldPos);
			if (oldDistance < minDistanceToInterpolate || oldDistance > maxDistanceToInterpolate) {
				// Too close or too far away, just teleport
				finalTarget = targetPos;
			}else {
				// Something changed abruptaly, interpolate
				// TODO: better interpolation algorithm?
				//UnityEngine.Debug.Log("Interpolate: " + oldDistance);
				float interpolationFactor = 1f - (oldDistance-minDistanceToInterpolate) / (maxDistanceToInterpolate - minDistanceToInterpolate);
				interpolationFactor = Mathf.Pow(interpolationFactor, 3);
				if (interpolationFactor < minInterpolationFactor) interpolationFactor = minInterpolationFactor;
				finalTarget = Vector2.Lerp(currentPos, targetPos, interpolationFactor);
			}
			// handy tricks to avoid entities overlapping
			float forcedZ = ((int)(pos3D.z * 100)) / 100.0f;
			obj.transform.position = new Vector3(finalTarget.x, -100 * forcedZ + model.Index * 0.05f, finalTarget.y);
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

