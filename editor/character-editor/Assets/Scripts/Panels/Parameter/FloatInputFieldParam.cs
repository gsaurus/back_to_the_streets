using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{

	public class FloatInputFieldParam : MonoBehaviour {

		public GameObject label;
		public GameObject field;

		private Text _label;
		private InputField _field;

		private GenericParameter parameter;
		private int paramItemId;
		private float minValue;
		private float maxValue;


		void Awake(){
			_label = label.GetComponent<Text>();
			_field = field.GetComponent<InputField>();
		}


		public void Setup(GenericParameter parameter, int paramItemId, string description, float minValue = 0, float maxValue = -1){
			this.parameter = parameter;
			this.paramItemId = paramItemId;
			parameter.EnsureFloatItem(paramItemId);
			if (_label == null) _label = label.GetComponent<Text>();
			if (_field == null) _field = field.GetComponent<InputField>();
			_label.text = description;
			_field.text = "" + parameter.floatsList[paramItemId];
			this.minValue = minValue;
			this.maxValue = maxValue;
		}



		public void OnChange(string text){
			float floatValue;
			bool changed = false;
			if (float.TryParse(text, out floatValue)) {
				if (floatValue < minValue) {
					floatValue = minValue;
					changed = true;
				} else if (maxValue > minValue && floatValue > maxValue) {
					floatValue = maxValue;
					changed = true;
				}
				parameter.floatsList[paramItemId] = floatValue;
				if (changed) {
					_field.text = "" + parameter.floatsList[paramItemId];
				}
			}
		}


	}

}
