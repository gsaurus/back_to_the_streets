using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class CharacterLoader{

		// TODO: may store here a list of known characters.. to know what exists when creating entities



		private static AnimationEvent ReadEvent(Storage.Character charData, Storage.CharacterEvent storageEvent, out int keyFrame){
			// Build event
			AnimationTriggerCondition condition = ConditionsBuilder.Build(charData, storageEvent.conditionIds, out keyFrame);
			AnimationEvent e = EventsBuilder.Build(charData, storageEvent.eventIds);
			e.condition = condition;
			return e;
		}





		public static void SetupCharacter(Storage.Character charData){
			
			string charName = charData.name;

			// Setup each animation
			AnimationController controller;
			AnimationEvent animEvent;
			int keyFrame;

			foreach(Storage.CharacterAnimation animation in charData.animations){

				// Register controller for this animation
				controller = new AnimationController();
				AnimationsVCPool.Instance.RegisterController(charName, animation.name, controller);

				// Setup animation events
				foreach(Storage.CharacterEvent e in animation.events){
					animEvent = ReadEvent(charData, e, out keyFrame);
					if (keyFrame == ConditionsBuilder.invalidKeyframe){
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
