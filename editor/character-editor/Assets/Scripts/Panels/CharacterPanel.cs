using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace RetroBread{

	public class CharacterPanel : MonoBehaviour {

		public GameObject nameInputField;
		public GameObject skinsPanel;
		public GameObject anchorsPanel;
		public GameObject openPanel;


		private InputField _nameInputField;


		// Use this for initialization
		void Start () {
			_nameInputField = nameInputField.GetComponent<InputField>();
			CharacterEditor.Instance.OnCharacterChangedEvent += OnCharacterChanged;
		}


		void OnCharacterChanged(){
			_nameInputField.text = CharacterEditor.Instance.character.name;
		}


		public void OnCharacterNameChanged(string newName){
			CharacterEditor.Instance.character.name = newName;
		}


		public void OnSkinsButton(){
			skinsPanel.SetActive(true);
		}

		public void OnAnchorsButton(){
			anchorsPanel.SetActive(true);
		}

		public void OnOpenButton(){
			openPanel.SetActive(true);
		}


	}

}