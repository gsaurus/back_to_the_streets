using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class GenericParameter {

		public int type;
		public List<int> intsList;
		public List<FixedFloat> floatsList;
		public List<string> stringsList;
		public List<bool> boolsList;



		public GenericParameter(int type = 0){
			this.type = type;
			this.intsList = new List<int>();
			this.floatsList = new List<FixedFloat>();
			this.stringsList = new List<string>();
			this.boolsList = new List<bool>();
		}


		public GenericParameter(int type, int[] intsList, FixedFloat[] floatsList, string[] stringsList, bool[] boolsList){
			this.type = type;
			this.intsList = new List<int>(intsList);
			this.floatsList = new List<FixedFloat>(floatsList);
			this.stringsList = new List<string>(stringsList);
			this.boolsList = new List<bool>(boolsList);
		}


		public bool IsEqual(GenericParameter other){
			// warning: ugly code ahead
			if (type != other.type) return false;
			if ((intsList == null) != (other.intsList == null)) return false;
			if ((floatsList == null) != (other.floatsList == null)) return false;
			if ((stringsList == null) != (other.stringsList == null)) return false;
			if ((boolsList == null) != (other.boolsList == null)) return false;
			if (intsList != null){
				if (intsList.Count != other.intsList.Count) return false;
				for (int i = 0 ; i < intsList.Count ; ++i){
					if (intsList[i] != other.intsList[i]) return false;
				}
			}
			if (floatsList != null){
				if (floatsList.Count != other.floatsList.Count) return false;
				for (int i = 0 ; i < floatsList.Count ; ++i){
					if (floatsList[i] != other.floatsList[i]) return false;
				}
			}
			if (stringsList != null){
				if (stringsList.Count != other.stringsList.Count) return false;
				for (int i = 0 ; i < stringsList.Count ; ++i){
					if (stringsList[i] != other.stringsList[i]) return false;
				}
			}
			if (boolsList != null){
				if (boolsList.Count != other.boolsList.Count) return false;
				for (int i = 0 ; i < boolsList.Count ; ++i){
					if (boolsList[i] != other.boolsList[i]) return false;
				}
			}
			return true;
		}


		public static GenericParameter LoadFromStorage(Storage.GenericParameter storageBox){
			return new GenericParameter(storageBox.type, storageBox.intsList, storageBox.floatsList, storageBox.stringsList, storageBox.boolsList);
		}

		public Storage.GenericParameter SaveToStorage(){
			Storage.GenericParameter param = new Storage.GenericParameter(type);
			param.intsList = intsList.ToArray();
			param.floatsList = floatsList.ToArray();
			param.stringsList = stringsList.ToArray();
			param.boolsList = boolsList.ToArray();
			return param;
		}

	}

}
