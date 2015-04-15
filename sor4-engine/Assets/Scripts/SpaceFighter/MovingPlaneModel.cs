using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RetroBread;

// Plane model
[Serializable]
public class MovingPlaneModel: PhysicPlaneModel{

	public int movingState;
	public FixedFloat blendFactor;
	private FixedVector3[] path; 

	// Constructor giving world points
	public MovingPlaneModel(FixedVector3[] path, params FixedVector3[] paramPoints):
	this(DefaultUpdateOrder.PhysicsUpdateOrder, path, paramPoints){}

	// Constructor giving world points
	public MovingPlaneModel(int updatingOrder, FixedVector3[] path, params FixedVector3[] paramPoints):base(updatingOrder, paramPoints){
		this.path = path;
	}


	// Create controller
	protected override Controller<PhysicPlaneModel> CreateController(){
		return new MovingPlaneController(path);
	}

	protected override View<PhysicPlaneModel> CreateView(){
		return new MovingPlaneView();
	}

}

