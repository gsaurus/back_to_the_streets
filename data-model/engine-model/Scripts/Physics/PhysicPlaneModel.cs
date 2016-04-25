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
		public FixedVector3 normal { get; private set; }

		public FixedVector3 minPos { get; private set; }
		public FixedVector3 maxPos { get; private set; }



		// Velocity = difference between current and previous positions
		// We use first point as reference
		public FixedVector3 GetVelocity(){
			return origin - lastOriginPosition;
		}

		public void ComputeBoundingBox(){
			if (offsets == null || offsets.Count < 2) return;
			minPos = FixedVector3.Zero;
			maxPos = FixedVector3.Zero;
			FixedVector3 point;
			for (int i = 0 ; i < offsets.Count ; ++i){
				point = offsets[i];
				minPos = FixedVector3.Min(minPos, point);
				maxPos = FixedVector3.Max(maxPos, point);
			}
			FixedVector3 tolerance = new FixedVector3(0.01f, 0.01f, 0.01f);
			minPos -= tolerance;
			maxPos += tolerance;
		}

		// Normal computed from the first 2 consecutive offsets
		public void ComputeNormal(){
			// Normal vector
			if (offsets == null || offsets.Count < 2) return;
			normal = FixedVector3.Cross(offsets[0], offsets[1]);
			normal = normal.Normalized;
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


		// Default constructor
		public PhysicPlaneModel(){
			// Nothing to do
		}

		// Constructor giving world points
		public PhysicPlaneModel(params FixedVector3[] paramPoints):
		this(null, null, DefaultUpdateOrder.PhysicsUpdateOrder, paramPoints){}

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
			ComputeBoundingBox();
		}
			

		[ProtoAfterDeserialization]
		public void OnDeserialization(){
			// Once deserialized, compute normal and bounding box
			ComputeNormal();
			ComputeBoundingBox();
		}

	}



}

