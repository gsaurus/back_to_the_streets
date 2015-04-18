﻿using UnityEngine;
using System;
using System.Collections;


namespace RetroBread{


	// Game entity used as base for characters and other game elements
	[Serializable]
	public class GameEntityModel: Model<GameEntityModel> {

		// Physic model
		public ModelReference physicsModelId;
		// Animation model
		public ModelReference animationModelId;
		// Input model (we store input info when necessary, specially if controlled by AI)
		public ModelReference inputModelId;

		// flag telling if the character is facing left or right
		public bool isFacingRight;

		// flag telling that the character can flip automatically depending on animation velocity
		public bool automaticFlip;

		// How much input velocity is applied to the entity
		public FixedVector3 maxInputVelocity;


		// Create a default Game Entity model
		public GameEntityModel(
			string characterName,
			string animationName,
			PhysicWorldModel worldModel,
			Model inputModel,
			FixedVector3 position,
			FixedVector3 stepTolerance,
			string controllerFactoryId	= DefaultVCFactoryIds.GameEntityControllerFactoryId,
			string viewFactoryId		= DefaultVCFactoryIds.GameEntityViewFactoryId,
			int updatingOrder			= DefaultUpdateOrder.EntitiesUpdateOrder
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			physicsModelId = StateManager.state.AddModel(new PhysicPointModel(this.Index, position, stepTolerance));
			worldModel.pointModels.Add(physicsModelId);
			animationModelId = StateManager.state.AddModel(new AnimationModel(this.Index, characterName, animationName));
			inputModelId = StateManager.state.AddModel(inputModel);
		}

		// TODO: other constructors receiving pre-made sub-models, etc?


		
		// called when the game entity model is destroyed
		public override void OnDestroy(){
			// err.. don't cleanup dependant states, destroy may be called via resync,
			// the state may be out of the state manager
	//		// Cleanup dependant states
	//		StateManager.state.RemoveModel(physicsModelId);
	//		StateManager.state.RemoveModel(animationModelId);
	//		StateManager.state.RemoveModel(inputModelId);
		}

	}


}
