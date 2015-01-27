
using System;
using UnityEngine;
using System.Collections.Generic;




// TODO: Think about how to control layers, at least leave generic to control by layer by derived classes
// remember layers are for things like walk with heavy pipe
// TODO: certain animations with custom "speed" control, how to
public class AnimationView:View<AnimationModel>{

	private float transitionTime = 0.2f;	 // TODO: depending on transition source -> dest?
	private float interpolationTime = 0.4f; // TODO: something based on lag?


	protected int GetAnimationCurrentFrame(AnimatorStateInfo stateInfo){
		//float currentTime = (stateInfo.normalizedTime - (int)stateInfo.normalizedTime)*stateInfo.length;
		float currentTime = stateInfo.normalizedTime*stateInfo.length;
		return (int)(currentTime / StateManager.Instance.UpdateRate);
	}


	// Visual update
	public override void Update(AnimationModel model, float deltaTime){

		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.ownerId);
		if (obj == null) return; // can't work without a game object
		Animator animator = obj.GetComponent<Animator>();
		if (animator == null) return; // can't work without an animator component

		// Get current animation (if transiting we consider next as current)
		AnimatorStateInfo stateInfo;
		if (animator.IsInTransition(0)){
			stateInfo = animator.GetNextAnimatorStateInfo(0);
		}else {
			stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		}

		if (stateInfo.IsName(model.animationName)) {
			// if time is not in sync, resync it
			int currentAnimationFrame = GetAnimationCurrentFrame(stateInfo);
			if (currentAnimationFrame > 0 && Math.Abs(currentAnimationFrame - model.currentFrame) > 2) {
				float timeToFade = Mathf.Abs(currentAnimationFrame - model.currentFrame) * StateManager.Instance.UpdateRate;
				timeToFade = Mathf.Min(timeToFade, interpolationTime);
				animator.CrossFade(model.animationName, timeToFade);
				//Debug.Log("fade time not in sync: " + currentAnimationFrame + ", " + model.currentFrame + "; time: " + timeToFade);
				Debug.Log("fade time not in sync");
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
				Debug.Log("fade forced sync");
			}
		}

	}

}

