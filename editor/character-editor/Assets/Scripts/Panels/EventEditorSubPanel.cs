using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{


	public class EventEditorSubPanel : MonoBehaviour {

		// one for conditions, other for events
		public bool isCondition;

		public GameObject paramsList;
		public GameObject removeButton;
		public GameObject typeDropdown;
		public GameObject parameterContent;

		private SingleSelectionList _paramsList;
		private Button _removeButton;
		private Dropdown _typeDropdown;

		private ParameterBuilder parameterBuilder;
		private List<GenericParameter> parameters;
		private bool isUpdating;


		void Awake(){
			_paramsList = paramsList.GetComponent<SingleSelectionList>();
			_removeButton = removeButton.GetComponent<Button>();
			_typeDropdown = typeDropdown.GetComponent<Dropdown>();
		}


		void OnEnable(){

			ConditionalEvent conditionalEvent = EventEditorPanel.eventToEdit;
			if (conditionalEvent == null) {
				conditionalEvent = CharacterEditor.Instance.CurrentEvent();
			}

			if (isCondition) {
				parameterBuilder = ConditionParameterBuilder.Instance;
				parameters = conditionalEvent.conditions;
			} else {
				parameterBuilder = EventParameterBuilder.Instance;
				parameters = conditionalEvent.events;
			}
			UpdateParamsList();
			UpdateTypesDropdown();
		}


//		void Refresh(){
//			UpdateParamsList();
//		}


		public void CheckParameterChanges(){
			if (parameters == null || parameters.Count == 0 || _paramsList.SelectedItem >= parameters.Count) {
				return;
			}
			List<string> options = _paramsList.Options;
			for (int i = 0; i < _paramsList.OptionsCount && i < parameters.Count; ++i) {
				string selectedItemName = options[i];
				string newItemName = parameterBuilder.ToString(parameters[i]);
				if (selectedItemName != newItemName) {
					UpdateParamsList();
					break;
				}
			}
		}


		private void UpdateParamsList(){
			if (parameters == null || parameters.Count == 0) {
				_paramsList.Options = new List<string>();
				_typeDropdown.interactable = false;
				_removeButton.interactable = false;
				UpdateParameter();
				return;
			}
			List<string> options = new List<string>();
			foreach (GenericParameter param in parameters) {
				options.Add(parameterBuilder.ToString(param));
			}
			int previouslySelectedItem = _paramsList.SelectedItem;
			_paramsList.Options = options;
			if (previouslySelectedItem >= options.Count) {
				previouslySelectedItem = options.Count - 1;
			}
			_paramsList.SelectedItem = previouslySelectedItem;
			_typeDropdown.interactable = true;
			_removeButton.interactable = true;
		}


		private void UpdateTypesDropdown(){
			// Cleanup
			_typeDropdown.ClearOptions();

			string[] options = parameterBuilder.TypesList();
			foreach (string option in options) {
				_typeDropdown.options.Add(new Dropdown.OptionData(option));
			}
			if (parameters != null && parameters.Count > 0) {
				_typeDropdown.value = parameters[_paramsList.SelectedItem].type;
			}
			_typeDropdown.RefreshShownValue();
		}


		private void UpdateParameter(){
			// avoid unecessary iterations
			if (isUpdating) return;
			isUpdating = true;
			// Cleanup
			foreach(Transform child in parameterContent.transform) {
				GameObject.Destroy(child.gameObject);
			}
			// Update if enabled
			if (parameters != null && _typeDropdown.interactable) {
				GenericParameter param = parameters[_paramsList.SelectedItem];
				if (param.type >= _typeDropdown.options.Count || param.type < 0) {
					RetroBread.Debug.LogError("Unknown parameter type " + param.type + ", parameter data removed");
					param = new RetroBread.Editor.GenericParameter();
				}
				_typeDropdown.value = param.type;
				parameterBuilder.Build(parameterContent, param);
			}
			isUpdating = false;
		}


		public void OnAddButton(){
			parameters.Add(new GenericParameter());
			UpdateParamsList();
			_paramsList.SelectedItem = parameters.Count - 1;

		}

		public void OnRemoveButton(){
			parameters.RemoveAt(_paramsList.SelectedItem);
			UpdateParamsList();
		}

		public void OnTypeChanged(int type){
			CheckParameterChanges();
			if (parameters[_paramsList.SelectedItem].type != type) {
				parameters[_paramsList.SelectedItem] = new GenericParameter(type);
				UpdateParameter();
				UpdateParamsList();
			}
		}


		public void OnParameterSelected(int paramId){
			CheckParameterChanges();
			UpdateParameter();
		}



	}


}
