using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class HitBox {

		public List<Box> boxesPerFrame;
		public GenericParameter param;

		// This is a temporary builder variable
		// it's populated during save, and discarded at save end 
		public Storage.HitBox storageBox;


		public HitBox(int paramType = 0, int animSize = 0){
			boxesPerFrame = new List<Box>(animSize);
			storageBox = null;
			param = new GenericParameter(paramType);
		}


		public static HitBox LoadFromStorage(Storage.HitBox storageBox, Storage.Character storageCharacter){

		}


		public Storage.HitBox SaveToStorage(){
			Storage.HitBox ret = storageBox;
			storageBox = null;
			return ret;
		}


	}


}
