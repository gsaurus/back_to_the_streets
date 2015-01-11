
using System;

[Serializable]
public class BulletModel:Model<BulletModel>{

	public uint player;

	public FixedVector3 position;


	public BulletModel(uint player, FixedFloat x, FixedFloat y, FixedFloat z){
		this.player = player;
		this.position = new FixedVector3(x, y, z);
	}


	protected override View<BulletModel> CreateView(){
		return new BulletView(this);
	}


	protected override Controller<BulletModel> CreateController(){
		return new BulletController();
	}

}

