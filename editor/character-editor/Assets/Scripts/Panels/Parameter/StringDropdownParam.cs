using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{
	
	public class StringDropdownParam : MonoBehaviour {

		public GameObject label;
		public GameObject dropdown;

		private Text _label;
		private Dropdown _dropdown;

		private GenericParameter parameter;
		private int paramItemId;


		void Awake(){
			_label = label.GetComponent<Text>();
			_dropdown = dropdown.GetComponent<Dropdown>();
		}


		public void Setup(GenericParameter parameter, int paramItemId, string description, string[] options){
			this.parameter = parameter;
			this.paramItemId = paramItemId;
			parameter.EnsureStringItem(paramItemId);
			if (_label == null) _label = label.GetComponent<Text>();
			if (_dropdown == null) _dropdown = dropdown.GetComponent<Dropdown>();
			_label.text = description;
			SetupDropdown(options);
		}

		private void SetupDropdown(string[] options){
			_dropdown.ClearOptions();
			Dropdown.OptionData optionData;
			string selectedItemText = parameter.stringsList[paramItemId];
			int selectedItem = 0;
			string option;
			for (int i = 0 ; i < options.Length ; ++i) {
				option = options[i];
				optionData = new Dropdown.OptionData(option);
				_dropdown.options.Add(optionData);
				if (option == selectedItemText) {
					selectedItem = i;
				}
			}
			_dropdown.value = selectedItem;
			_dropdown.RefreshShownValue();
		}



		public void OnChange(int itemId){
			parameter.stringsList[paramItemId] = _dropdown.options[itemId].text;
		}


	}

}
