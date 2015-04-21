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


}

