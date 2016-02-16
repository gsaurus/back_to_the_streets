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

		// This variables are used during each frame update only
		public string nextAnimation {get; private set; }
		public int nextFrame { get; private set; }

		// Invalid frame id
		public static int invalidFrameId = -1;


		// Animation changes at the end of the update
		public void SetNextAnimation(string nextAnimation, uint initialFrame = 0){
			this.nextAnimation = nextAnimation;
			this.nextFrame = (int)initialFrame;
		}

		public void ResetNextParameters(){
			this.nextAnimation = null;
			this.nextFrame = invalidFrameId;
		}


		#region Constructors

		// Default Constructor
		public AnimationModel(){
			// no next parameters
			ResetNextParameters();
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