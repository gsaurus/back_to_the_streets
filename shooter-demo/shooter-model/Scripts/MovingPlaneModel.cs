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
	[ProtoMember(3)] public FixedVector3[] planePath;

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
		this.planePath = path;
	}


	// Assign copy.
	// Need to override Clone too because the generic method won't build this class
	protected override void AssignCopy(PhysicPlaneModel other){
		base.AssignCopy(other);

		MovingPlaneModel otherPlane = other as MovingPlaneModel;
		if (otherPlane == null) return;
		movingState = otherPlane.movingState;
		blendFactor = otherPlane.blendFactor;
		planePath = (FixedVector3[])otherPlane.planePath.Clone();
	}
	public override Model Clone(){
		MovingPlaneModel newModel = new MovingPlaneModel();
		newModel.AssignCopy(this);
		return newModel;
	}

}

