using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class ConditionalEvent {

		public List<GenericParameter> conditions;
		public List<GenericParameter> events;

		// This is a temporary builder variable
		// it's populated during save, and discarded at save end 
		private Storage.CharacterEvent storageEvent;



		public ConditionalEvent(){
			conditions = new List<GenericParameter>();
			events = new List<GenericParameter>();
			storageEvent = null;
		}






		public static ConditionalEvent LoadFromStorage(Storage.CharacterEvent storageEvent, Storage.Character storageCharacter){

			ConditionalEvent newEvent = new ConditionalEvent();

			// Populate conditions
			newEvent.conditions = new List<GenericParameter>(storageEvent.conditionIds.Length);
			foreach (int conditionId in storageEvent.conditionIds){
				newEvent.conditions.Add(GenericParameter.LoadFromStorage(storageCharacter.genericParameters[conditionId]));
			}

			// Populate events
			newEvent.events = new List<GenericParameter>(storageEvent.eventIds.Length);
			foreach (int eventId in storageEvent.eventIds){
				newEvent.events.Add(GenericParameter.LoadFromStorage(storageCharacter.genericParameters[eventId]));
			}

			return newEvent;
		}


		public Storage.CharacterEvent SaveToStorage(){
			Storage.CharacterEvent ret = storageEvent;
			storageEvent = null;
			return ret;
		}


		public void BuildStorage(List<Box> boxes, List<GenericParameter> genericParams){
			storageEvent = new Storage.CharacterEvent();
			storageEvent.conditionIds = new int[conditions.Count];
			storageEvent.eventIds = new int[events.Count];
			GenericParameter searchParam;
			int paramIndex;
			for (int i = 0 ; i < conditions.Count ; ++i){
				searchParam = conditions[i];
				paramIndex = genericParams.FindIndex(x => x.IsEqual(searchParam));
				if (paramIndex < 0){
					paramIndex = genericParams.Count;
					genericParams.Add(searchParam);
				}
				storageEvent.conditionIds[i] = paramIndex;
			}
			for (int i = 0 ; i < events.Count ; ++i){
				searchParam = events[i];
				paramIndex = genericParams.FindIndex(x => x.IsEqual(searchParam));
				if (paramIndex < 0){
					paramIndex = genericParams.Count;
					genericParams.Add(searchParam);
				}
				storageEvent.eventIds[i] = paramIndex;
			}
		}

			
	}


}
