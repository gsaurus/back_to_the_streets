using System;
using System.Collections.Generic;


namespace RetroBread{


	public class PhysicPlaneController: Controller<PhysicPlaneModel>{

		// TODO: set position, which also updates the oldPosition..
		private FixedVector3 newPosition;
		private bool hasNewPosition;

		// Update natural physics 
		protected override void Update(PhysicPlaneModel model){
			model.lastOriginPosition = model.origin;
		}

		// Post update used to consolidate direct modifications
		protected override void PostUpdate(PhysicPlaneModel model){
			// New position (teleport)
			if (hasNewPosition){
				model.origin = newPosition;
				model.lastOriginPosition = newPosition;
				hasNewPosition = false;
			}
		}

		// Teleport to a new position
		public void SetPosition(PhysicPlaneModel model, FixedVector3 newPos){
			if (StateManager.state.IsPostUpdating){
				model.lastOriginPosition = model.origin = newPos;
				hasNewPosition = false;
			}else{
				newPosition = newPos;
				hasNewPosition = true;
			}
		}




		protected static bool BoxIntersection(PhysicPlaneModel planeModel, FixedVector3 point1, FixedVector3 point2){
			FixedVector3 minPoint = FixedVector3.Min(point1, point2);
			FixedVector3 maxPoint = FixedVector3.Max(point1, point2);
			minPoint-= planeModel.origin;
			maxPoint-= planeModel.origin;
			return !(
				   minPoint.X > planeModel.maxPos.X
				|| maxPoint.X < planeModel.minPos.X
				|| minPoint.Y > planeModel.maxPos.Y
				|| maxPoint.Y < planeModel.minPos.Y
				|| minPoint.Z > planeModel.maxPos.Z
				|| maxPoint.Z < planeModel.minPos.Z
			);
		}


		// Compute the intersection point against a line segment
		public static bool CheckIntersection(PhysicPlaneModel planeModel, PhysicPointModel pointModel, out FixedVector3 intersection){
			// plane may be moving, sum velocity to initial point position
			FixedVector3 pos1 = pointModel.lastPosition;
			FixedVector3 pos2 = pointModel.position;
			pos1 += planeModel.GetVelocity();

			// Check bounding box intersection, including step tolerance
			if (!BoxIntersection(planeModel, pos1 + pointModel.stepTolerance, pos2)){
				intersection = FixedVector3.Zero;
				return false;
			}

			// check collision with the hiperplane
			FixedVector3 pointDeltaPos = pos2 - pos1;
			if (pointDeltaPos.Magnitude == 0){
				// The point is not moving relatively to the plane
				intersection = FixedVector3.Zero;
				return false;
			}
			FixedVector3 pos1ToOrigin = planeModel.origin - pos1;
			FixedFloat dotDeltaPosNormal = FixedVector3.Dot(pointDeltaPos, planeModel.normal);
			if (dotDeltaPosNormal >= 0){
				// Point moving away from the plane
				intersection = FixedVector3.Zero;
				return false;
			}

			// Find intersection location in the deltapos vector
			FixedFloat t = FixedVector3.Dot(pos1ToOrigin, planeModel.normal) / dotDeltaPosNormal;

			// a small delta due to precision errors
			// based on deltaPos magnitude (the smaller the magnitude the higher the error)
			FixedFloat error = 0.01 / pointDeltaPos.Magnitude;

			if (t < -error){
				// falling through the plane, try step tolerance to recover
				pos1 += pointModel.stepTolerance;
				pointDeltaPos = pos2 - pos1;
				pos1ToOrigin = planeModel.origin - pos1;
				dotDeltaPosNormal = FixedVector3.Dot(pointDeltaPos, planeModel.normal);
				t = FixedVector3.Dot(pos1ToOrigin, planeModel.normal) / dotDeltaPosNormal;
				error = 0.01 / pointDeltaPos.Magnitude;
			}
			// give some tolerance
			if (t < -error || t > 1 + error) {
				// not colliding
				intersection = FixedVector3.Zero;
				return false;
			}
			intersection = pos1 + t * pointDeltaPos;

			// Check if intersection point is inside the plane
			FixedFloat anglesSum = FixedFloat.Zero;
			FixedVector3 originVector = planeModel.origin - intersection;
			FixedVector3 vec1 = originVector;
			FixedVector3 vec2 = FixedVector3.Zero;
			FixedVector3 vertex;
			for (int i = 0 ; i < planeModel.offsets.Count ; ++i){
				vertex = planeModel.GetPointFromOffsetId(i);
				vec2 = vertex - intersection;
				anglesSum += FixedVector3.Angle(vec1, vec2);
				vec1 = vec2;
			}
			// last vertex with origin
			anglesSum += FixedVector3.Angle(vec2, originVector);

			// a small delta due to precision errors
			return FixedFloat.Abs(anglesSum - FixedFloat.TwoPI) < 0.2;
		}




		// Collision reaction, return true if collision is considered stable (no position modifications occured)
		public virtual bool OnCollision(PhysicWorldModel world, PhysicPointModel pointModel, PhysicPlaneModel planeModel, FixedVector3 intersection){
			// Nothing by default
			return true;
		}

	}


}

