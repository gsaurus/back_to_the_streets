using UnityEngine;
using System.Collections.Generic;
using System.IO;
using RetroBread;
using RetroBread.Editor;

namespace RetroBread{

	// Central class
	// Other editor classes use it to access data and model, and set selections
	public class CharacterEditor : SingletonMonoBehaviour<CharacterEditor> {

		public static string charactersDataPath = Application.streamingAssetsPath + "/Characters/Data/";
		public static string charactersModelsPath = Application.streamingAssetsPath + "/Characters/Models/";

		public static string dataExtension = ".bytes";


		// The character being edited
		public Character character { get ; private set; }

		// The skin model visible in the editor
		public GameObject characterModel { get; private set; }


		// Events when something changes
		public delegate void OnSomethingChanged();
		public event OnSomethingChanged OnAnimationChangedEvent;
		public event OnSomethingChanged OnFrameChangedEvent;
		public event OnSomethingChanged OnCollisionChangedEvent;
		public event OnSomethingChanged OnHitChangedEvent;
		public event OnSomethingChanged OnEventChangedEvent;
		public event OnSomethingChanged OnSkinChangedEvent;


		// Editor selections
		private int selectedAnimationId;
		public int SelectedAnimationId {
			get{ return selectedAnimationId; }
			set{
				selectedAnimationId = value;
				if (OnAnimationChangedEvent != null) OnAnimationChangedEvent();
			}
		}
		private int selectedFrame;
		public int SelectedFrame {
			get{ return selectedFrame; }
			set{
				selectedFrame = value;
				if (OnFrameChangedEvent != null) OnFrameChangedEvent();
			}
		}
		private int selectedCollisionId;
		public int SelectedCollisionId {
			get{ return selectedCollisionId; }
			set{
				selectedCollisionId = value;
				if (OnCollisionChangedEvent != null) OnCollisionChangedEvent();
			}
		}
		private int selectedHitId;
		public int SelectedHitId {
			get{ return selectedHitId; }
			set {
				selectedHitId = value;
				if (OnHitChangedEvent != null) OnHitChangedEvent();
			}
		}
		private int selectedEventId;
		public int SelectedEventId{
			get{ return selectedEventId; }
			set{
				selectedEventId = value;
				if (OnEventChangedEvent != null) OnEventChangedEvent();
			}
		}

		// temporary import information, used once a skin is selected
		private List<string> collisionImportList;
		private List<string> hitImportList;



		static CharacterEditor(){
			// Setup debug
			RetroBread.Debug.Instance = new UnityDebug();
		}


		void Start(){
			
			// Cache is unwanted on editor - everything changes
			Caching.CleanCache();
		}


		void Reset(){
			
			// Clear skin
			if (characterModel != null) {
				GameObject.Destroy(characterModel);
				characterModel = null;
				if (OnSkinChangedEvent != null) OnSkinChangedEvent();
			}

			// Reset selections
			selectedAnimationId = 0;
			selectedFrame = 0;
			selectedCollisionId = 0;
			selectedHitId = 0;
			selectedEventId = 0;
			if (OnAnimationChangedEvent != null) OnAnimationChangedEvent();
			if (OnFrameChangedEvent != null) OnFrameChangedEvent();
			if (OnCollisionChangedEvent != null) OnCollisionChangedEvent();
			if (OnHitChangedEvent != null) OnHitChangedEvent();
			if (OnEventChangedEvent != null) OnEventChangedEvent();

			// Pick a skin
			if (character.viewModels != null && character.viewModels.Count > 0) {
				char[] delimiters = { ':' };
				string[] pathItems = character.viewModels[0].Split(delimiters);
				if (pathItems != null && pathItems.Length > 1) {
					SetSkin(pathItems[0], pathItems[1]);
				}
			}

		}



		public void CreateCharacter(string characterName, List<string> collisionImportList, List<string> hitImportList){

			// Fresh new character
			character = new Character(characterName);
			SaveCharacter();

			// store import data temporarily
			this.collisionImportList = collisionImportList;
			this.hitImportList = hitImportList;

			Reset();
		}


		private void ImportCollisionAndHitData(){
			if (characterModel == null || (collisionImportList == null && hitImportList == null)) {
				// Nothing to import
				UnityEngine.Debug.Log("No collision & attack data imported");
			}
			// TODO: find coordinates of each item in each "frame" of all animations
			// do something with collisionImportList & hitImportList

			// clear import lists
			collisionImportList = null;
			hitImportList = null;
		}




		// Load
		public void LoadCharacter(string characterName){

			// Open file stream and deserialize it
			FileInfo charFile = new FileInfo(charactersDataPath + characterName + dataExtension);
			FileStream charStream = charFile.OpenRead();
			RbStorageSerializer serializer = new RbStorageSerializer();
			Storage.Character storageCharacter = serializer.Deserialize(charStream, null, typeof(Storage.Character)) as Storage.Character;

			// Load character into editor character format
			character = Character.LoadFromStorage(storageCharacter);

			Reset();
		}


		// Save
		public void SaveCharacter(){
			// Convert to storage format
			Storage.Character storageCharacter = character.SaveToStorage();

			// Open file stream and serialize it
			FileInfo charFile = new FileInfo (charactersDataPath + storageCharacter.name + dataExtension);
			if (charFile.Exists) {
				// TODO: store backup
				charFile.Delete();
			}
			FileStream charStream = charFile.Create();
			RbStorageSerializer serializer = new RbStorageSerializer();
			serializer.Serialize(charStream, storageCharacter);
		}


		// Select a skin (2D or 3D model)
		public void SetSkin(string bundleName, string modelName){
			string url = "file://" + charactersModelsPath + bundleName;
			WWW www = WWW.LoadFromCacheOrDownload(url, 1);
			SetSkin(www, modelName);
		}

		public void SetSkin(WWW www, string modelName){
			if (characterModel != null) {
				GameObject.Destroy(characterModel);
			}
			GameObject prefab = www.assetBundle.LoadAsset(modelName) as GameObject;
			characterModel = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
			if (OnSkinChangedEvent != null) OnSkinChangedEvent();
		}



	}

}
