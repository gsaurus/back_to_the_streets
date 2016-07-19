using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{


	public class AllEventsPanel : MonoBehaviour {

		public GameObject eventsList;
		public GameObject objectsList;
		public GameObject objectsDropdown;
		public GameObject editButton;
		public GameObject addButton;
		public GameObject removeButton;
		public GameObject editPanel;

		private SingleSelectionList _eventsList;
		private SingleSelectionList _objectsList;
		private Dropdown _objectsDropdown;
		private Button _editButton;
		private Button _addButton;
		private Button _removeButton;

		private Dictionary<ConditionalEvent, List<HUDObject> > allEvents;
		private List<ConditionalEvent> allEventsSorted;


		private class ConditionalEventsComparer: IComparer<ConditionalEvent>{
			private Dictionary<ConditionalEvent, List<HUDObject> > allEvents;
			public ConditionalEventsComparer(Dictionary<ConditionalEvent, List<HUDObject> > allEvents){
				this.allEvents = allEvents;
			}
			public int Compare(ConditionalEvent e1, ConditionalEvent e2){
				return allEvents[e2].Count - allEvents[e1].Count;
			}
		}


		void Awake(){
			_eventsList = eventsList.GetComponent<SingleSelectionList>();
			_objectsList = objectsList.GetComponent<SingleSelectionList>();
			_objectsDropdown = objectsDropdown.GetComponent<Dropdown>();
			_editButton = editButton.GetComponent<Button>();
			_addButton = addButton.GetComponent<Button>();
			_removeButton = removeButton.GetComponent<Button>();

			allEvents = new Dictionary<ConditionalEvent, List<HUDObject>>(new ConditionalEventComparer());
			allEventsSorted = new List<ConditionalEvent>();
		}


		void OnEnable(){
			HUDEditor.Instance.OnEventChangedEvent += RefreshPanel;
			RefreshPanel();
		}

#region refresh UI items

		void RefreshPanel(){

			if (EventEditorPanel.eventToEdit != null) {
				// an event was edited, apply it to all objects that are using it
				ApplyEventModifications();
				return; // will be refreshed later
			}

			RefreshAllEvents();
			RefreshEventsList();
			_addButton.interactable = allEvents.Count > 0;
			_editButton.interactable = allEvents.Count > 0;
			_objectsDropdown.interactable = allEvents.Count > 0;
			_removeButton.interactable = _objectsList.OptionsCount > 0;
		}


		void RefreshAllEvents(){
			allEvents.Clear();
			HUD hud = HUDEditor.Instance.hud;
			if (hud == null) return;
			foreach (HUDObject anim in hud.objects) {
				foreach (ConditionalEvent e in anim.events) {
					if (!allEvents.ContainsKey(e)) {
						allEvents.Add(e, new List<HUDObject>());
					}
					allEvents[e].Add(anim);
				}
			}
			allEventsSorted.Clear();
			allEventsSorted.AddRange(allEvents.Keys);
			allEventsSorted.Sort(new ConditionalEventsComparer(allEvents));
		}


		void RefreshEventsList(){
			List<string> newOptions = new List<string>();
			foreach (ConditionalEvent e in allEventsSorted) {
				newOptions.Add(e.ToString());
			}
			_eventsList.Options = newOptions;
		}


		void RefreshObjectsList(){
			if (allEventsSorted.Count == 0) {
				_removeButton.interactable = false;
				return;
			}
			ConditionalEvent selectedEvent = allEventsSorted[_eventsList.SelectedItem];
			List<string> newOptions = new List<string>();
			List<HUDObject> eventAnims;
			if (allEvents.TryGetValue(selectedEvent, out eventAnims)) {
				foreach (HUDObject anim in eventAnims) {
					newOptions.Add(anim.name);
				}
				_objectsList.Options = newOptions;
			}
			_removeButton.interactable = _objectsList.OptionsCount > 0;
		}


		void RefreshObjectsDropdown(){
			HUD hud = HUDEditor.Instance.hud;
			if (hud == null || allEventsSorted.Count == 0) return;
			ConditionalEvent selectedEvent = allEventsSorted[_eventsList.SelectedItem];
			_objectsDropdown.ClearOptions();
			List<HUDObject> eventAnims = allEvents[selectedEvent];
			foreach (HUDObject anim in hud.objects) {
				if (!eventAnims.Contains(anim)) {
					_objectsDropdown.options.Add(new Dropdown.OptionData(anim.name));
				}
			}
			_addButton.interactable = _objectsDropdown.options.Count > 0;
			_objectsDropdown.RefreshShownValue();
		}


#endregion



#region UI events


		public void OnEventSelected(int itemId){
			RefreshObjectsList();
			RefreshObjectsDropdown();
		}


//		void OnDropdownObjectSelected(int itemId){
//		
//		}

//		void OnListObjectSelected(int itemId){
//		
//		}



		public void OnAddButton(){

			// Find selected anim
			HUDObject selectedAnim = GetObjectFromDropdown();
			// Find selected event
			ConditionalEvent selectedEvent = allEventsSorted[_eventsList.SelectedItem];
			// Clone event and add to object
			selectedAnim.events.Add(selectedEvent.Clone());
			allEvents[selectedEvent].Add(selectedAnim);

			// Refresh UI
			RefreshObjectsList();
			RefreshObjectsDropdown();

		}



		public void OnEditButton(){
			// Ugly temporary static variable.. will do for now
			EventEditorPanel.eventToEdit = allEventsSorted[_eventsList.SelectedItem].Clone();
			editPanel.SetActive(true);
		}
			


		public void OnRemoveButton(){
			
			// Find selected anim
			HUDObject selectedAnim = GetObjectFromList();
			// Find selected event
			ConditionalEvent selectedEvent = allEventsSorted[_eventsList.SelectedItem];
			// Find the event on the anim and remove it
			ConditionalEventComparer comparer = new ConditionalEventComparer();
			for (int i = 0; i < selectedAnim.events.Count; ++i) {
				if (comparer.Equals(selectedEvent, selectedAnim.events[i])) {
					selectedAnim.events.RemoveAt(i);
					break;
				}
			}

			allEvents[selectedEvent].Remove(selectedAnim);

			// Refresh UI
			RefreshObjectsList();
			RefreshObjectsDropdown();

		}



		public void Close(){
			gameObject.SetActive(false);
			HUDEditor.Instance.RefreshEvents();
		}

#endregion


		HUDObject GetObjectFromDropdown(){
			// Find selected object
			string selectedObjectName = _objectsDropdown.options[_objectsDropdown.value].text;
			HUD hud = HUDEditor.Instance.hud;
			foreach (HUDObject anim in hud.objects) {
				if (anim.name == selectedObjectName) {
					return anim;
				}
			}
			return null;
		}

		HUDObject GetObjectFromList(){
			// Find selected object
			string selectedObjectName = _objectsList.SelectedOption;
			HUD hud = HUDEditor.Instance.hud;
			foreach (HUDObject anim in hud.objects) {
				if (anim.name == selectedObjectName) {
					return anim;
				}
			}
			return null;
		}


		void ApplyEventModifications(){
			// replace old event with new event on all objects containing it
			ConditionalEvent oldEvent = allEventsSorted[_eventsList.SelectedItem];
			ConditionalEventComparer comparer = new ConditionalEventComparer();
			foreach (HUDObject anim in allEvents[oldEvent]) {
				for (int i = 0; i < anim.events.Count; ++i) {
					if (comparer.Equals(oldEvent, anim.events[i])) {
						anim.events[i] = EventEditorPanel.eventToEdit.Clone();
						break;
					}
				}
			}
			EventEditorPanel.eventToEdit = null;
			HUDEditor.Instance.RefreshEvents();
		}

	}


}