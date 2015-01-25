
using System;

[Serializable]
public class ShipModel:Model<ShipModel>{

	public uint player;
	public uint physicsModelId;
	public uint animationModelId;

	public bool leftHolded;
	public bool rightHolded;
	public bool upHolded;
	public bool downHolded;


	public ShipModel(uint player){
		this.player = player;
		leftHolded = false;
		rightHolded = false;
		upHolded = false;
		downHolded = false;
	}


	protected override View<ShipModel> CreateView(){
		return new ShipView();
	}


	protected override Controller<ShipModel> CreateController(){
		return new ShipController();
	}

}

