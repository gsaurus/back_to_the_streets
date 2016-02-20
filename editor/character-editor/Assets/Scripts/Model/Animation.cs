using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class Animation {

		public string name;
		public List<CollisionBox> collisionBoxes;
		public List<HitBox> hitboxes;
		public List<ConditionalEvent> events;
			

		public static Animation LoadFromStorage(Storage.CharacterAnimation storageAnimation, Storage.Character storageCharacter){
			// Build collisionboxes, hitboxes and events from storageCharacter
			// No worries with performance here, get a copy to everything
		}


		public Storage.CharacterAnimation SaveToStorage(){

		}

	}


}
