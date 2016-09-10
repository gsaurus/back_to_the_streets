using UnityEngine;
using System.IO;
using System.Collections.Generic;


namespace RetroBread{


	public static class HUDLoader{

		public static string hudDataPath;
		public static string hudModelsPath;

		public static string dataExtension = ".bytes";
		public static string prefabDelimiter = ":";

		private static string currentHudName;
		private static Storage.HUD hudData;
		private static GameObject hudModel;


		static HUDLoader(){
			if (hudDataPath == null) hudDataPath = Application.streamingAssetsPath + "/HUD/data/";
			if (hudModelsPath == null) hudModelsPath = Application.streamingAssetsPath + "/HUD/models/";
		}


		// Load Character view model, and portrait
		public static void LoadHud(string hudName){

			// Check if it'ss already loaded
			if (currentHudName != null && currentHudName == hudName){
				return;
			}

			// Load HUD Data
			Storage.HUD storageHud;
			// Open file stream and deserialize it
			FileInfo hudFile = new FileInfo(hudDataPath + hudName + dataExtension);
			FileStream hudStream = hudFile.OpenRead();
			RbStorageSerializer serializer = new RbStorageSerializer();
			storageHud = serializer.Deserialize(hudStream, null, typeof(Storage.HUD)) as Storage.HUD;
			if (storageHud != null) {
				currentHudName = hudName;
				hudData = storageHud;
			} else {
				Debug.LogError("Can't find hud data " + hudName);
				return;
			}

			// Load HUD Model
			string url = "file://" + hudModelsPath + hudName;
			WWW www = WWW.LoadFromCacheOrDownload(url, 1);
			if (www.assetBundle == null) {
				Debug.LogError("Failed to load hud bundle at " + url);
			}
			// Load HUD main prefab from bundle
			GameObject prefab = www.assetBundle.LoadAsset<GameObject>(hudData.mainPrefabName);

			// TODO: load other things, put other prefabs into dictionaries, etc

			www.assetBundle.Unload(false);

			// Remove previous hud, if any
			if (hudModel != null){
				GameObject.Destroy(hudModel);
				hudModel = null;
			}

			if (prefab != null) {
				// Add hudModel to the scene
				GameObject.Instantiate(prefab);
				hudModel = prefab;
				hudModel.AddComponent<HUDViewBehaviour>();
				HUDViewBehaviour hudView = hudModel.GetComponent<HUDViewBehaviour>();
				hudView.hudData = hudData;
			}else{
				Debug.LogError("Failed to load hud canvas " + hudData.mainPrefabName + " from bundle " + url);
			}

		}



		private static GenericEvent<HUDViewBehaviour> ReadEvent(Storage.HUD hud, Storage.GenericEvent storageEvent){
			// Build event
			GenericTriggerCondition<HUDViewBehaviour> condition = HUDConditionsBuilder.Build(hud, storageEvent.conditionIds);
			GenericEvent<HUDViewBehaviour> e = HudEventsBuilder.Build(hud, storageEvent.eventIds);
			e.condition = condition;
			return e;
		}


		
	} // class HUDLoader

} // namespace RetroBread

