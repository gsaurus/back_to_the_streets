
using System;
using UnityEngine;
using System.Collections.Generic;




// TODO: Think about how to control layers, at least leave generic to control by layer by derived classes
public class AnimationView:View<AnimationModel>{
	
	private Animator animator;

	private float transitionTime = 0.1f;	 // TODO: depending on transition source -> dest?
	private float interpolationTime = 0.3f; // TODO: something based on lag?

	public AnimationView(Animator animator){
		this.animator = animator;
	}
	


	protected int GetAnimationCurrentFrame(AnimatorStateInfo stateInfo){
		float currentTime = ((stateInfo.normalizedTime - (int)stateInfo.normalizedTime)*stateInfo.length);
		return (int)(currentTime / StateManager.Instance.UpdateRate);
	}


	// Visual update
	public override void Update(AnimationModel model, float deltaTime){

		// Get current animation (if transiting we consider next as current)
		AnimatorStateInfo stateInfo;
		if (animator.IsInTransition(0)){
			stateInfo = animator.GetNextAnimatorStateInfo(0);
		}else {
			stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		}

		if (stateInfo.IsName(model.name)) {
			// if time is not in sync, resync it
			int currentAnimationFrame = GetAnimationCurrentFrame(stateInfo);
			if (currentAnimationFrame != model.currentFrame) {
				float timeToFade = Mathf.Abs(currentAnimationFrame - model.currentFrame) * StateManager.Instance.UpdateRate;
				timeToFade = Mathf.Min(timeToFade, interpolationTime);
				animator.CrossFade(model.name, timeToFade);
			}
		}else {
			// We need to make transition to the new animation
			animator.CrossFade(model.name, transitionTime);
			// check target offset (note current API doesn't give access to length before starting the transition)
			AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(0);
			float nextAnimationOffset = model.currentFrame * StateManager.Instance.UpdateRate;
			float nextAnimationNormalizedOffset = nextAnimationOffset / nextStateInfo.length;
			if (nextAnimationNormalizedOffset >= StateManager.Instance.UpdateRate){
				// need to resync transition
				float timeToFade = Mathf.Min(nextAnimationOffset, interpolationTime);
				animator.CrossFade(model.name, timeToFade, 0, nextAnimationNormalizedOffset);
			}
		}

	}


	public override bool IsCompatibleWithModel(AnimationModel model){
		// TODO: check if the instance is of same type...
		return false;
	}

	
}

