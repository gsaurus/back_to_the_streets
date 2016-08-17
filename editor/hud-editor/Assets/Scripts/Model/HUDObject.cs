using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class HUDObject {

		public string name;
		public int teamId;
		public int playerId;
		public bool attackAndGrabDelegation;
		public FixedFloat visibilityTime;
		public bool usePortraitSprite;
		public bool useCharacterText;

		public List<ConditionalEvent> events;


		public HUDObject(){
			events = new List<ConditionalEvent>();
		}

		public static HUDObject LoadFromStorage(Storage.HUDObject storageHudObject, Storage.HUD storageHud){
			// Build collisionboxes, hitboxes and events from storageCharacter
			// No worries with performance here, get a copy to everything
			HUDObject hudObj = new HUDObject();

			hudObj.name = storageHudObject.name;
			hudObj.teamId = storageHudObject.teamId;
			hudObj.playerId = storageHudObject.playerId;
			hudObj.attackAndGrabDelegation = storageHudObject.attackAndGrabDelegation;
			hudObj.visibilityTime = storageHudObject.visibilityTime;
		
			// Populate events
			if (storageHudObject.events != null) {
				hudObj.events = new List<ConditionalEvent>(storageHudObject.events.Length);
				foreach (Storage.GenericEvent e in storageHudObject.events) {
					hudObj.events.Add(ConditionalEvent.LoadFromStorage(e, storageHud));
				}
			} else {
				hudObj.events = new List<ConditionalEvent>();
			}

			return hudObj;

		}


		public Storage.HUDObject SaveToStorage(){

			Storage.HUDObject storageHudObject = new Storage.HUDObject();

			storageHudObject.name = name;
			storageHudObject.teamId = teamId;
			storageHudObject.playerId = playerId;
			storageHudObject.attackAndGrabDelegation = attackAndGrabDelegation;
			storageHudObject.visibilityTime = visibilityTime;

			// Populate events
			storageHudObject.events = new Storage.GenericEvent[events.Count];
			for (int i = 0 ; i < events.Count ; ++i){
				storageHudObject.events[i] = events[i].SaveToStorage();
			}

			return storageHudObject;

		}


		public void BuildStorage( List<GenericParameter> genericParams){

			foreach(ConditionalEvent e in events){
				e.BuildStorage(genericParams);
			}

		}

	}


}
