using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace RetroBread{

	public class SkinsPanel : MonoBehaviour {

		// References to relevant objects
		public GameObject bundlesList;
		public GameObject prefabsList;
		public GameObject skinsList;
		public GameObject addButton;
		public GameObject removeButton;
		public GameObject closeButton;

		// Simplified references
		private SingleSelectionList _bundlesList;
		private SingleSelectionList _prefabsList;
		private SingleSelectionList _skinsList;
		// Buttons
		private Button _addButton;
		private Button _removeButton;
		private Button _closeButton;

		// List of model prefabs
		GameObject[] prefabs;


		// Use this for initialization
		void Awake() {

			// Get the scripts for buttons
			_addButton 		= addButton.GetComponent<Button>();
			_removeButton 	= removeButton.GetComponent<Button>();
			_closeButton	= closeButton.GetComponent<Button>();

			// Get the scripts for lists
			_bundlesList = bundlesList.GetComponent<SingleSelectionList>();
			_prefabsList = prefabsList.GetComponent<SingleSelectionList>();
			_skinsList 	 = skinsList.GetComponent<SingleSelectionList>();

		}

		void OnEnable(){
			// most buttons are initially inactive due to empty lists and input fields
			_addButton.interactable = false;
			_removeButton.interactable = false;

			SetupBundlesList();
			SetupSkinsList();

			// can only close popup if a character is already selected
			_closeButton.interactable = CharacterEditor.Instance.character.viewModels.Count > 0;
		}



		private void SetupBundlesList(){
			DirectoryInfo charsModelsDir = new DirectoryInfo(CharacterEditor.charactersModelsPath);
			FileInfo[] modelFiles = charsModelsDir.GetFiles().Where(fileInfo => !fileInfo.Name.Contains(".")).ToArray<FileInfo>();
			List<string> fileNames = new List<string>(modelFiles.Length);
			foreach (FileInfo fileInfo in modelFiles) {
				fileNames.Add(fileInfo.Name);
			}
			_bundlesList.Options = fileNames;
		}
			

		private void SetupPrefabsList(string bundleName){
			WWW www = WWW.LoadFromCacheOrDownload("file://" + CharacterEditor.charactersModelsPath + bundleName, 1);
			if (www == null || www.assetBundle == null) return;
			AssetBundle bundle = www.assetBundle;

			// Load all assets to filter game objects with animations
			prefabs = bundle.LoadAllAssets<GameObject>();
			List<string> prefabNames = new List<string>(prefabs.Length);
			foreach (GameObject prefab in prefabs){
				if (prefab.GetComponent<Animator>() != null) {
					prefabNames.Add(prefab.name);
				}
			}
			_prefabsList.Options = prefabNames;

			if (prefabNames.Count == 0) {
				_addButton.interactable = false;
			}
			www.assetBundle.Unload(false);
		}

		private void SetupSkinsList(){
			List<string> modelsList = CharacterEditor.Instance.character.viewModels;
			_skinsList.Options = modelsList;
			// Refresh close & remove buttons
			bool haveSkins = _skinsList.OptionsCount > 0;
			_closeButton.interactable = haveSkins;
			_removeButton.interactable = haveSkins;
			// Refresh add button
			OnPrefabSelected(_prefabsList.SelectedItem);
		}


		public void OnBundleSelected(int itemId){
			SetupPrefabsList(_bundlesList.SelectedOption);
		}


		public void OnPrefabSelected(int itemId){
			List<string> modelsList = CharacterEditor.Instance.character.viewModels;
			_addButton.interactable =
				_prefabsList.OptionsCount > 0
				&& !modelsList.Contains(_bundlesList.SelectedOption + CharacterEditor.skinsDelimiter + _prefabsList.SelectedOption)
			;
		}



		public void OnAddButton(){
			string bundleName = _bundlesList.SelectedOption;
			string newSkin = _prefabsList.SelectedOption;
			CharacterEditor.Instance.character.viewModels.Add(bundleName + CharacterEditor.skinsDelimiter + newSkin);
			// Refresh skins list
			SetupSkinsList();
		}


		public void OnRemoveButton(){
			CharacterEditor.Instance.character.viewModels.Remove(_skinsList.SelectedOption);
			// Refresh skins list
			SetupSkinsList();
		}

		public void Close(){
			this.gameObject.SetActive(false);

			// Load skin
			string[] pathItems = _skinsList.SelectedOption.Split(CharacterEditor.skinsDelimiter.ToCharArray());
			if (pathItems != null && pathItems.Length > 1) {
				CharacterEditor.Instance.SetSkin(pathItems[0], pathItems[1]);
			}
			CharacterEditor.Instance.SaveCharacter();
		}



	}



}