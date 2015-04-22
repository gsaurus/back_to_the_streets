using System;
using ProtoBuf;


namespace RetroBread{


	// Game entity used as base for characters and other game elements
	[ProtoContract]
	public class GameEntityModel: Model<GameEntityModel> {

		// Physic model
		[ProtoMember(1)]
		public ModelReference physicsModelId;

		// Animation model
		[ProtoMember(2)]
		public ModelReference animationModelId;

		// Input model (we store input info when necessary, specially if controlled by AI)
		[ProtoMember(3)]
		public ModelReference inputModelId;

		// flag telling if the character is facing left or right
		[ProtoMember(4)]
		public bool isFacingRight;

		// flag telling that the character can flip automatically depending on animation velocity
		[ProtoMember(5)]
		public bool automaticFlip;

		// How much input velocity is applied to the entity
		[ProtoMember(6)]
		public FixedVector3 maxInputVelocity;



		#region Constructors


		// Default Constructor
		public GameEntityModel(){
			// nothing to do
		}


		public GameEntityModel(
			State state,
			string characterName,
			string animationName,
			PhysicWorldModel worldModel,
			Model inputModel,
			FixedVector3 position,
			FixedVector3 stepTolerance
		):this(state,
		       characterName,
		       animationName,
		       worldModel,
		       inputModel,
		       position,
		       stepTolerance,
		       DefaultVCFactoryIds.GameEntityControllerFactoryId,
		       DefaultVCFactoryIds.GameEntityViewFactoryId,
		       DefaultUpdateOrder.EntitiesUpdateOrder
		 ){}


		// Create a default Game Entity model
		public GameEntityModel(
			State state,
			string characterName,
			string animationName,
			PhysicWorldModel worldModel,
			Model inputModel,
			FixedVector3 position,
			FixedVector3 stepTolerance,
			string controllerFactoryId,
			string viewFactoryId,
			int updatingOrder
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			physicsModelId = state.AddModel(new PhysicPointModel(this.Index, position, stepTolerance));
			worldModel.pointModels.Add(physicsModelId);
			animationModelId = state.AddModel(new AnimationModel(this.Index, characterName, animationName));
			inputModelId = state.AddModel(inputModel);
		}

		// TODO: other constructors receiving pre-made sub-models, etc?
		// TODO: simplify arguments with a builder object?


		// Copy fields from other model
		protected override void AssignCopy(GameEntityModel other){
			base.AssignCopy(other);

			physicsModelId = new ModelReference(other.physicsModelId);
			animationModelId = new ModelReference(other.animationModelId);
			inputModelId = new ModelReference(other.inputModelId);
			isFacingRight = other.isFacingRight;
			automaticFlip = other.automaticFlip;
			maxInputVelocity = other.maxInputVelocity;

		}

		#endregion


		
		// called when the game entity model is destroyed
		protected override void OnDestroy(){
			// err.. don't cleanup dependant states, destroy may be called via resync,
			// the state may be out of the state manager
	//		// Cleanup dependant states
	//		StateManager.state.RemoveModel(physicsModelId);
	//		StateManager.state.RemoveModel(animationModelId);
	//		StateManager.state.RemoveModel(inputModelId);
		}

	}


}
