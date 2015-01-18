
using System;


// Animation model keeps the name of the animation and it's current frame
// We also keep a reference to the state of the owner, to use during animation updates
[Serializable]
public class AnimationModel:Model<AnimationModel>{

	public uint ownerStateId;
	public uint currentFrame;
	public string name;


	// Constructor
	public AnimationModel(uint ownerStateId, string name, int updatingOrder = 0):
		base(updatingOrder)
	{
		this.name = name;
		this.currentFrame = 0;
		this.ownerStateId = ownerStateId;
	}


	protected override Controller<AnimationModel> CreateController(){
		// Get the controller that corresponds to the current animation name
		return AnimationsManager.Instance.GetController(name);
	}

}

