using System;
using System.Collections.Generic;
using ProtoBuf;


namespace RetroBread{


	// Plane model
	[ProtoContract]
	public class PhysicPlaneModel: Model<PhysicPlaneModel>{

		// The first point is considered the plane's origin
		[ProtoMember(1)]
		public FixedVector3 origin;

		// List of offsets of the other points to the plane's origin.
		// They must be convex and coplanar
		[ProtoMember(2)]
		public List<FixedVector3> offsets;

		// Position of first point on the previous frame
		[ProtoMember(3)]
		public FixedVector3 lastOriginPosition;

		// Normal isn't serialized
		private FixedVector3 normal;
		public FixedVector3 Normal { get{ return normal; }}



		// Velocity = difference between current and previous positions
		// We use first point as reference
		public FixedVector3 GetVelocity(){
			return origin - lastOriginPosition;
		}

		// Normal computed from the first 2 consecutive offsets
		public void ComputeNormal(){
			// Normal vector
			normal = FixedVector3.Cross(offsets[0], offsets[1]); //(origin + offsets[1]) - (origin + offsets[0]));
			normal.Normalize();
		}


		// As we only store offsets, provide a way of accessing plane points
		public FixedVector3 GetPointFromOffsetId(int offsetId){
			return origin + offsets[offsetId];
		}


		public List<FixedVector3> GetPointsList()
		{
			List<FixedVector3> list = new List<FixedVector3>();
			list.Add(origin);
			for (int i = 0 ; i < offsets.Count ; ++i){
				list.Add(GetPointFromOffsetId(i));
			}
			return list;
		}


		// Constructor giving world points
		public PhysicPlaneModel(params FixedVector3[] paramPoints):
		this(DefaultVCFactoryIds.PhysicPlaneControllerFactoryId, null, DefaultUpdateOrder.PhysicsUpdateOrder, paramPoints){}

		// Constructor giving world points and controller factory id
		public PhysicPlaneModel(string controllerFactoryId, params FixedVector3[] paramPoints):
		this(controllerFactoryId, null, DefaultUpdateOrder.PhysicsUpdateOrder, paramPoints){}

		// Constructor giving world points and VC factory ids
		public PhysicPlaneModel(string controllerFactoryId, string viewFactoryId, params FixedVector3[] paramPoints):
		this(controllerFactoryId, viewFactoryId, DefaultUpdateOrder.PhysicsUpdateOrder, paramPoints){}


		// Constructor giving world points, VC factory ids and updating order
		public PhysicPlaneModel(string controllerFactoryId,
			                        string viewFactoryId,
			                        int updatingOrder,
			                        params FixedVector3[] paramPoints
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			if (paramPoints.Length < 3){
				// Can't build a plane with less than 3 points
				return;
			}
			origin = lastOriginPosition = paramPoints[0];
			offsets = new List<FixedVector3>(paramPoints.Length-1);
			for(int i = 1 ; i < paramPoints.Length; ++i){
				offsets.Add(paramPoints[i] - origin);
			}
			ComputeNormal();
		}

		[ProtoAfterDeserialization]
		public void OnDeserialization(){
			// Once deserialized, compute normal
			ComputeNormal();
		}


		// TODO: this should move to PhysicPlaneController!
		// Compute the intersection point against a line segment
		public bool CheckIntersection(PhysicPointModel pointModel, out FixedVector3 intersection){
			// plane may be moving, sum velocity to initial point position
			FixedVector3 pos1 = pointModel.lastPosition;
			FixedVector3 pos2 = pointModel.position;
			pos1 += GetVelocity();
		
			// check collision with the hiperplane
			FixedVector3 pointDeltaPos = pos2 - pos1;
			if (pointDeltaPos.Magnitude == 0){
				// The point is not moving relatively to the plane
				intersection = FixedVector3.Zero; return false;
			}
			FixedVector3 pos1ToOrigin = origin - pos1;
			FixedFloat dotDeltaPosNormal = FixedVector3.Dot(pointDeltaPos,normal);
			if (dotDeltaPosNormal >= 0){
				// Point moving away from the plane
				intersection = FixedVector3.Zero;
				return false;
			}

			// Find intersection location in the deltapos vector
			FixedFloat t = FixedVector3.Dot(pos1ToOrigin, normal) / dotDeltaPosNormal;
		
			// a small delta due to precision errors
			// based on deltaPos magnitude (the smaller the magnitude the higher the error)
			FixedFloat error = 0.01 / pointDeltaPos.Magnitude;

			if (t < -error){
				// falling through the plane, try step tolerance to recover
				pos1 += pointModel.stepTolerance;
				pointDeltaPos = pos2 - pos1;
				pos1ToOrigin = origin - pos1;
				dotDeltaPosNormal = FixedVector3.Dot(pointDeltaPos,normal);
				t = FixedVector3.Dot(pos1ToOrigin, normal) / dotDeltaPosNormal;
				error = 0.01 / pointDeltaPos.Magnitude;
			}
			// give some step tolerance
			if (t < -error || t > 1 + error) {
				// not colliding
				intersection = FixedVector3.Zero;
				return false;
			}
			intersection = pos1 + t * pointDeltaPos;

			// Check if intersection point is inside the plane
			FixedFloat anglesSum = FixedFloat.Zero;
			FixedVector3 originVector = origin - intersection;
			FixedVector3 vec1 = originVector;
			FixedVector3 vec2 = FixedVector3.Zero;
			FixedVector3 vertex;
			for (int i = 0 ; i < offsets.Count ; ++i){
				vertex = GetPointFromOffsetId(i);
				vec2 = vertex - intersection;
				anglesSum += FixedVector3.Angle(vec1, vec2);
				vec1 = vec2;
			}
			// last vertex with origin
			anglesSum += FixedVector3.Angle(vec2, originVector);

			// a small delta due to precision errors
			return FixedFloat.Abs(anglesSum - FixedFloat.TwoPI) < 0.05;
		}

	}



}

