using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class CollisionBox {

		public List<Box> boxesPerFrame;

		// This is a temporary builder variable
		// it's populated during save, and discarded at save end 
		public Storage.CollisionBox storageBox;


		public CollisionBox(int animSize = 0){
			boxesPerFrame = new List<Box>(animSize);
			storageBox = null;
		}


		public static CollisionBox LoadFromStorage(Storage.CollisionBox storageBox, Storage.Character storageCharacter){

		}


		public Storage.CollisionBox SaveToStorage(){
			Storage.CollisionBox ret = storageBox;
			storageBox = null;
			return ret;
		}

	}


}
