﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class CharacterAnimation {

		public string name;
		public int numFrames;
		public List<CollisionBox> collisionBoxes;
		public List<HitBox> hitBoxes;
		public List<ConditionalEvent> events;


		public CharacterAnimation(string name, int numFrames){
			this.name = name;
			this.numFrames = numFrames;
		}

		public static CharacterAnimation LoadFromStorage(Storage.CharacterAnimation storageAnimation, Storage.Character storageCharacter){
			// Build collisionboxes, hitboxes and events from storageCharacter
			// No worries with performance here, get a copy to everything
			CharacterAnimation anim = new CharacterAnimation(storageAnimation.name, storageAnimation.numFrames);

			// Populate collision boxes
			anim.collisionBoxes = new List<CollisionBox>(storageAnimation.collisionBoxes.Length);
			foreach (Storage.CollisionBox box in storageAnimation.collisionBoxes){
				anim.collisionBoxes.Add( CollisionBox.LoadFromStorage(box, storageCharacter) );
			}

			// Populate hit boxes
			anim.hitBoxes = new List<HitBox>(storageAnimation.hitBoxes.Length);
			foreach (Storage.HitBox box in storageAnimation.hitBoxes){
				anim.hitBoxes.Add( HitBox.LoadFromStorage(box, storageCharacter) );
			}

			// Populate events
			anim.events = new List<ConditionalEvent>(storageAnimation.events.Length);
			foreach (Storage.CharacterEvent e in storageAnimation.events){
				anim.events.Add( ConditionalEvent.LoadFromStorage(e, storageCharacter) );
			}

			return anim;

		}


		public Storage.CharacterAnimation SaveToStorage(){

			Storage.CharacterAnimation storageAnimation = new Storage.CharacterAnimation(name, numFrames);
			// Populate collision boxes
			storageAnimation.collisionBoxes = new Storage.CollisionBox[collisionBoxes.Count];
			for (int i = 0 ; i < collisionBoxes.Count ; ++i){
				storageAnimation.collisionBoxes[i] = collisionBoxes[i].SaveToStorage();
			}
			// Populate hit boxes
			storageAnimation.hitBoxes = new Storage.HitBox[hitBoxes.Count];
			for (int i = 0 ; i < hitBoxes.Count ; ++i){
				storageAnimation.hitBoxes[i] = hitBoxes[i].SaveToStorage();
			}
			// Populate events
			storageAnimation.events = new Storage.CharacterEvent[events.Count];
			for (int i = 0 ; i < events.Count ; ++i){
				storageAnimation.events[i] = events[i].SaveToStorage();
			}

			return storageAnimation;

		}


		public void BuildStorage(List<Box> boxes, List<GenericParameter> genericParams){
			foreach (CollisionBox box in collisionBoxes){
				box.BuildStorage(boxes, genericParams);
			}
			foreach (HitBox box in hitBoxes){
				box.BuildStorage(boxes, genericParams);
			}
			foreach(ConditionalEvent e in events){
				e.BuildStorage(boxes, genericParams);
			}
		}

	}


}