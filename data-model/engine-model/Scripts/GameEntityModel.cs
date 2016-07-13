﻿using System;
using ProtoBuf;
using System.Collections.Generic;


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

		// Pause timer (built in pause support)
		[ProtoMember(7)]
		public int pauseTimer;

		// Custom int variables.
		// e.g. comboCounter, energy, etc
		[ProtoMember(8, OverwriteList=true)]
		public Dictionary<string, int> customVariables;

		// Custom automatic timers (they decrement on each frame)
		// e.g. comboTimer, knockdownTimer
		[ProtoMember(9, OverwriteList=true)]
		public Dictionary<string, int> customTimers;

		// Entities anchored to it
		[ProtoMember(10, OverwriteList=true)]
		public List<ModelReference> anchoredEntities;

		// If anchored, to what entity is it anchored to
		[ProtoMember(11)]
		public ModelReference parentEntity;

		// Keep hooked to parent entity at a relative position
		// Only used if parent entity is != null.
		[ProtoMember(12)]
		public FixedVector3 positionRelativeToParent;


		#region Constructors


		// Default Constructor
		public GameEntityModel(){
			// nothing to do
		}


		public GameEntityModel(
			State state,
			string characterName,
			string animationName,
			string viewModelName,
			PhysicWorldModel worldModel,
			Model inputModel,
			FixedVector3 position,
			FixedVector3 stepTolerance
		):this(state,
		       characterName,
		       animationName,
		       viewModelName,
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
			string viewModelName,
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
			animationModelId = state.AddModel(new AnimationModel(this.Index, characterName, animationName, viewModelName));
			if (inputModel != null) { 
				inputModelId = state.AddModel(inputModel);
			} else {
				inputModelId = new ModelReference(ModelReference.InvalidModelIndex);
			}
			anchoredEntities = new List<ModelReference>();
			customVariables = new Dictionary<string, int>();
			customTimers = new Dictionary<string, int>();
		}



		public GameEntityModel(
			State state,
			PhysicWorldModel worldModel,
			PhysicPointModel physicsModel,
			AnimationModel animationModel,
			Model inputModel,
			string controllerFactoryId,
			string viewFactoryId,
			int updatingOrder
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			animationModel.ownerId = this.Index;
			physicsModel.ownerId = this.Index;
			physicsModelId = state.AddModel(physicsModel);
			worldModel.pointModels.Add(physicsModelId);
			animationModelId = state.AddModel(animationModel);
			if (inputModel != null) { 
				inputModelId = state.AddModel(inputModel);
			} else {
				inputModelId = new ModelReference(ModelReference.InvalidModelIndex);
			}
			anchoredEntities = new List<ModelReference>();
			customVariables = new Dictionary<string, int>();
			customTimers = new Dictionary<string, int>();
		}

		// TODO: simplify arguments with a builder object?


		#endregion

		// TODO: Not sure about custom variables and timers yet 
//		private void EnsureListIndex(ref List<int> list, int index){
//			if (list == null) {
//				list = new List<int>();
//			}
//			while (list.Count <= index) {
//				list.Add(0);
//			}
//		}
//
//		public int GetVariable(int variableIndex){
//			EnsureListIndex(customVariables, variableIndex);
//			return customVariables[variableIndex];
//		}
//
//		public void SetVariable(int variableIndex, int value){
//			EnsureListIndex(customVariables, variableIndex);
//			customVariables[variableIndex] = value;
//		}
//
//		public int GetTimer(int timerIndex){
//			EnsureListIndex(customTimers, timerIndex);
//			return customTimers[timerIndex];
//		}
//
//		public void SetTimer(int timerIndex, int value){
//			EnsureListIndex(customTimers, timerIndex);
//			customTimers[timerIndex] = value;
//		}


		
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
