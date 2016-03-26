using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class Box {

		public static int invalidBoxId = -1;

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
			if (storageBox == null) return new Box(FixedVector3.Zero, FixedVector3.Zero);
			return new Box(storageBox.pointOne, storageBox.pointTwo);
		}

		public Storage.Box SaveToStorage(){
			FixedVector3 newPointOne = FixedVector3.Min(pointOne, pointTwo);
			FixedVector3 newPointTwo = FixedVector3.Max(pointOne, pointTwo);
			return new Storage.Box(newPointOne, newPointTwo);
		}

	}

}
