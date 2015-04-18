using System;


namespace RetroBread{


	// Animation model keeps the name of the animation and it's current frame
	// We also keep a reference to the state of the owner, to use during animation updates
	[Serializable]
	public class AnimationModel:Model<AnimationModel>{

		public ModelReference ownerId;
		public uint currentFrame;
		public string characterName;
		public string animationName;

		// This variable is used during each frame only
		[NonSerialized]
		public bool animationChanged;

		// Constructor
		public AnimationModel(ModelReference ownerId,
		                      string characterName,
		                      string animationName,
		                      string controllerFactoryId	= DefaultVCFactoryIds.AnimationControllerFactoryId,
		                      string viewFactoryId			= DefaultVCFactoryIds.AnimationViewFactoryId,
		                      int updatingOrder 			= DefaultUpdateOrder.AnimationsUpdateOrder):
		base(controllerFactoryId, viewFactoryId, updatingOrder){
			this.characterName = characterName;
			this.animationName = animationName;
			this.currentFrame = 0;
			this.ownerId = ownerId;
		}
	

	}


}