
using System;


// Animation model keeps the name of the animation and it's current frame
// We also keep a reference to the state of the owner, to use during animation updates
[Serializable]
public class AnimationModel:Model<AnimationModel>{

	public uint ownerId;
	public uint currentFrame;
	public string characterName;
	public string animationName;


	// Constructor
	public AnimationModel(uint ownerId, string characterName, string animationName, int updatingOrder = 0):
		base(updatingOrder)
	{
		this.characterName = characterName;
		this.animationName = animationName;
		this.currentFrame = 0;
		this.ownerId = ownerId;
	}


	protected override Controller<AnimationModel> CreateController(){
		// Get the controller that corresponds to the current animation name
		return AnimationsVCPool.Instance.GetController(characterName, animationName);
	}

	protected override View<AnimationModel> CreateView(){
		// Get the controller that corresponds to the current animation name
		return AnimationsVCPool.Instance.GetView(characterName, animationName);
	}

}

