using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class GenericParameter {

		int type;
		public int[] intsList;
		public FixedFloat[] floatsList;
		public string[] stringsList;
		public bool[] boolsList;


		public bool IsEqual(GenericParameter other){
			// warning: ugly code ahead
			if (type != other.type) return false;
			if ((intsList == null) != (other.intsList == null)) return false;
			if ((floatsList == null) != (other.floatsList == null)) return false;
			if ((stringsList == null) != (other.stringsList == null)) return false;
			if ((boolsList == null) != (other.boolsList == null)) return false;
			if (intsList != null){
				if (intsList.Length != other.intsList.Length) return false;
				for (int i = 0 ; i < intsList.Length ; ++i){
					if (intsList[i] != other.intsList[i]) return false;
				}
			}
			if (floatsList != null){
				if (floatsList.Length != other.floatsList.Length) return false;
				for (int i = 0 ; i < floatsList.Length ; ++i){
					if (floatsList[i] != other.floatsList[i]) return false;
				}
			}
			if (stringsList != null){
				if (stringsList.Length != other.stringsList.Length) return false;
				for (int i = 0 ; i < stringsList.Length ; ++i){
					if (stringsList[i] != other.stringsList[i]) return false;
				}
			}
			if (boolsList != null){
				if (boolsList.Length != other.boolsList.Length) return false;
				for (int i = 0 ; i < boolsList.Length ; ++i){
					if (boolsList[i] != other.boolsList[i]) return false;
				}
			}
			return true;
		}


		public static GenericParameter LoadFromStorage(Storage.GenericParameter storageBox){

		}

		public Storage.GenericParameter SaveToStorage(){

		}

	}

}
