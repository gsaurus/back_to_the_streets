using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class Character {

		public string name;
		public List<CharacterAnimation> animations;

		public List<string> viewAnchors;
		public List<string> viewModels;
		public List<string> viewPortraits;

		public Character(string name){
			this.name = name;
			animations = new List<CharacterAnimation>();
			viewAnchors = new List<string>();
			viewModels = new List<string>();
			viewPortraits = new List<string>();
		}


		public static Character LoadFromStorage(Storage.Character storageCharacter){
			// Basic data
			Character character = new Character(storageCharacter.name);
			if (storageCharacter.viewAnchors != null) {
				character.viewAnchors = new List<string>(storageCharacter.viewAnchors);
			} else {
				character.viewAnchors = new List<string>();
			}
			if (storageCharacter.viewModels != null) {
				character.viewModels = new List<string>(storageCharacter.viewModels);
			} else {
				character.viewModels = new List<string>();
			}
			if (storageCharacter.portraits != null) {
				character.viewPortraits = new List<string>(storageCharacter.portraits);
			} else {
				character.viewPortraits = new List<string>();
				for (int i = 0 ; i < character.viewModels.Count ; ++i){
					character.viewPortraits.Add("");
				}
			}
			// Populate animations
			if (storageCharacter.animations != null) {
				character.animations = new List<CharacterAnimation>(storageCharacter.animations.Length);
				foreach (Storage.CharacterAnimation storageAnimation in storageCharacter.animations) {
					character.animations.Add(CharacterAnimation.LoadFromStorage(storageAnimation, storageCharacter));
				}
			} else {
				character.animations = new List<CharacterAnimation>();
			}

			return character;
		}


		public Storage.Character SaveToStorage(){

			// Basic data
			Storage.Character storageCharacter = new Storage.Character(name);
			if (viewAnchors != null) {
				storageCharacter.viewAnchors = viewAnchors.ToArray();
			}
			if (viewModels != null) {
				storageCharacter.viewModels = viewModels.ToArray();
			}
			if (viewPortraits != null) {
				storageCharacter.portraits = viewPortraits.ToArray();
			}

			// Generate boxes, generic parameters, and imediately construct the rest of the data
			// Tricky step
			List<Box> boxes = new List<Box>();
			List<GenericParameter> genericParams = new List<GenericParameter>();
			if (animations != null) {
				foreach (CharacterAnimation anim in animations) {
					anim.BuildStorage (boxes, genericParams);
				}
			}

			// Populate boxes and generic params
			if (boxes != null) {
				storageCharacter.boxes = new Storage.Box[boxes.Count];
				for (int i = 0; i < boxes.Count; ++i) {
					storageCharacter.boxes[i] = boxes[i].SaveToStorage();
				}
			}
			if (genericParams != null) {
				storageCharacter.genericParameters = new Storage.GenericParameter[genericParams.Count];
				for (int i = 0; i < genericParams.Count; ++i) {
					storageCharacter.genericParameters[i] = genericParams[i].SaveToStorage();
				}
			}

			// Populate animations
			if (animations != null) {
				storageCharacter.animations = new Storage.CharacterAnimation[animations.Count];
				for (int i = 0; i < animations.Count; ++i) {
					storageCharacter.animations[i] = animations[i].SaveToStorage();
				}
			}

			return storageCharacter;
		}


	}


}
