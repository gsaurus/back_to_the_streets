using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace RetroBread{

	public class HUDPanel : MonoBehaviour {

		public GameObject nameInputField;
		public GameObject openPanel;
		public GameObject canvasDropdown;
		public GameObject objectsList;


		private InputField _nameInputField;
		private Dropdown _canvasDropdown;
		private SingleSelectionList _objectsList;


		void Awake(){
			_nameInputField = nameInputField.GetComponent<InputField>();
			_canvasDropdown = canvasDropdown.GetComponent<Dropdown>();
			_objectsList = objectsList.GetComponent<SingleSelectionList>();
			HUDEditor.Instance.OnHUDChangedEvent += OnHUDChanged;
			HUDEditor.Instance.OnRootCanvasChangedEvent += OnRootCanvasChangedEvent;
		}

		// Use this for initialization
		void OnEnable() {
			SetupCanvasDropdown();
			SetupObjectsList();
		}


		void SetupCanvasDropdown(){
			_canvasDropdown.ClearOptions();
			if (HUDEditor.Instance.hud == null) return;
			List<GameObject> allCanvasObjects = HUDEditor.Instance.canvasList;
			if (allCanvasObjects == null) return;
			Dropdown.OptionData optionData;
			int index = 0;
			foreach (GameObject obj in allCanvasObjects) {
				optionData = new Dropdown.OptionData(obj.name);
				_canvasDropdown.options.Add(optionData);
				Debug.Log("HUDEditor.Instance.hud.rootCanvas: " + HUDEditor.Instance.hud.rootCanvas);
				if (obj.name == HUDEditor.Instance.hud.rootCanvas){
					_canvasDropdown.value = index;
				}
				++index;
			}
			_canvasDropdown.RefreshShownValue();
		}


		void SetupObjectsList(){
			_objectsList.Options = new List<string>();

			GameObject hudModel = HUDEditor.Instance.hudModel;
			if (hudModel == null) return;

			List<string> newList = new List<string>();
			AddChildrenRecursive(newList, hudModel);
			_objectsList.Options = newList;
		}

		void AddChildrenRecursive(List<string> newList, GameObject obj){
			newList.Add(obj.name);
			foreach(Transform child in obj.transform){
				AddChildrenRecursive(newList, child.gameObject);
			}
		}
	


		void OnHUDChanged(){
			if (HUDEditor.Instance.hud != null){
				_nameInputField.text = HUDEditor.Instance.hud.bundleName;
			}else{
				_nameInputField.text = "error";
			}
			SetupCanvasDropdown();
		}

		public void OnOpenButton(){
			openPanel.SetActive(true);
		}

		public void OnSaveButton(){
			HUDEditor.Instance.SaveHud();
		}

		public void OnRootCanvasChangedEvent(int itemId){
			HUDEditor.Instance.hudModel = HUDEditor.Instance.canvasList[itemId];
			HUDEditor.Instance.SelectedObjectId = 0;
			HUDEditor.Instance.SetRootCanvas(HUDEditor.Instance.canvasList[itemId].name);
		}

		public void OnRootCanvasChangedEvent(){
			_canvasDropdown.value = _canvasDropdown.options.FindIndex(x => x.text == HUDEditor.Instance.hud.rootCanvas);
			_canvasDropdown.RefreshShownValue();
			SetupObjectsList();
		}

		public void OnObjectSelected(int itemId){
			// Look for corresponding object in hud objects
			HUDEditor.Instance.SelectObjectWithName(_objectsList.SelectedOption);
		}


	}

}