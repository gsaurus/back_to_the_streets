using System;
using ProtoBuf;


namespace RetroBread{


	// Animation model keeps the name of the animation and it's current frame
	// We also keep a reference to the state of the owner, to use during animation updates
	[ProtoContract]
	public class AnimationModel:Model<AnimationModel>{

		[ProtoMember(1)] public ModelReference ownerId;
		[ProtoMember(2)] public uint currentFrame;
		[ProtoMember(3)] public string characterName;
		[ProtoMember(4)] public string animationName;

		// This variable is used during each frame only
		public bool animationChanged;


		#region Constructors

		// Default Constructor
		public AnimationModel(){
			// Nothing to do
		}


		// Constructor
		public AnimationModel(ModelReference ownerId,
		                      string characterName,
		                      string animationName
		):this(ownerId,
		       characterName,
		       animationName,
		       DefaultVCFactoryIds.AnimationControllerFactoryId,
		       DefaultVCFactoryIds.AnimationViewFactoryId,
		       DefaultUpdateOrder.AnimationsUpdateOrder
		){}



		// Constructor
		public AnimationModel(ModelReference ownerId,
		                      string characterName,
		                      string animationName,
		                      string controllerFactoryId,
		                      string viewFactoryId,
		                      int updatingOrder
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			this.characterName = characterName;
			this.animationName = animationName;
			this.currentFrame = 0;
			this.ownerId = ownerId;
		}


		#endregion
	

	}


}