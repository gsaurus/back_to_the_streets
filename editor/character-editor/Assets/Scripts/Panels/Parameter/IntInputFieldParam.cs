using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{
	
	public class IntInputFieldParam : MonoBehaviour {

		public GameObject label;
		public GameObject field;

		private Text _label;
		private InputField _field;

		private GenericParameter parameter;
		private int paramItemId;
		private int minValue;
		private int maxValue;


		// Handy static instantiation
		public static void Instantiate(GameObject parent, GenericParameter parameter, int paramItemId, string description, int minValue = -1, int maxValue = -1){
			GameObject paramObj = GameObject.Instantiate(CharacterEditor.Instance.intInputFieldParam);
			paramObj.GetComponent<IntInputFieldParam>().Setup(parameter, paramItemId, description, minValue, maxValue);
			paramObj.transform.SetParent(parent.transform);
		}


		void Awake(){
			_label = label.GetComponent<Text>();
			_field = field.GetComponent<InputField>();
		}


		public void Setup(GenericParameter parameter, int paramItemId, string description, int minValue = -1, int maxValue = -1){
			this.parameter = parameter;
			this.paramItemId = paramItemId;
			this.minValue = minValue;
			this.maxValue = maxValue;
			parameter.EnsureIntItem(paramItemId);
			if (_label == null) _label = label.GetComponent<Text>();
			if (_field == null) _field = field.GetComponent<InputField>();
			_label.text = description;
			_field.text = "" + parameter.intsList[paramItemId];
		}



		public void OnChange(string text){
			int intValue;
			bool changed = false;
			bool haveMinMax = minValue != maxValue || minValue != -1;
			if (int.TryParse(text, out intValue)) {
				if (haveMinMax) {
					if (intValue < minValue) {
						intValue = minValue;
						changed = true;
					} else if (maxValue > minValue && intValue > maxValue) {
						intValue = maxValue;
						changed = true;
					}
				}
				parameter.intsList[paramItemId] = intValue;
				if (changed) {
					_field.text = "" + intValue;
				}
			}
		}


	}

}
