using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{
	
	public class StringInputFieldParam : MonoBehaviour {

		public GameObject label;
		public GameObject field;

		private Text _label;
		private InputField _field;

		private GenericParameter parameter;
		private int paramItemId;


		// Handy static instantiation
		public static void Instantiate(GameObject parent, GenericParameter parameter, int paramItemId, string description){
			GameObject paramObj = GameObject.Instantiate(HUDEditor.Instance.stringInputFieldParam);
			paramObj.GetComponent<StringInputFieldParam>().Setup(parameter, paramItemId, description);
			paramObj.transform.SetParent(parent.transform);
		}


		void Awake(){
			_label = label.GetComponent<Text>();
			_field = field.GetComponent<InputField>();
		}


		public void Setup(GenericParameter parameter, int paramItemId, string description){
			this.parameter = parameter;
			this.paramItemId = paramItemId;
			parameter.EnsureStringItem(paramItemId);
			if (_label == null) _label = label.GetComponent<Text>();
			if (_field == null) _field = field.GetComponent<InputField>();
			_label.text = description;
			_field.text = parameter.stringsList[paramItemId];
		}



		public void OnChange(string text){
			parameter.stringsList[paramItemId] = text;
		}


	}

}
