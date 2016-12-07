using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{
	
	public class IntListInputFieldParam : MonoBehaviour {

		public GameObject label;
		public GameObject field;

		private Text _label;
		private InputField _field;

		private GenericParameter parameter;
		private int paramItemId;


		// Handy static instantiation
		public static void Instantiate(GameObject parent, GenericParameter parameter, int paramItemId, string description){
			GameObject paramObj = GameObject.Instantiate(CharacterEditor.Instance.intListInputFieldParam);
			paramObj.GetComponent<IntListInputFieldParam>().Setup(parameter, paramItemId, description);
			paramObj.transform.SetParent(parent.transform);
		}


		void Awake(){
			_label = label.GetComponent<Text>();
			_field = field.GetComponent<InputField>();
		}


		private string IntsListToString(List<int> intsList){
			List<string> stringsList = new List<string>(intsList.Count);
			foreach (int intItem in intsList) {
				stringsList.Add(intItem + "");
			}
			return string.Join(", ", stringsList.ToArray());
		}

		public void Setup(GenericParameter parameter, int paramItemId, string description){
			this.parameter = parameter;
			this.paramItemId = paramItemId;
			parameter.EnsureIntsListItem(paramItemId);
			if (_label == null) _label = label.GetComponent<Text>();
			if (_field == null) _field = field.GetComponent<InputField>();
			_label.text = description;
			_field.text = IntsListToString(parameter.intsListList[paramItemId]);
		}



		public void OnChange(string text){
			parameter.SetIntsListFromString(paramItemId, text);
		}


	}

}
