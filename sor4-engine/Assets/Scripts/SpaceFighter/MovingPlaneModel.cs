using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RetroBread;

// Plane model
[Serializable]
public class MovingPlaneModel: PhysicPlaneModel{

	public const string MovingPlaneControllerFactoryId = "my_mpc";
	public const string MovingPlaneViewFactoryId = "my_mpv";

	public int movingState;
	public FixedFloat blendFactor;
	public FixedVector3[] path { get; private set; }

	// Constructor giving world points
	public MovingPlaneModel(FixedVector3[] path, params FixedVector3[] paramPoints):
	this(DefaultUpdateOrder.PhysicsUpdateOrder, path, paramPoints){}

	// Constructor giving world points
	public MovingPlaneModel(int updatingOrder, FixedVector3[] path, params FixedVector3[] paramPoints)
	:base(MovingPlaneControllerFactoryId, MovingPlaneViewFactoryId, updatingOrder, paramPoints){
		this.path = path;
	}


}

