using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{


	public class EventsPanel : MonoBehaviour {

		public GameObject editButton;
		public GameObject removeButton;
		public GameObject eventsList;

		public GameObject eventEditorPanel;
		public GameObject allEventsPanel;

		private Button _editButton;
		private Button _removeButton;
		private SingleSelectionList _eventsList;

		private bool refreshing;


		void Awake(){
			_editButton = editButton.GetComponent<Button>();
			_removeButton = removeButton.GetComponent<Button>();
			_eventsList = eventsList.GetComponent<SingleSelectionList>();
		}

		void OnEnable(){
			HUDEditor.Instance.OnObjectChangedEvent += RefreshEventsList;
			HUDEditor.Instance.OnEventChangedEvent += RefreshEventsList;

//			List<string> newOptions = new List<string>();
//			newOptions.Add(Directory.GetCurrentDirectory());
//			newOptions.Add(Application.dataPath);
//			newOptions.Add(Application.persistentDataPath);
//			newOptions.Add(Application.temporaryCachePath);
//			newOptions.Add(Application.streamingAssetsPath);
//			_eventsList.Options = newOptions;
		}


		void RefreshEventsList(){
			HUDObject currentObj = HUDEditor.Instance.CurrentObject();
			if (currentObj == null || currentObj.events.Count == 0) {
				_eventsList.Options = new List<string>();
				_removeButton.interactable = false;
				_editButton.interactable = false;
				return;
			}
			List<string> newEvents = new List<string>();
			foreach (ConditionalEvent e in currentObj.events) {
				newEvents.Add(e.ToString());
			}
			int currentSelection = HUDEditor.Instance.SelectedEventId;
			_eventsList.Options = newEvents;
			_eventsList.SelectedItem = currentSelection;
			_removeButton.interactable = true;
			_editButton.interactable = true;
		}

		public void OnEventSelectionChanged(int itemId){
			HUDEditor.Instance.SelectedEventId = itemId;
		}

		public void OnAddButton(){
			HUDObject currentObj = HUDEditor.Instance.CurrentObject();
			currentObj.events.Add(new ConditionalEvent());
			HUDEditor.Instance.SelectedEventId = currentObj.events.Count-1;
			RefreshEventsList();
			OnEditButton();
		}

		public void OnRemoveButton(){
			HUDObject currentObj = HUDEditor.Instance.CurrentObject();
			currentObj.events.RemoveAt(HUDEditor.Instance.SelectedEventId);
			if (currentObj.events.Count == 0) {
				HUDEditor.Instance.SelectedEventId = 0;
			}else if (HUDEditor.Instance.SelectedEventId >= currentObj.events.Count) {
				HUDEditor.Instance.SelectedEventId = currentObj.events.Count - 1;
			}
			RefreshEventsList();
		}

		public void OnEditButton(){
			eventEditorPanel.SetActive(true);
		}

		public void OnAllEventsButton(){
			allEventsPanel.SetActive(true);
		}


	}



}
