using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{

	public class IntDropdownParam : MonoBehaviour {

		public GameObject label;
		public GameObject dropdown;

		private Text _label;
		private Dropdown _dropdown;

		private GenericParameter parameter;
		private int paramItemId;


		// Handy static instantiation
		public static void Instantiate(GameObject parent, GenericParameter parameter, int paramItemId, string description, string[] options){
			GameObject paramObj = GameObject.Instantiate(HUDEditor.Instance.intDropdownParam);
			paramObj.GetComponent<IntDropdownParam>().Setup(parameter, paramItemId, description, options);
			paramObj.transform.SetParent(parent.transform);
		}


		void Awake(){
			_label = label.GetComponent<Text>();
			_dropdown = dropdown.GetComponent<Dropdown>();
		}


		public void Setup(GenericParameter parameter, int paramItemId, string description, string[] options){
			this.parameter = parameter;
			this.paramItemId = paramItemId;
			parameter.EnsureIntItem(paramItemId);
			if (_label == null) _label = label.GetComponent<Text>();
			if (_dropdown == null) _dropdown = dropdown.GetComponent<Dropdown>();
			_label.text = description;
			SetupDropdown(options);
		}

		private void SetupDropdown(string[] options){
			_dropdown.ClearOptions();
			Dropdown.OptionData optionData;
			foreach (string option in options) {
				optionData = new Dropdown.OptionData(option);
				_dropdown.options.Add(optionData);
			}
			_dropdown.value = parameter.intsList[paramItemId];
			_dropdown.RefreshShownValue();
		}



		public void OnChange(int itemId){
			parameter.intsList[paramItemId] = itemId;
		}


	}

}
