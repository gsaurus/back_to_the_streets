using System;
using ProtoBuf;
using System.Collections.Generic;


namespace RetroBread{


	// Wrapper for list of entities hit
	[ProtoContract]
	public class AnimationHittenEntities{
		// Team members
		[ProtoMember(1, OverwriteList=true)]
		public List<ModelReference> entities;

		public AnimationHittenEntities(){
			// Nothing to do
		}
	}


	// Animation model keeps the name of the animation and it's current frame
	// We also keep a reference to the state of the owner, to use during animation updates
	[ProtoContract]
	public class AnimationModel:Model<AnimationModel>{

		[ProtoMember(1)] public ModelReference ownerId;
		[ProtoMember(2)] public uint currentFrame;
		[ProtoMember(3)] public string characterName;
		[ProtoMember(4)] public string animationName;
		// A character may have different skins (2D or 3D models),
		// This variable tells which one to use
		[ProtoMember(5)] public string viewModelName;

		// What entities were hit during this animation
		// Used to prevent multihit from the same hitId
		[ProtoMember(6, OverwriteList=true)]
		public List<AnimationHittenEntities> hittenEntitiesByHitId;


		// This variables are used during each frame update only
		// Stored here because same controller may be used for multiple animation models
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
		                      string animationName,
		                      string viewModelName
		):this(ownerId,
		       characterName,
		       animationName,
		       viewModelName,
		       DefaultVCFactoryIds.AnimationControllerFactoryId,
		       DefaultVCFactoryIds.AnimationViewFactoryId,
		       DefaultUpdateOrder.AnimationsUpdateOrder
		){}



		// Constructor
		public AnimationModel(ModelReference ownerId,
		                      string characterName,
		                      string animationName,
		                      string viewModelName,
		                      string controllerFactoryId,
		                      string viewFactoryId,
		                      int updatingOrder
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			this.characterName = characterName;
			this.animationName = animationName;
			this.viewModelName = viewModelName;
			this.currentFrame = 0;
			this.ownerId = ownerId;
		}


		#endregion
	

	}


}