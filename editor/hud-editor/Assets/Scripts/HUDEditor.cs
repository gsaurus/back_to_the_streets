using UnityEngine;
using System.Collections.Generic;
using System.IO;
using RetroBread;
using RetroBread.Editor;

namespace RetroBread{

	// Class to hold handy type conversions to/from unity
	public static class UnityExtensions{

		public static UnityEngine.Vector3 AsVector3(this FixedVector3 src){
			return new UnityEngine.Vector3((float)src.X, (float)src.Y, (float)src.Z);
		}

		public static FixedVector3 AsFixedVetor3(this UnityEngine.Vector3 src){
			return new FixedVector3(src.x, src.y, src.z);
		}

		public static Transform FindDeepChild(this Transform aParent, string aName){
			foreach(Transform child in aParent){
				if(child.name == aName )
					return child;
				Transform result = child.FindDeepChild(aName);
				if (result != null)
					return result;
			}
			return null;
		}

	}

	// Central class
	// Other editor classes use it to access data and model, and set selections
	public class HUDEditor : SingletonMonoBehaviour<HUDEditor> {

		public static string hudsDataPath;
		public static string hudsModelsPath;

		public static string dataExtension = ".bytes";


		// The hud being edited
		public HUD hud { get ; private set; }

		// The hud canvas (visible in the editor?)
		public GameObject hudModel;

		// References to param UI prefabs
		public GameObject boolToggleParam;
		public GameObject floatInputFieldParam;
		public GameObject intDropdownParam;
		public GameObject intInputFieldParam;
		public GameObject stringDropdownParam;
		public GameObject stringInputFieldParam;


		// Events when something changes
		public delegate void OnSomethingChanged();
		public event OnSomethingChanged OnHUDChangedEvent;
		public event OnSomethingChanged OnRootCanvasChangedEvent;
		public event OnSomethingChanged OnObjectChangedEvent;
		public event OnSomethingChanged OnEventChangedEvent;

		public List<GameObject> canvasList = new List<GameObject>();


		// Editor selections
		private int selectedObjectId;
		public int SelectedObjectId{
			get { return selectedObjectId; }
			set {
				if (selectedObjectId != value) {
					selectedObjectId = value;
					if (OnObjectChangedEvent != null) {
						OnObjectChangedEvent();
					}
				}
			}
		}

		private int selectedEventId;
		public int SelectedEventId{
			get{ return selectedEventId; }
			set{
				if (selectedEventId != value) {
					selectedEventId = value;
					if (OnEventChangedEvent != null) {
						OnEventChangedEvent();
					}
				}
			}
		}

		static HUDEditor(){
			// Setup debug
			RetroBread.Debug.Instance = new UnityDebug();
		}

		void Awake(){
			// Windows fail on loading assets from outside directories.. permission issues?..
//			if (hudsDataPath == null) hudsDataPath = Directory.GetCurrentDirectory() + "/Data/HUD/Data/";
//			if (hudsModelsPath == null) hudsModelsPath = Directory.GetCurrentDirectory() + "/Data/HUD/Models/";
			if (hudsDataPath == null) hudsDataPath = Application.streamingAssetsPath + "/HUD/Data/";
			if (hudsModelsPath == null) hudsModelsPath = Application.streamingAssetsPath + "/HUD/Models/";
			// Cache is unwanted on editor - everything changes all the time
			Caching.CleanCache();
		}
			

		void Reset(){

			LoadHudModel();

//			if (hudModel != null) {
//				GameObject.Destroy(hudModel);
//				hudModel = null;
//			}

			if (OnHUDChangedEvent 		 != null) 	OnHUDChangedEvent();
			if (OnRootCanvasChangedEvent != null) 	OnRootCanvasChangedEvent();
			if (OnObjectChangedEvent	 != null)	OnObjectChangedEvent();
			if (OnEventChangedEvent		 != null) 	OnEventChangedEvent();

		}



		public void CreateHUD(string hudName){

			// Fresh new hud
			hud = new HUD(hudName);
			SaveHud();

			Reset();
		}


		// Load
		public void LoadHud(string hudName){

			// Open file stream and deserialize it
			FileInfo hudFile = new FileInfo(hudsDataPath + hudName + dataExtension);
			if (hudFile.Exists){
				FileStream hudStream = hudFile.OpenRead();
				RbStorageSerializer serializer = new RbStorageSerializer();
				Storage.HUD storageHUD = serializer.Deserialize(hudStream, null, typeof(Storage.HUD)) as Storage.HUD;
				hudStream.Close();
				// Load hud into editor hud format
				hud = HUD.LoadFromStorage(hudName, storageHUD);
			}else{
				// New hud data
				hud = new HUD(hudName);
			}
			Reset();
		}


		// Save
		public void SaveHud(){
			// Convert to storage format
			Storage.HUD storageHud = hud.SaveToStorage();

			// Open file stream and serialize it
			FileInfo hudFile = new FileInfo (hudsDataPath + hud.bundleName + dataExtension);
			FileStream hudStream = hudFile.Open(FileMode.Create, FileAccess.Write);
			RbStorageSerializer serializer = new RbStorageSerializer();
			serializer.Serialize(hudStream, storageHud);
			hudStream.Close();
		}


		// Select a skin (2D or 3D model)
		public bool LoadHudModel(){
			string bundleName = hud.bundleName;
			string modelName = hud.rootCanvas;
			string url = "file://" + hudsModelsPath + bundleName;
			WWW www = WWW.LoadFromCacheOrDownload(url, 1);
			if (www.assetBundle == null) {
				Debug.LogError("Couldn't load bundle at " + url);
				return false;
			}

			// Get list of canvas prefabs
			LoadCanvasList(www.assetBundle);

			// Load current selected canvas
			if (modelName != null){
				hudModel = www.assetBundle.LoadAsset<GameObject>(modelName);
			}else{
				hudModel = null;
			}

			www.assetBundle.Unload(false);

			return hudModel != null;
		}


		private void LoadCanvasList(AssetBundle bundle){
			canvasList = new List<GameObject>();
			GameObject[] allObjects = bundle.LoadAllAssets<GameObject>();
			foreach (GameObject obj in allObjects){
				if (obj.GetComponent<Canvas>() != null){
					canvasList.Add(obj);
				}
			}
		}


#region Handy getters/setters

		public HUDObject CurrentObject(){
			if (hud == null || hud == null || hud.objects.Count == 0) {
				return null;
			}
			return hud.objects[selectedObjectId];
		}

		public ConditionalEvent CurrentEvent(){
			HUDObject currentObj = CurrentObject();
			if (currentObj == null || currentObj.events.Count == 0) {
				return null;
			}
			return currentObj.events[selectedEventId];
		}

		public void SetRootCanvas(string canvasName){
			hud.rootCanvas = canvasName;
			if (OnRootCanvasChangedEvent != null) OnRootCanvasChangedEvent();
		}

		public void SelectObjectWithName(string objectName){
			// Search for it
			int index = 0;
			foreach (HUDObject obj in hud.objects){
				if (obj.name.Equals(objectName)){
					SelectedObjectId = index;
					return;
				}
				++index;
			}
			// not found, create it
			HUDObject newHudObj = new HUDObject();
			newHudObj.name = objectName;
			hud.objects.Add(newHudObj);
			SelectedObjectId = hud.objects.Count -1;
		}

#endregion

		public void RefreshEvents(){
			OnEventChangedEvent();
		}


	}

}
