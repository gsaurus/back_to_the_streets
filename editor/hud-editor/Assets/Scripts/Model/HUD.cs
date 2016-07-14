using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class HUD {

		public string bundleName { get; private set; }
		public string rootCanvas;

		public HUD(string bundleName){
			this.bundleName = bundleName;
		}


		public static HUD LoadFromStorage(Storage.HUD storageHud){

			HUD hud = new HUD(storageHud.mainPrefabName);

			return hud;
		}


		public Storage.HUD SaveToStorage(){

			Storage.HUD storageHud = new Storage.HUD();
			storageHud.mainPrefabName = rootCanvas;

			return storageHud;
		}


	}


}
