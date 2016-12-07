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
		public List<List<int>> intsListList;
		public List<List<string>> stringsListList;



		public GenericParameter(int type = 0){
			this.type = type;
			this.intsList = new List<int>();
			this.floatsList = new List<FixedFloat>();
			this.stringsList = new List<string>();
			this.boolsList = new List<bool>();
			this.intsListList = new List<List<int>>();
			this.stringsListList = new List<List<string>>();
		}

		public GenericParameter Clone(){
			return new GenericParameter(
				type,
				intsList.ToArray(),
				floatsList.ToArray(),
				stringsList.ToArray(),
				boolsList.ToArray()
			);
		}

		public int DeepHashCode(){
			int hash = 7043;
			foreach (int i in intsList) {
				hash = hash * 547 + i.GetHashCode();
			}
			foreach (float f in floatsList) {
				hash = hash * 3779 + f.GetHashCode();
			}
			foreach (string s in stringsList) {
				hash = hash * 11939 + s.GetHashCode();
			}
			foreach (bool b in boolsList) {
				hash = hash * 199933 + b.GetHashCode();
			}
			foreach (List<int> intsListListItem in intsListList) {
				foreach (int i in intsListListItem) {
					hash = hash * 6619 + i.GetHashCode();
				}
			}
			foreach (List<string> stringsListListItem in stringsListList) {
				foreach (string s in stringsListListItem) {
					hash = hash * 7919 + s.GetHashCode();
				}
			}
			return hash;
		}


		public GenericParameter(int type, int[] intsList, FixedFloat[] floatsList, string[] stringsList, bool[] boolsList){
			this.type = type;
			this.intsList = intsList != null ? new List<int>(intsList) : new List<int>();
			this.floatsList = floatsList != null ? new List<FixedFloat>(floatsList) : new List<FixedFloat>();
			this.stringsList = stringsList != null ? new List<string>(stringsList) : new List<string>();
			this.boolsList = boolsList != null ? new List<bool>(boolsList) : new List<bool>();
			this.intsListList = new List<List<int>>();
			this.stringsListList = new List<List<string>>();
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
					if (!stringsList[i].Equals(other.stringsList[i])) return false;
				}
			}
			if (boolsList != null){
				if (boolsList.Count != other.boolsList.Count) return false;
				for (int i = 0 ; i < boolsList.Count ; ++i){
					if (boolsList[i] != other.boolsList[i]) return false;
				}
			}
			if (intsListList != null) {
				if (intsListList.Count != other.intsListList.Count) return false;
				for (int i = 0 ; i < intsListList.Count ; ++i){
					if (intsListList[i].Count != other.intsListList[i].Count) return false;
					for (int j = 0; j < intsListList[i].Count; ++j) {
						if (intsListList[i][j] != other.intsListList[i][j]) return false;
					}
				}
			}
			if (stringsListList != null) {
				if (stringsListList.Count != other.stringsListList.Count) return false;
				for (int i = 0 ; i < stringsListList.Count ; ++i){
					if (stringsListList[i].Count != other.stringsListList[i].Count) return false;
					for (int j = 0; j < stringsListList[i].Count; ++j) {
						if (!stringsListList[i][j].Equals(other.stringsListList[i][j])) return false;
					}
				}
			}
			return true;
		}


		private static List<List<int>> StorageToGenericIntsList(Storage.GenericIntsList[] storage){
            if (storage == null) return new List<List<int>>();
			List<List<int>> res = new List<List<int>>(storage.Length);
			foreach (Storage.GenericIntsList storageIntsList in storage) {
				res.Add(new List<int>(storageIntsList.list));
			}
			return res;
		}

		private static List<List<string>> StorageToGenericStringsList(Storage.GenericStringsList[] storage){
            if (storage == null) return new List<List<string>>();
			List<List<string>> res = new List<List<string>>(storage.Length);
			foreach (Storage.GenericStringsList storageStringsList in storage) {
				res.Add(new List<string>(storageStringsList.list));
			}
			return res;
		}


		public static GenericParameter LoadFromStorage(Storage.GenericParameter storageBox){
			GenericParameter newParam = new GenericParameter(storageBox.type, storageBox.intsList, storageBox.floatsList, storageBox.stringsList, storageBox.boolsList);
			newParam.intsListList = StorageToGenericIntsList(storageBox.intsListList);
			newParam.stringsListList = StorageToGenericStringsList(storageBox.stringsListList);
			return newParam;
		}

		private Storage.GenericIntsList[] GenericIntsListToStorage(){
			Storage.GenericIntsList[] res = new Storage.GenericIntsList[intsListList.Count];
			List<int> intsList;
			for (int i = 0 ; i < intsListList.Count ; ++i) {
				intsList = intsListList[i];
				res[i].list = intsList.ToArray();
			}
			return res;
		}

		private Storage.GenericStringsList[] GenericStringsListToStorage(){
			Storage.GenericStringsList[] res = new Storage.GenericStringsList[stringsListList.Count];
			List<string> stringsList;
			for (int i = 0 ; i < stringsListList.Count ; ++i) {
				stringsList = stringsListList[i];
				res[i].list = stringsList.ToArray();
			}
			return res;
		}

		public Storage.GenericParameter SaveToStorage(){
			Storage.GenericParameter param = new Storage.GenericParameter(type);
			param.intsList = intsList.ToArray();
			param.floatsList = floatsList.ToArray();
			param.stringsList = stringsList.ToArray();
			param.boolsList = boolsList.ToArray();
			param.intsListList = GenericIntsListToStorage();
			param.stringsListList = GenericStringsListToStorage();
			return param;
		}

		public void EnsureStringItem(int itemId, string defaultValue = ""){
			while (stringsList.Count <= itemId) {
				stringsList.Add(defaultValue);
			}
		}
		public void EnsureIntItem(int itemId, int defaultValue = 0){
			while (intsList.Count <= itemId) {
				intsList.Add(defaultValue);
			}
		}
		public void EnsureFloatItem(int itemId, float defaultValue = 0){
			while (floatsList.Count <= itemId) {
				floatsList.Add(defaultValue);
			}
		}
		public void EnsureBoolItem(int itemId, bool defaultValue = false){
			while (boolsList.Count <= itemId) {
				boolsList.Add(defaultValue);
			}
		}
		public void EnsureIntsListItem(int itemId){
			while (intsListList.Count <= itemId) {
				intsListList.Add(new List<int>());
			}
		}
		public void EnsureStringsListItem(int itemId){
			while (stringsListList.Count <= itemId) {
				stringsListList.Add(new List<string>());
			}
		}

		public void SetIntsListFromString(int itemId, string text, char separator = ','){
			EnsureIntsListItem(itemId);
			List<int> items = intsListList[itemId];
			items.Clear();
			text = text.Replace(" ", string.Empty);
			char[] separators = {separator};
			string[] components = text.Split(separators);
			int intItem;
			foreach (string component in components) {
				if (int.TryParse(component, out intItem)){
					items.Add(intItem);
				}
			}
		}


		public void SetStringsListFromString(int itemId, string text, char separator = ','){
			EnsureStringsListItem(itemId);
			List<string> items = stringsListList[itemId];
			items.Clear();
			char[] separators = {separator};
			string[] components = text.Split(separators);
			string stringItem = null;
			foreach (string component in components) {
				stringItem = component.Trim();
				if (stringItem != null && stringItem.Length > 0) {
					items.Add(stringItem);
				}
			}
		}


	}

}
