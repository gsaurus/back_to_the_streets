using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class Character {

		public string name;
		public List<Animation> animations;

		public List<string> viewAnchors;
		public List<string> viewModels;

		public Character(string name){
			this.name = name;
		}


		public static Character LoadFromStorage(Storage.Character storageCharacter){
			// Basic data
			Character character = new Character(storageCharacter.name);
			character.viewAnchors = new List<string>(storageCharacter.viewAnchors);
			character.viewModels = new List<string>(storageCharacter.viewModels);
			// Populate animations
			character.animations = new List<Animation>(storageCharacter.animations.Length);
			foreach (Storage.CharacterAnimation storageAnimation in storageCharacter.animations){
				character.animations.Add( Animation.LoadFromStorage(storageAnimation, storageCharacter) );
			}

			return character;
		}


		public Storage.Character SaveToStorage(){

			// Basic data
			Storage.Character storageCharacter = new Storage.Character(name);
			storageCharacter.viewAnchors = viewAnchors.ToArray();
			storageCharacter.viewModels = viewModels.ToArray();

			// Generate boxes, generic parameters, and imediately construct the rest of the data
			// Tricky step
			List<Box> boxes = new List<Box>();
			List<GenericParameter> genericParams = new List<GenericParameter>();
			foreach (Animation anim in animations){
				anim.BuildStorage(boxes, genericParams);
			}

			// Populate boxes and generic params
			storageCharacter.boxes = new Storage.Box[boxes.Count];
			for (int i = 0 ; i < boxes.Count ; ++i){
				storageCharacter.boxes[i] = boxes[i].SaveToStorage();
			}
			storageCharacter.genericParameters = new Storage.GenericParameter[genericParams.Count];
			for (int i = 0 ; i < genericParams.Count ; ++i){
				storageCharacter.genericParameters[i] = genericParams[i].SaveToStorage();
			}

			// Populate animations
			storageCharacter.animations = new Storage.CharacterAnimation[animations.Count];
			for (int i = 0 ; i < animations.Count ; ++i){
				storageCharacter.animations[i] = animations[i].SaveToStorage();
			}

			return storageCharacter;
		}


	}


}
