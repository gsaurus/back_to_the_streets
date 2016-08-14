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
		public GameObject animationDropdown;
		public GameObject shadowInputField;


		private InputField _nameInputField;
		private Dropdown _animationDropdown;
		private InputField _shadowInputField;


		void Awake(){
			_nameInputField = nameInputField.GetComponent<InputField>();
			_animationDropdown = animationDropdown.GetComponent<Dropdown>();
			CharacterEditor.Instance.OnCharacterChangedEvent += OnCharacterChanged;
			CharacterEditor.Instance.OnAnimationChangedEvent += OnAnimationChanged;
			_shadowInputField = shadowInputField.GetComponent<InputField>();
		}

		// Use this for initialization
		void OnEnable () {
			SetupAnimationsDropdown();
		}


		void SetupAnimationsDropdown(){
			_animationDropdown.ClearOptions();
			if (CharacterEditor.Instance.character == null) return;
			List<Editor.CharacterAnimation> animations = CharacterEditor.Instance.character.animations;
			if (animations == null) return;
			Dropdown.OptionData optionData;
			foreach (Editor.CharacterAnimation anim in animations) {
				optionData = new Dropdown.OptionData(anim.name);
				_animationDropdown.options.Add(optionData);
			}
			_animationDropdown.RefreshShownValue();
		}


		void OnCharacterChanged(){
			string name = CharacterEditor.Instance.character.name;
			string shadow = CharacterEditor.Instance.character.shadowName;
			_nameInputField.text = name != null ? name : "";
			_shadowInputField.text = shadow != null ? shadow : "";
			SetupAnimationsDropdown();
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

		public void OnSaveButton(){
			CharacterEditor.Instance.SaveCharacter();
		}

		public void OnAnimationChanged(int itemId){
			CharacterEditor.Instance.SelectedAnimationId = itemId;
		}

		public void OnAnimationChanged(){
			_animationDropdown.value = CharacterEditor.Instance.SelectedAnimationId;
			_animationDropdown.RefreshShownValue();
		}

		public void OnShadowNameChanged(string newName){
			CharacterEditor.Instance.character.shadowName = newName;
		}


	}

}