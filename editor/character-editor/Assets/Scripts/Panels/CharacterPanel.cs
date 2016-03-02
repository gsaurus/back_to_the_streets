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


		private InputField _nameInputField;
		private Dropdown _animationDropdown;


		void Awake(){
			_nameInputField = nameInputField.GetComponent<InputField>();
			_animationDropdown = animationDropdown.GetComponent<Dropdown>();
			CharacterEditor.Instance.OnCharacterChangedEvent += OnCharacterChanged;
			CharacterEditor.Instance.OnAnimationChangedEvent += OnAnimationChanged;
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
			_nameInputField.text = CharacterEditor.Instance.character.name;
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


	}

}