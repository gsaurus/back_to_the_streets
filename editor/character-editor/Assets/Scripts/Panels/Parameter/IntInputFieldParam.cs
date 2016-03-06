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


		void Awake(){
			_label = label.GetComponent<Text>();
			_field = field.GetComponent<InputField>();
		}


		public void Setup(GenericParameter parameter, int paramItemId, string description, int minValue = 0, int maxValue = -1){
			this.parameter = parameter;
			this.paramItemId = paramItemId;
			parameter.EnsureIntItem(paramItemId);
			if (_label == null) _label = label.GetComponent<Text>();
			if (_field == null) _field = field.GetComponent<InputField>();
			_label.text = description;
			_field.text = "" + parameter.intsList[paramItemId];
			this.minValue = minValue;
			this.maxValue = maxValue;
		}



		public void OnChange(string text){
			int intValue;
			bool changed = false;
			if (int.TryParse(text, out intValue)) {
				if (intValue < minValue) {
					intValue = minValue;
					changed = true;
				} else if (maxValue > minValue && intValue > maxValue) {
					intValue = maxValue;
					changed = true;
				}
				parameter.intsList[paramItemId] = intValue;
				if (changed) {
					_field.text = "" + intValue;
				}
			}
		}


	}

}
