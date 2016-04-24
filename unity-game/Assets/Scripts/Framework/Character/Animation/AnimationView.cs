
using System;
using UnityEngine;
using System.Collections.Generic;



namespace RetroBread{



	// TODO: Think about how to control layers, at least leave generic to control by layer by derived classes
	// remember layers are for things like walk with heavy pipe
	// TODO: certain animations with custom "speed" control, how to
	public class AnimationView:View<AnimationModel>{

		// This is public, so that it can be set from outside
		public float transitionTime = 0.2f;
		public float interpolationTime = 0.4f; // TODO: something based on lag?

		// This flag lets derived classes to turn off frame synchronization
		// E.g. walk animation needs a different timing than model frames
		protected bool isTimingSynchroizedWithModelFrames = true;


		protected int GetAnimationCurrentFrame(AnimatorStateInfo stateInfo){
			//float currentTime = (stateInfo.normalizedTime - (int)stateInfo.normalizedTime)*stateInfo.length;
			float currentTime = stateInfo.normalizedTime * stateInfo.length;
			return (int)(currentTime / StateManager.Instance.UpdateRate);
		}



		private void LegacyUpdate(AnimationModel model, Animation animation, float deltaTime){
			// don't animate if paused
			animation.enabled = !StateManager.Instance.IsPaused;
			if (!animation.enabled) return;
			
			if (!animation.IsPlaying(model.animationName)) {
				//animation.Play(model.animationName);
				animation.CrossFade(model.animationName, transitionTime);
			}
		}



		// Visual update
		protected override void Update(AnimationModel model, float deltaTime){

			GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.ownerId);
			if (obj == null) return; // can't work without a game object
			Animator animator = obj.GetComponent<Animator>();
			if (animator == null) {
				// No animator, use legacy animation
				Animation animation = obj.GetComponent<Animation>();
				if (animation != null) {
					LegacyUpdate(model, animation, deltaTime);
				}
				return;
			}

			// don't animate if paused
			GameEntityModel entityModel = StateManager.state.GetModel(model.ownerId) as GameEntityModel;
			bool entityPaused = false;
			if (entityModel != null) {
				entityPaused = entityModel.pauseTimer > 0;
			}
			animator.enabled = !(StateManager.Instance.IsPaused || entityPaused);
			if (!animator.enabled) return;

			// Get current animation (if transiting we consider next as current)
			AnimatorStateInfo stateInfo;
			if (animator.IsInTransition(0)){
				stateInfo = animator.GetNextAnimatorStateInfo(0);
			}else {
				stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			}

			if (stateInfo.IsName(model.animationName)) {
				// if time is not in sync, resync it
				if (isTimingSynchroizedWithModelFrames) {
					int currentAnimationFrame = GetAnimationCurrentFrame(stateInfo);
					if (Math.Abs(currentAnimationFrame - model.currentFrame) > 2) {
						animator.Play(model.animationName, 0, (model.currentFrame * StateManager.Instance.UpdateRate) / stateInfo.length);
					}
				}
			}else {
				// We need to make transition to the new animation
				animator.CrossFade(model.animationName, transitionTime);
				// check target offset (note current API doesn't give access to length before starting the transition)
				AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(0);
				float nextAnimationOffset = model.currentFrame * StateManager.Instance.UpdateRate;
				float nextAnimationNormalizedOffset = nextAnimationOffset / nextStateInfo.length;
				if (nextAnimationNormalizedOffset >= StateManager.Instance.UpdateRate){
					// need to resync transition
					float timeToFade = Mathf.Min(nextAnimationOffset, interpolationTime);
					animator.CrossFade(model.animationName, timeToFade, 0, nextAnimationNormalizedOffset);
					//Debug.Log("fade forced sync");
				}
			}

		}


		public override bool IsCompatible(AnimationModel originalModel, AnimationModel newModel){
			return originalModel.characterName == newModel.characterName
				&& originalModel.animationName == newModel.animationName
				&& originalModel.viewModelName == newModel.viewModelName
			;
		}

	}



}

