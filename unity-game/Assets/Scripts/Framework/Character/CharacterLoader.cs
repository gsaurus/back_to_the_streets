using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class CharacterLoader{

		// TODO: may store here a list of known characters.. to know what exists when creating entities

		// Auxiliar temporary condition. Frame based events go in a different bucket
		private class TemporaryFrameCondition: AnimationTriggerCondition{

			public static int lastAnimationFrame = -1;
			public int frameNumber { get; private set; }
			// May have a replacement condition
			public AnimationTriggerCondition subCondition;

			public TemporaryFrameCondition(int frameNumber, AnimationTriggerCondition subCondition = null){
				this.frameNumber = frameNumber;
				this.subCondition = subCondition;
			}

			public bool Evaluate(AnimationModel model){
				// Should not be evaluated
				throw new Exception("TemporaryFrameCondition: Should not be evaluated");
			}

		}


		private static AnimationTriggerCondition ReadCondition(Storage.GenericParameter condition){

			// TODO: depending on parameter type, for now dummy condition
			// TODO: call the plugin to create the condition
			return new TemporaryFrameCondition(TemporaryFrameCondition.lastAnimationFrame);

		}


		private static AnimationEvent ReadEvent(Storage.CharacterEvent e){

			AnimationTriggerCondition condition = conditions[e.conditionId];

			// TODO: create respective event based on parameter type
			// TODO: call the plugin to create the event for the e.param with the given condition
			return null;
		}


		public static void SetupCharacter(Storage.Character charData){

			// Collect global information

			string charName = charData.name;
			AnimationTriggerCondition condition;
			List<AnimationTriggerCondition> conditions = new List<AnimationTriggerCondition>(charData.conditions.Length);
			List<AnimationEvent> events = new List<AnimationEvent>(charData.events.Length);

			// Load all conditions
			foreach(Storage.GenericParameter storageCondition in charData.conditions){
				conditions.Add( ReadCondition(storageCondition) );
			}

			// Load all events
			foreach(Storage.CharacterEvent storageEvent in charData.events){
				events.Add( ReadEvent(storageEvent) );
			}

			// Setup each animation
			AnimationController controller;
			TemporaryFrameCondition tempCondition;
			int keyFrame;
			foreach(Storage.CharacterAnimation animation in charData.animations){

				// Register controller for this animation
				controller = new AnimationController();
				AnimationsVCPool.Instance.RegisterController(charName, animation.name, controller);

				// Setup animation events
				foreach(int eventIndex in animation.eventIds){
					AnimationEvent animEvent = events[eventIndex];
					condition = animEvent.condition;
					if (condition is TemporaryFrameCondition){
						// Instead of frame condition, add event as keyframe event
						tempCondition = condition as TemporaryFrameCondition;
						animEvent.condition = tempCondition.subCondition;
						if (tempCondition.frameNumber == TemporaryFrameCondition.lastAnimationFrame){
							keyFrame = animation.numFrames;
						}else{
							keyFrame = tempCondition.frameNumber;
						} 
						controller.AddKeyframeEvent((uint)keyFrame, animEvent);
					}else{
						controller.AddGeneralEvent(animEvent);
					}
				}

			}

			// TODO: collision boxes

			// TODO: hit boxes

			// TODO: what about view anchors ?
			// TODO: what about view model names ?

		}

	}



	
}
