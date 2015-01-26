
using System;
using System.Collections.Generic;


// An animation event is something we can execute at a certain keyframe
// Implementations can store information to be used during execute
// Example: playSound stores the name of the sound and call playSound(soundName) on execute
public interface AnimationEvent{
	void Execute(AnimationModel model);
}

// A trigger condition evaluates to true/false
public interface AnimationTriggerCondition{
	bool Evaluate(AnimationModel model);
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
	protected virtual void OnTransition(AnimationModel model){
		// Nothing by default
	}

	// Evaluate conditions
	public string CheckTransition(AnimationModel model){
		foreach (AnimationTriggerCondition condition in conditions){
			if (!condition.Evaluate(model)) {
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
	}

	// Add animation events
	public void AddEvent(uint keyframe, AnimationEvent e){
		List<AnimationEvent> frameEvents;
		if (!events.TryGetValue(keyframe, out frameEvents)){
			frameEvents = new List<AnimationEvent>(1);
			events.Add(keyframe, frameEvents);
		}
		frameEvents.Add(e);
	}

	// Add transition
	public void AddTransition(AnimationTransition transition){
		transitions.Add(transition);
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
					e.Execute(model);
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
				nextAnimation = transition.CheckTransition(model);
				if (nextAnimation != null) break;
			}
		}

		// If there is a transition pending, move to it
		if (nextAnimation != null) {
			model.animationName = nextAnimation;
			model.currentFrame = 0;
			model.InvalidateVC();
		}else {
			// update animation time
			++model.currentFrame;
		}
	}
	
}

