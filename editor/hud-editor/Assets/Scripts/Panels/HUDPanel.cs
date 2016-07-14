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


		private InputField _nameInputField;
		private Dropdown _canvasDropdown;


		void Awake(){
			_nameInputField = nameInputField.GetComponent<InputField>();
			_canvasDropdown = canvasDropdown.GetComponent<Dropdown>();
			HUDEditor.Instance.OnHUDChangedEvent += OnHUDChanged;
			HUDEditor.Instance.OnRootCanvasChangedEvent += OnRootCanvasChangedEvent;
		}

		// Use this for initialization
		void OnEnable () {
			SetupCanvasDropdown();
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
				if (obj.name == HUDEditor.Instance.hud.rootCanvas){
					_canvasDropdown.value = index;
				}
				++index;
			}
			_canvasDropdown.RefreshShownValue();
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
			HUDEditor.Instance.hud.rootCanvas = HUDEditor.Instance.canvasList[itemId].name;
			HUDEditor.Instance.hudModel = HUDEditor.Instance.canvasList[itemId];
		}

		public void OnRootCanvasChangedEvent(){
			_canvasDropdown.value = _canvasDropdown.options.FindIndex(x => x.text == HUDEditor.Instance.hud.rootCanvas);
			_canvasDropdown.RefreshShownValue();
		}


	}

}