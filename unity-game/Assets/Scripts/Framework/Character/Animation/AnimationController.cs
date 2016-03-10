using System;
using System.Collections.Generic;


namespace RetroBread{


	// A trigger condition evaluates to true/false
	public interface AnimationTriggerCondition{
		bool Evaluate(AnimationModel model);
	}

	// An animation event is something we can execute if certain condition is met
	// Implementations can store information to be used during execute
	// Example: playSound stores the name of the sound and call playSound(soundName) on execute
	public abstract class AnimationEvent{

		// Condition may be null if executed directly at a key frame
		// It is public only to be edited during character loading..
		public AnimationTriggerCondition condition;

		public AnimationEvent(AnimationTriggerCondition condition){
			this.condition = condition;
		}

		// To be implemented, execute the event
		protected abstract void Execute(AnimationModel model);

		// Check the condition and execute event if condition passes
		public void Evaluate(AnimationModel model){
			if (condition == null || condition.Evaluate(model)){
				Execute(model);
			}
		}
	}


	// Animation Controller:
	// Execute animation events and perform transitions automatically
	public class AnimationController:Controller<AnimationModel>{

		// Events associated to keyframes (evaluated once)
		private Dictionary<uint, List<AnimationEvent>> keyframeEvents;

		// General events, evaluated every frame
		private List<AnimationEvent> generalEvents;

		public AnimationController(){
			keyframeEvents = new Dictionary<uint, List<AnimationEvent>>();
			generalEvents = new List<AnimationEvent>();
		}

		// Add frame based events
		public void AddKeyframeEvent(uint keyframe, AnimationEvent e){
			List<AnimationEvent> frameEvents;
			if (!keyframeEvents.TryGetValue(keyframe, out frameEvents)){
				frameEvents = new List<AnimationEvent>(1);
				keyframeEvents.Add(keyframe, frameEvents);
			}
			frameEvents.Add(e);
		}

		// Add general event
		public void AddGeneralEvent(AnimationEvent e){
			generalEvents.Add(e);
		}


		// Process any events for this keyframe
		private void ProcessKeyframeEvents(AnimationModel model){
			if (keyframeEvents != null){
				List<AnimationEvent> currentFrameEvents;
				if (keyframeEvents.TryGetValue(model.currentFrame, out currentFrameEvents)){
					foreach (AnimationEvent e in currentFrameEvents){
						e.Evaluate(model);
					}
				}
			}
		}

		// Process general animation events
		private void ProcessGeneralEvents(AnimationModel model){
			foreach (AnimationEvent e in generalEvents){
				e.Evaluate(model);
			}
		}


		// On update we execute animation events 
		protected override void Update(AnimationModel model){

			// reset animation & frame changed variables
			model.ResetNextParameters();

			// process keyframe events
			ProcessKeyframeEvents(model);

			// process general events
			ProcessGeneralEvents(model);
			
		}



		// On post-update we move to next frame, if animation didn't change
		protected override void PostUpdate(AnimationModel model){

			bool haveNewNextFrame = model.nextFrame != AnimationModel.invalidFrameId;
			bool haveNewAnimation = model.nextAnimation != null && model.nextAnimation != model.animationName;

			if (haveNewNextFrame){
				model.currentFrame = (uint) model.nextFrame;
			}

			if (haveNewAnimation) {
				model.animationName = model.nextAnimation;
				model.InvalidateVC();
			}

			if (!haveNewNextFrame && !haveNewAnimation){
				// move to next frame
				++model.currentFrame;
			}
		}


		public override bool IsCompatible(AnimationModel originalModel, AnimationModel newModel){
			return originalModel.characterName == newModel.characterName
				&& originalModel.animationName == newModel.animationName
			;
		}
		
	}


}

