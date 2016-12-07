using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{
	
	public class StringListInputFieldParam : MonoBehaviour {

		public GameObject label;
		public GameObject field;

		private Text _label;
		private InputField _field;

		private GenericParameter parameter;
		private int paramItemId;


		// Handy static instantiation
		public static void Instantiate(GameObject parent, GenericParameter parameter, int paramItemId, string description){
			GameObject paramObj = GameObject.Instantiate(CharacterEditor.Instance.stringListInputFieldParam);
			paramObj.GetComponent<StringListInputFieldParam>().Setup(parameter, paramItemId, description);
			paramObj.transform.SetParent(parent.transform);
		}


		void Awake(){
			_label = label.GetComponent<Text>();
			_field = field.GetComponent<InputField>();
		}


		private string StringsListToString(List<string> stringsList){
			return string.Join(", ", stringsList.ToArray());
		}

		public void Setup(GenericParameter parameter, int paramItemId, string description){
			this.parameter = parameter;
			this.paramItemId = paramItemId;
			parameter.EnsureStringsListItem(paramItemId);
			if (_label == null) _label = label.GetComponent<Text>();
			if (_field == null) _field = field.GetComponent<InputField>();
			_label.text = description;
			_field.text = StringsListToString(parameter.stringsListList[paramItemId]);
		}



		public void OnChange(string text){
			parameter.SetStringsListFromString(paramItemId, text);
		}


	}

}
