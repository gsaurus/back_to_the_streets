using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace RetroBread{

	public class AnchorsPanel : MonoBehaviour {

		public GameObject anchorsList;
		public GameObject nameInputField;
		public GameObject addButton;
		public GameObject removeButton;


		private SingleSelectionList _anchorsList;
		private InputField _nameInputField;
		private Button _addButton;
		private Button _removeButton;


		// Use this for initialization
		void Awake() {
			_nameInputField = nameInputField.GetComponent<InputField>();
			_addButton = addButton.GetComponent<Button>();
			_removeButton = removeButton.GetComponent<Button>();
			_anchorsList = anchorsList.GetComponent<SingleSelectionList>();
		}


		void OnEnable(){
			Reset();
		}

		void Reset(){
			_anchorsList.Options = CharacterEditor.Instance.character.viewAnchors;
			UpdateAddButton();
			_removeButton.interactable = _anchorsList.OptionsCount > 0;
		}



		void UpdateAddButton(){
			_addButton.interactable =
				_nameInputField.text != null
				&& _nameInputField.text.Length > 0
				&& !_anchorsList.Options.Contains(_nameInputField.text)
			;
		}

		public void OnAnchorNameChanged(string ignoredParameter){
			UpdateAddButton();
		}

		public void OnAddButton(){
			CharacterEditor.Instance.character.viewAnchors.Add(_nameInputField.text);
			Reset();
		}

		public void OnRemoveButton(){
			CharacterEditor.Instance.character.viewAnchors.Remove(_anchorsList.SelectedOption);
			Reset();
		}

		public void Close(){
			this.gameObject.SetActive(false);
			CharacterEditor.Instance.SaveCharacter();
		}


	}

}