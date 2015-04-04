
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

	public float transitionTime { get ; private set; }

	// Constructor
	public AnimationTransition(string nextAnimation, List<AnimationTriggerCondition> conditions, float transitionTime = 0.2f ){
		this.nextAnimation = nextAnimation;
		this.conditions = conditions;
		this.transitionTime = transitionTime;
	}

//	// Execute some code when doing the transition
//	// Example: cleanup current animation state variables
//	protected virtual void OnTransition(AnimationModel model){
//		// Nothing by default
//	}

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

	// can't hold it here cose same controller can act over various models
	//private bool animationChanged;

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


	// Execute any events for this keyframe
	private void ExecuteEvents(AnimationModel model){
		if (events != null){
			List<AnimationEvent> currentFrameEvents;
			if (events.TryGetValue(model.currentFrame, out currentFrameEvents)){
				foreach (AnimationEvent e in currentFrameEvents){
					e.Execute(model);
				}
			}
		}
	}


	private void CheckTransitions(AnimationModel model){

		// TODO: check global transitions (i.e. applicable to any state)

		string nextAnimation = null;
		AnimationTransition theTransition = null;
		foreach(AnimationTransition transition in transitions){
			nextAnimation = transition.CheckTransition(model);
			if (nextAnimation != null){
				theTransition = transition;
				break;
			}
		}
		
		// If there is a transition pending, move to it
		if (nextAnimation != null) {
			SetAnimation(model, nextAnimation);
			AnimationView view = model.GetView() as AnimationView;
			if (view != null){
				view.transitionTime = theTransition.transitionTime;
			}
		}

	}



	// On update we execute animation events 
	public override void Update(AnimationModel model){

		// reset changed flag
		model.animationChanged = false;

		// Check transitions based on how the state is now
		CheckTransitions(model);

		// Execute any events for this keyframe
		ExecuteEvents(model);
		
	}

	// Allow to force change animation
	public void SetAnimation(AnimationModel model, string nextAnimation, uint initialFrame = 0){
		model.animationName = nextAnimation;
		model.currentFrame = initialFrame;
		model.InvalidateVC();
		model.animationChanged = true;
	}


	// On post-update we move to next frame, if animation didn't change
	public override void PostUpdate(AnimationModel model){
		if (!model.animationChanged) {
			// move to next frame
			// Note: even though we increment it here, this update cycle was about currentFrame-1 
			++model.currentFrame;
		}
	}
	
}

