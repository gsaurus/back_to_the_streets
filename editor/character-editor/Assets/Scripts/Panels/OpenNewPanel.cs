using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;


namespace RetroBread{

	public class OpenNewPanel : MonoBehaviour {

		// Reference to skins panel, next panel when creating a character
		public GameObject skinsPanel;

		// References to relevant objects
		public GameObject filesList;
		public GameObject collisionsList;
		public GameObject hitsList;
		public GameObject openButton;
		public GameObject addCollisionButton;
		public GameObject removeCollisionButton;
		public GameObject addHitButton;
		public GameObject removeHitButton;
		public GameObject createButton;
		public GameObject closeButton;

		// Simplified references
		private SingleSelectionList _filesList;
		private SingleSelectionList _collisionsList;
		private SingleSelectionList _hitsList;
		// Buttons
		private Button _addCollisionButton;
		private Button _removeCollisionButton;
		private Button _addHitButton;
		private Button _removeHitButton;
		private Button _createButton;

		// Texts
		private string newCharacterName;
		private string newCollisionName;
		private string newHitName;



		// Use this for initialization
		void Awake() {
			
			// Get the scripts for buttons
			_addCollisionButton 	= addCollisionButton.GetComponent<Button>();
			_removeCollisionButton 	= removeCollisionButton.GetComponent<Button>();
			_addHitButton			= addHitButton.GetComponent<Button>();
			_removeHitButton 		= removeHitButton.GetComponent<Button>();
			_createButton 			= createButton.GetComponent<Button>();

			// Get the scripts for lists
			_filesList = filesList.GetComponent<SingleSelectionList>();
			_collisionsList = collisionsList.GetComponent<SingleSelectionList>();
			_hitsList = hitsList.GetComponent<SingleSelectionList>();

			// most buttons are initially inactive due to empty lists and input fields
			_addCollisionButton.interactable = false;
			_removeCollisionButton.interactable = false;
			_addHitButton.interactable = false;
			_removeHitButton.interactable = false;
			_createButton.interactable = false;

		}


		void OnEnable(){
			SetupFilesList();
			// can only close popup if a character is already selected
			closeButton.GetComponent<Button>().interactable = CharacterEditor.Instance.character != null;
			// can only open a character if any exists
			openButton.GetComponent<Button>().interactable = _filesList.OptionsCount > 0;
		}
			

		public void OnAddCollision(){
			_collisionsList.AddOption(newCollisionName);
			_removeCollisionButton.interactable = _collisionsList.OptionsCount > 0;
			_addCollisionButton.interactable = false; // would cause duplicate name
		}

		public void OnAddHit(){
			_hitsList.AddOption(newHitName);
			_removeHitButton.interactable = _hitsList.OptionsCount > 0;
			_addHitButton.interactable = false; // would cause duplicate name
		}

		public void OnRemoveCollision(){
			_collisionsList.RemoveOption(_collisionsList.SelectedItem);
			_removeCollisionButton.interactable = _collisionsList.OptionsCount > 0;
			OnCollisionNameChanged(newCollisionName); // refresh add button
		}

		public void OnRemoveHit(){
			_hitsList.RemoveOption(_hitsList.SelectedItem);
			_removeHitButton.interactable = _hitsList.OptionsCount > 0;
			OnHitNameChanged(newHitName); // refresh add button
		}
			

		public void OnCharacterNameChanged(string text){
			bool valid = false;
			if (text.Length > 0) {
				// check if it's a duplicate
				valid = true;
				List<string> options = _filesList.Options;
				foreach (string fileName in options) {
					if (fileName == text) {
						valid = false;
						break;
					}
				}
			}
			_createButton.interactable = valid;
			newCharacterName = text;
		}

		public void OnCollisionNameChanged(string text){
			bool valid = false;
			if (text.Length > 0) {
				// check if it's a duplicate
				valid = true;
				List<string> options = _collisionsList.Options;
				foreach (string option in options) {
					if (option == text) {
						valid = false;
						break;
					}
				}
			}
			newCollisionName = text;
			_addCollisionButton.interactable = valid;
		}

		public void OnHitNameChanged(string text){
			bool valid = false;
			if (text.Length > 0) {
				// check if it's a duplicate
				valid = true;
				List<string> options = _hitsList.Options;
				foreach (string option in options) {
					if (option == text) {
						valid = false;
						break;
					}
				}
			}

			newHitName = text;
			_addHitButton.interactable = valid;
		}



		private void SetupFilesList(){
			DirectoryInfo charsDataDir = new DirectoryInfo(CharacterEditor.charactersDataPath);
			FileInfo[] charFiles = charsDataDir.GetFiles("*.bytes");
			List<string> fileNames = new List<string>(charFiles.Length);
			foreach (FileInfo fileInfo in charFiles) {
				fileNames.Add(fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.')));
			}
			_filesList.Options = fileNames;
		}


		public void OnOpen(){
			
			CharacterEditor.Instance.LoadCharacter( _filesList.SelectedOption );
			ClosePanel();
		}


		public void OnCreateCharacter(){

			CharacterEditor.Instance.CreateCharacter(newCharacterName, _collisionsList.Options, _hitsList.Options);
			ClosePanel();

		}


		public void ClosePanel(){

			this.gameObject.SetActive(false);

			// Check if there is at least one skin
			// If not, show skins panel
			if (CharacterEditor.Instance.character.viewModels.Count == 0) {
				skinsPanel.SetActive(true);
			}

		}


	}



}