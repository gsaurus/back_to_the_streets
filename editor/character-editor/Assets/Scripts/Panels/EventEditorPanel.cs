using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{


	public class EventEditorPanel : MonoBehaviour {

		public static ConditionalEvent eventToEdit = null;

		public void Close(){
			gameObject.SetActive(false);
			CharacterEditor.Instance.RefreshEvents();
		}
	

	}


}
