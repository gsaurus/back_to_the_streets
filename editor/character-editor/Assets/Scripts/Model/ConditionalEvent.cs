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
		public Storage.CharacterEvent storageEvent;



		public ConditionalEvent(){
			conditions = new List<GenericParameter>();
			events = new List<GenericParameter>();
		}



		public static ConditionalEvent LoadFromStorage(Storage.CharacterEvent storageEvent, Storage.Character storageCharacter){
			// Populate all params
		}


		public Storage.CharacterEvent SaveToStorage(){
			Storage.CharacterEvent ret = storageEvent;
			storageEvent = null;
			return ret;
		}

			
	}


}
