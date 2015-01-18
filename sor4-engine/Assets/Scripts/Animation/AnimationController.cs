
using System;
using UnityEngine;
using System.Collections.Generic;


// An animation event is something we can execute at a certain keyframe
public interface AnimationEvent{
	void Execute(uint ownerStateId);
}

// A trigger condition evaluates to true/false
public interface AnimationTriggerCondition{
	bool Evaluate(uint ownerStateId);
}

// A transition executes if a list of conditions are verified
public class AnimationTransition{

	// Transition endpoint
	private string nextAnimation;

	// List of conditions for the transition
	private List<AnimationTriggerCondition> conditions;

	// Constructor
	public AnimationTransition(string nextAnimation, List<AnimationTriggerCondition> conditions){
		this.nextAnimation = nextAnimation;
		this.conditions = conditions;
	}

	// Execute some code when doing the transition
	// Example: cleanup current animation state variables
	protected virtual void OnTransition(uint ownerStateId){
		// Nothing by default
	}

	// Evaluate conditions
	public string CheckTransition(uint ownerStateId){
		foreach (AnimationTriggerCondition condition in conditions){
			if (!condition.Evaluate(ownerStateId)) {
				return null;
			}
		}
		// All conditions verified
		return nextAnimation;
	}

}


// Animation Controller:
// Execute animation events and perform transitions automatically
public class AnimationController:Controller<AnimationModel>{

	private Dictionary<uint, List<AnimationEvent>> events;
	private List<AnimationTransition> transitions;

	private string nextAnimation;

	public AnimationController(){
		events = new Dictionary<uint, List<AnimationEvent>>();
		transitions = new List<AnimationTransition>();
		// TODO: read events and transitions from somewhere...
	}


	// On update we execute animation events 
	public override void Update(AnimationModel model){

		// Invalidate next animation
		nextAnimation = null;

		// Execute any events for this keyframe
		if (events != null){
			List<AnimationEvent> currentFrameEvents;
			if (events.TryGetValue(model.currentFrame, out currentFrameEvents)){
				foreach (AnimationEvent e in currentFrameEvents){
					e.Execute(model.ownerStateId);
				}
			}
		}

	}

	// Allow to force change animation
	public void SetNextAnimation(string nextAnimation){
		this.nextAnimation = nextAnimation;
	}


	// On post-update we check and apply transitions
	public override void PostUpdate(AnimationModel model){

		// Check transitions to other animations, only if next animation wasn't forced
		if (nextAnimation == null){
			foreach(AnimationTransition transition in transitions){
				nextAnimation = transition.CheckTransition(model.ownerStateId);
				if (nextAnimation != null) break;
			}
		}

		// If there is a transition pending, move to it
		if (nextAnimation != null) {
			model.name = nextAnimation;
			model.currentFrame = 0;
			model.InvalidateController();
		}else {
			// update animation time
			++model.currentFrame;
		}
	}
	
}

