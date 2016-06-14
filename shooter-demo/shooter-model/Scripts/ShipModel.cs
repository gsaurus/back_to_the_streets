using System;
using System.Collections.Generic;
using RetroBread;
using ProtoBuf;



[ProtoContract]
public class ShipModel{

	[ProtoMember(1)]
	public ModelReference inputModelRef;

	[ProtoMember(2)]
	public int numMatrixLines;
	[ProtoMember(3, OverwriteList=true)]
	public int[] matrix;

	[ProtoMember(4)]
	public FixedFloat x;
	[ProtoMember(5)]
	public FixedFloat y;
	[ProtoMember(6)]
	public FixedFloat rotation;

	[ProtoMember(7)]
	public FixedFloat rotationVel;

	[ProtoMember(8)]
	public FixedFloat velAngle;
	[ProtoMember(9)]
	public FixedFloat velPower;

	[ProtoMember(10)]
	public FixedFloat aimingAngle;
	[ProtoMember(11)]
	public FixedFloat aimingVel;

	[ProtoMember(12)]
	public ModelReference dockerShip;

	[ProtoMember(13, OverwriteList=true)]
	public List<ModelReference> dockedShips;

	//---------------------------------
	// Extra data that could be obtained from matrix, but is stored to reduce computations

	// Those are based on the number of thrusts, rotation engines and range engines
	[ProtoMember(14)]
	public FixedFloat accelerationPower;
	[ProtoMember(15)]
	public FixedFloat aimingRotationPower;
	[ProtoMember(16)]
	public FixedFloat range;

	// Direct access to turrets, cannons and docks
	[ProtoMember(17)]
	public List<int> turretIds;
	[ProtoMember(18)]
	public List<int> cannonIds;
	[ProtoMember(19)]
	public List<int> dockIds;

	// Mass center cell ref of the ship
	[ProtoMember(20)]
	public int centerCell;

	/**
	 * Docker acceleration = ship.accelerationPower + recursive_sum(dockeds_ships.accelerationPower)
	 * Docker AND docked aimingRotationPower = ship.aimingRotationPower + recursive_sum(dockeds_ships.aimingRotationPower)
	 * No extra range, range based on each ship
	**/



	// Default Constructor
	public ShipModel(){
		
	}

	// Constructor
	public ShipModel(int numMatrixLines, int[] matrix, FixedFloat x, FixedFloat y, ModelReference inputModelRef){
		this.numMatrixLines = numMatrixLines;
		this.matrix = matrix;
		this.x = x;
		this.y = y;
		this.velAngle = 0;
		this.velPower = 0;
		this.aimingAngle = 0;
		this.dockerShip = new ModelReference(ModelReference.InvalidModelIndex);
		this.dockedShips = new List<ModelReference>();
		this.turretIds = new List<int>();
		this.cannonIds = new List<int>();
		this.dockIds = new List<int>();
		this.inputModelRef = inputModelRef;
	}

}

