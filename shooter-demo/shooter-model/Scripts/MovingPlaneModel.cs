using System;
using RetroBread;
using ProtoBuf;

// Plane model
[ProtoContract]
public class MovingPlaneModel: PhysicPlaneModel{

	public const string MovingPlaneControllerFactoryId = "my_mpc";
	public const string MovingPlaneViewFactoryId = "my_mpv";

	[ProtoMember(1)] public int movingState;
	[ProtoMember(2)] public FixedFloat blendFactor;
	[ProtoMember(3)] public FixedVector3[] path { get; private set; }

	// Default Constructor
	public MovingPlaneModel(){
		// Nothing to do
	}

	// Constructor giving world points
	public MovingPlaneModel(FixedVector3[] path, params FixedVector3[] paramPoints):
	this(DefaultUpdateOrder.PhysicsUpdateOrder, path, paramPoints){}

	// Constructor giving world points
	public MovingPlaneModel(int updatingOrder, FixedVector3[] path, params FixedVector3[] paramPoints)
	:base(MovingPlaneControllerFactoryId, MovingPlaneViewFactoryId, updatingOrder, paramPoints){
		this.path = path;
	}


	protected override void AssignCopy(PhysicPlaneModel other){
		base.AssignCopy(other);

		MovingPlaneModel otherPlane = other as MovingPlaneModel;
		if (otherPlane == null) return;
		movingState = otherPlane.movingState;
		blendFactor = otherPlane.blendFactor;
		path = new FixedVector3[otherPlane.path.Length];
		for (int i = 0 ; i < otherPlane.path.Length ; ++i){
			path[i] = otherPlane.path[i];
		}

		/*
		int[] a = new int[] {1,2,3,4,5,6,7,8};
		int[] b = new int[a.Length];
		int size = sizeof(int);
		int length = a.Length * size;               
		System.Buffer.BlockCopy(a, 0, b, 0, length);
		*/
	}

}

