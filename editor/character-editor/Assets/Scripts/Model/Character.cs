using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class Character {

		public string name;
		public List<Animation> animations;

		public string[] viewAnchors;
		public string[] viewModels;


		public static Character LoadFromStorage(Storage.Character storageCharacter){

		}


		public Storage.Character SaveToStorage(){
			
		}

	}


}
