using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class Box {
		public FixedVector3 pointOne;
		public FixedVector3 pointTwo;


		public Box(){
			// Nothing to do here
		}


		public Box(FixedVector3 pointOne, FixedVector3 pointTwo){
			this.pointOne = pointOne;
			this.pointTwo = pointTwo;
		}


		public bool IsEqual(Box other){
			return pointOne == other.pointOne && pointTwo == other.pointTwo;
		}

		public static Box LoadFromStorage(Storage.Box storageBox){
			return new Box(storageBox.pointOne, storageBox.pointTwo);
		}

		public Storage.Box SaveToStorage(){
			return new Storage.Box(pointOne, pointTwo);
		}

	}

}
