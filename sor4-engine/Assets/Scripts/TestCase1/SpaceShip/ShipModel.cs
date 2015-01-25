
using System;

[Serializable]
public class ShipModel:PhysicPointModel{

	public uint player;

	public bool leftHolded;
	public bool rightHolded;
	public bool upHolded;
	public bool downHolded;


	public ShipModel(uint player, FixedFloat x, FixedFloat y):
		base(new FixedVector3(x,y,0), PhysicWorldController.PhysicsUpdateOrder)
	{
		this.player = player;
		leftHolded = false;
		rightHolded = false;
		upHolded = false;
		downHolded = false;
	}


	protected override View<PhysicPointModel> CreateView(){
		return new ShipView();
	}


	protected override Controller<PhysicPointModel> CreateController(){
		return new ShipController();
	}

}

