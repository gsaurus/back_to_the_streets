using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{
	
	public class BoolToggleParam : MonoBehaviour {

		public GameObject label;
		public GameObject toggle;

		private Text _label;
		private Toggle _toggle;

		private GenericParameter parameter;
		private int paramItemId;

		// Handy static instantiation
		public static void Instantiate(GameObject parent, GenericParameter parameter, int paramItemId, string description){
			GameObject paramObj = GameObject.Instantiate(CharacterEditor.Instance.boolToggleParam);
			paramObj.GetComponent<BoolToggleParam>().Setup(parameter, paramItemId, description);
			paramObj.transform.SetParent(parent.transform);
		}


		void Awake(){
			_label = label.GetComponent<Text>();
			_toggle = toggle.GetComponent<Toggle>();
		}


		public void Setup(GenericParameter parameter, int paramItemId, string description){
			this.parameter = parameter;
			this.paramItemId = paramItemId;
			parameter.EnsureBoolItem(paramItemId);
			if (_label == null) _label = label.GetComponent<Text>();
			if (_toggle == null) _toggle = toggle.GetComponent<Toggle>();
			_label.text = description;
			_toggle.isOn = parameter.boolsList[paramItemId];
		}



		public void OnChange(bool value){
			parameter.boolsList[paramItemId] = value;
		}


	}

}
