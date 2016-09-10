using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class HUD {

		public string bundleName;
		public string rootCanvas;

		public List<HUDObject> objects;

		public HUD(string bundleName){
			this.bundleName = bundleName;
			objects = new List<HUDObject>();
		}


		public static HUD LoadFromStorage(string bundleName, Storage.HUD storageHud){

			HUD hud = new HUD(bundleName);

			// Populate objects
			if (storageHud.objects != null) {
				hud.objects = new List<HUDObject>(storageHud.objects.Length);
				foreach (Storage.HUDObject storageObject in storageHud.objects) {
					hud.objects.Add(HUDObject.LoadFromStorage(storageObject, storageHud));
				}
			} else {
				hud.objects = new List<HUDObject>();
			}

			return hud;
		}


		public Storage.HUD SaveToStorage(){

			Storage.HUD storageHud = new Storage.HUD();
			storageHud.mainPrefabName = rootCanvas;


			// Generate generic parameters, and imediately construct the rest of the data
			// Tricky step
			List<GenericParameter> genericParams = new List<GenericParameter>();
			if (objects != null) {
				foreach (HUDObject hudObj in objects) {
					hudObj.BuildStorage(genericParams);
				}
			}

			// Populate generic params
			if (genericParams != null) {
				storageHud.genericParameters = new Storage.GenericParameter[genericParams.Count];
				for (int i = 0; i < genericParams.Count; ++i) {
					storageHud.genericParameters[i] = genericParams[i].SaveToStorage();
				}
			}


			// Populate objects
			if (objects != null) {
				storageHud.objects = new Storage.HUDObject[objects.Count];
				for (int i = 0; i < objects.Count; ++i) {
					storageHud.objects[i] = objects[i].SaveToStorage();
				}
			}

			return storageHud;
		}


	}


}
