using System;
using System.Collections.Generic;


namespace RetroBread{


// Animation Controller:
// Execute animation events and perform transitions automatically
public class AnimationController:Controller<AnimationModel>{

	// Events associated to keyframes (evaluated once)
	private Dictionary<uint, List<ConditionalEvent<GameEntityModel>>> keyframeEvents;

	// General events, evaluated every frame
	private List<ConditionalEvent<GameEntityModel>> generalEvents;


	// Collision and hits information
	private FrameData[] framesData;


	public AnimationController(){
		keyframeEvents = new Dictionary<uint, List<ConditionalEvent<GameEntityModel>>>();
		generalEvents = new List<ConditionalEvent<GameEntityModel>>();
	}

	// Add frame based events
	public void AddKeyframeEvent(uint keyframe, ConditionalEvent<GameEntityModel> e){
		List<ConditionalEvent<GameEntityModel>> frameEvents;
		if (!keyframeEvents.TryGetValue(keyframe, out frameEvents)){
			frameEvents = new List<ConditionalEvent<GameEntityModel>>(1);
			keyframeEvents.Add(keyframe, frameEvents);
		}
		frameEvents.Add(e);
	}

	// Add general event
	public void AddGeneralEvent(ConditionalEvent<GameEntityModel> e){
		generalEvents.Add(e);
	}

	// Setup frames data
	public void SetFramesData(FrameData[] framesData){
		this.framesData = framesData;
	}


	// Collision check against other controller & model
	// Params: animation models of both entities, offsets and orientation of both entities
	public bool CollisionCollisionCheck(
		AnimationModel model, FixedVector3 offset, bool facingRight,
		AnimationModel otherModel, FixedVector3 otherOffset, bool otherFacingRight
	){
		AnimationController otherController = otherModel.Controller() as AnimationController;
		FrameData data = framesData[model.currentFrame % framesData.Length];
		FrameData otherData = otherController.framesData[otherModel.currentFrame % otherController.framesData.Length];
		if (data == null || otherData == null) return false;
		return data.CollisionCollisionCheck(offset, facingRight, otherData, otherOffset, otherFacingRight);
	}


	// Hit check against other controller & model
	// Params: animation models of both entities, offsets and orientation of both entities
	public HitInformation HitCollisionCheck(
		AnimationModel model, FixedVector3 offset, bool facingRight,
		AnimationModel otherModel, FixedVector3 otherOffset, bool otherFacingRight
	){
		AnimationController otherController = otherModel.Controller() as AnimationController;
		FrameData data = framesData[model.currentFrame % framesData.Length];
		FrameData otherData = otherController.framesData[otherModel.currentFrame % otherController.framesData.Length];
		if (data == null || otherData == null) return null;
		HitInformation hitInformation = data.HitCollisionCheck(offset, facingRight, otherData, otherOffset, otherFacingRight, model.hittenEntitiesByHitId, otherModel.ownerId);
		if (hitInformation != null) {
			// store hitten so that it doesn't get hit again by this hit
			EnsureHittenData(model, hitInformation.hitData.hitboxID);
			model.hittenEntitiesByHitId[hitInformation.hitData.hitboxID].entities.Add(otherModel.ownerId);
		}
		return hitInformation;
	}


	private void EnsureHittenData(AnimationModel model, int hitboxId){
		if (model.hittenEntitiesByHitId == null) {
			model.hittenEntitiesByHitId = new List<AnimationHittenEntities>();
		}
		AnimationHittenEntities hittenEntities;
		while (model.hittenEntitiesByHitId.Count <= hitboxId) {
			hittenEntities = new AnimationHittenEntities();
			hittenEntities.entities = new List<ModelReference>();
			model.hittenEntitiesByHitId.Add(hittenEntities);
		}
	}



	// Process any events for this keyframe
	private void ProcessKeyframeEvents(AnimationModel model, GameEntityModel entityModel){
		if (keyframeEvents != null){
			List<ConditionalEvent<GameEntityModel>> currentFrameEvents;
			if (keyframeEvents.TryGetValue(model.currentFrame, out currentFrameEvents)){
				foreach (ConditionalEvent<GameEntityModel> e in currentFrameEvents){
					e.Evaluate(entityModel);
				}
			}
		}
	}

	// Process general animation events
	private void ProcessGeneralEvents(AnimationModel model, GameEntityModel entityModel){
		foreach (ConditionalEvent<GameEntityModel> e in generalEvents){
			e.Evaluate(entityModel);
		}
	}


	// On update we execute animation events 
	protected override void Update(AnimationModel model){

		GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
		if (entityModel != null && entityModel.pauseTimer > 0){
			return;
		}

		// reset animation & frame changed variables
		model.ResetNextParameters();

		// process keyframe events
		ProcessKeyframeEvents(model, entityModel);

		// process general events
		ProcessGeneralEvents(model, entityModel);
		
	}



	// On post-update we move to next frame, if animation didn't change
	protected override void PostUpdate(AnimationModel model){

		GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
		if (entityModel != null && entityModel.pauseTimer > 0){
			return;
		}

		bool haveNewNextFrame = model.nextFrame != AnimationModel.invalidFrameId;
		bool haveNewAnimation = model.nextAnimation != null && model.nextAnimation != model.animationName;

		if (haveNewNextFrame){
			model.currentFrame = (uint) model.nextFrame;
		}

		if (haveNewAnimation) {
			model.animationName = model.nextAnimation;
			// clear hitten entities
			model.hittenEntitiesByHitId = null;
			GameEntityModel anchoredModel;
			AnimationModel anchoredAnimationModel;
			foreach (ModelReference anchoredRef in entityModel.anchoredEntities) {
				if (anchoredRef != null && anchoredRef != ModelReference.InvalidModelIndex) {
					anchoredModel = StateManager.state.GetModel(anchoredRef) as GameEntityModel;
					if (anchoredModel == null) continue;
					anchoredAnimationModel = StateManager.state.GetModel(anchoredModel.animationModelId) as AnimationModel;
					if (anchoredAnimationModel == null) continue;
					anchoredAnimationModel.hittenEntitiesByHitId = null;
				}
			}
			model.InvalidateVC();
			// Clear combo animation flag
			entityModel.customVariables[CharacterConditionsBuilder.comboAnimationClearFlag] = 0;
		}

		if (!haveNewNextFrame && !haveNewAnimation){
			// move to next frame
			++model.currentFrame;
		}
	}


	// Force an animation to play, e.g. in grab circumstances. Should be avoided
	public void ForceAnimation(AnimationModel model, string animationName){
		model.currentFrame = 0;
		model.animationName = animationName;
		model.SetNextAnimation(animationName, 0);
		model.InvalidateVC();
	}


	public override bool IsCompatible(AnimationModel originalModel, AnimationModel newModel){
		return originalModel.characterName == newModel.characterName
			&& originalModel.animationName == newModel.animationName
		;
	}
	
}


}

