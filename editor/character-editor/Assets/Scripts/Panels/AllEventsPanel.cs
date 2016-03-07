using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{


	public class AllEventsPanel : MonoBehaviour {

		public GameObject eventsList;
		public GameObject animationsList;
		public GameObject animationsDropdown;
		public GameObject editButton;
		public GameObject addButton;
		public GameObject removeButton;
		public GameObject editPanel;

		private SingleSelectionList _eventsList;
		private SingleSelectionList _animationsList;
		private Dropdown _animationsDropdown;
		private Button _editButton;
		private Button _addButton;
		private Button _removeButton;

		private Dictionary<ConditionalEvent, List<CharacterAnimation> > allEvents;
		private List<ConditionalEvent> allEventsSorted;


		private class ConditionalEventsComparer: IComparer<ConditionalEvent>{
			private Dictionary<ConditionalEvent, List<CharacterAnimation> > allEvents;
			public ConditionalEventsComparer(Dictionary<ConditionalEvent, List<CharacterAnimation> > allEvents){
				this.allEvents = allEvents;
			}
			public int Compare(ConditionalEvent e1, ConditionalEvent e2){
				return allEvents[e2].Count - allEvents[e1].Count;
			}
		}


		void Awake(){
			_eventsList = eventsList.GetComponent<SingleSelectionList>();
			_animationsList = animationsList.GetComponent<SingleSelectionList>();
			_animationsDropdown = animationsDropdown.GetComponent<Dropdown>();
			_editButton = editButton.GetComponent<Button>();
			_addButton = addButton.GetComponent<Button>();
			_removeButton = removeButton.GetComponent<Button>();

			allEvents = new Dictionary<ConditionalEvent, List<CharacterAnimation>>(new ConditionalEventComparer());
			allEventsSorted = new List<ConditionalEvent>();
		}


		void OnEnable(){
			CharacterEditor.Instance.OnEventChangedEvent += RefreshPanel;
			RefreshPanel();
		}

#region refresh UI items

		void RefreshPanel(){

			if (EventEditorPanel.eventToEdit != null) {
				// an event was edited, apply it to all animations that are using it
				ApplyEventModifications();
				return; // will be refreshed later
			}

			RefreshAllEvents();
			RefreshEventsList();
			_addButton.interactable = allEvents.Count > 0;
			_editButton.interactable = allEvents.Count > 0;
			_animationsDropdown.interactable = allEvents.Count > 0;
			_removeButton.interactable = _animationsList.OptionsCount > 0;
		}


		void RefreshAllEvents(){
			allEvents.Clear();
			Character character = CharacterEditor.Instance.character;
			if (character == null) return;
			foreach (CharacterAnimation anim in character.animations) {
				foreach (ConditionalEvent e in anim.events) {
					if (!allEvents.ContainsKey(e)) {
						allEvents.Add(e, new List<CharacterAnimation>());
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


		void RefreshAnimationsList(){
			if (allEventsSorted.Count == 0) {
				_removeButton.interactable = false;
				return;
			}
			ConditionalEvent selectedEvent = allEventsSorted[_eventsList.SelectedItem];
			List<string> newOptions = new List<string>();
			List<CharacterAnimation> eventAnims = allEvents[selectedEvent];
			foreach (CharacterAnimation anim in eventAnims) {
				newOptions.Add(anim.name);
			}
			_animationsList.Options = newOptions;
			_removeButton.interactable = _animationsList.OptionsCount > 0;
		}


		void RefreshAnimationsDropdown(){
			Character character = CharacterEditor.Instance.character;
			if (character == null || allEventsSorted.Count == 0) return;
			ConditionalEvent selectedEvent = allEventsSorted[_eventsList.SelectedItem];
			_animationsDropdown.ClearOptions();
			List<CharacterAnimation> eventAnims = allEvents[selectedEvent];
			foreach (CharacterAnimation anim in character.animations) {
				if (!eventAnims.Contains(anim)) {
					_animationsDropdown.options.Add(new Dropdown.OptionData(anim.name));
				}
			}
			_addButton.interactable = _animationsDropdown.options.Count > 0;
			_animationsDropdown.RefreshShownValue();
		}


#endregion



#region UI events


		public void OnEventSelected(int itemId){
			RefreshAnimationsList();
			RefreshAnimationsDropdown();
		}


//		void OnDropdownAnimationSelected(int itemId){
//		
//		}

//		void OnListAnimationSelected(int itemId){
//		
//		}



		public void OnAddButton(){

			// Find selected anim
			CharacterAnimation selectedAnim = GetAnimationFromDropdown();
			// Find selected event
			ConditionalEvent selectedEvent = allEventsSorted[_eventsList.SelectedItem];
			// Clone event and add to animation
			selectedAnim.events.Add(selectedEvent.Clone());
			allEvents[selectedEvent].Add(selectedAnim);

			// Refresh UI
			RefreshAnimationsList();
			RefreshAnimationsDropdown();

		}



		public void OnEditButton(){
			// Ugly temporary static variable.. will do for now
			EventEditorPanel.eventToEdit = allEventsSorted[_eventsList.SelectedItem].Clone();
			editPanel.SetActive(true);
		}
			


		public void OnRemoveButton(){
			
			// Find selected anim
			CharacterAnimation selectedAnim = GetAnimationFromList();
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
			RefreshAnimationsList();
			RefreshAnimationsDropdown();

		}



		public void Close(){
			gameObject.SetActive(false);
			CharacterEditor.Instance.RefreshEvents();
		}

#endregion


		CharacterAnimation GetAnimationFromDropdown(){
			// Find selected animation
			string selectedAnimationName = _animationsDropdown.options[_animationsDropdown.value].text;
			Character character = CharacterEditor.Instance.character;
			foreach (CharacterAnimation anim in character.animations) {
				if (anim.name == selectedAnimationName) {
					return anim;
				}
			}
			return null;
		}

		CharacterAnimation GetAnimationFromList(){
			// Find selected animation
			string selectedAnimationName = _animationsList.SelectedOption;
			Character character = CharacterEditor.Instance.character;
			foreach (CharacterAnimation anim in character.animations) {
				if (anim.name == selectedAnimationName) {
					return anim;
				}
			}
			return null;
		}


		void ApplyEventModifications(){
			// replace old event with new event on all animations containing it
			ConditionalEvent oldEvent = allEventsSorted[_eventsList.SelectedItem];
			ConditionalEventComparer comparer = new ConditionalEventComparer();
			foreach (CharacterAnimation anim in allEvents[oldEvent]) {
				for (int i = 0; i < anim.events.Count; ++i) {
					if (comparer.Equals(oldEvent, anim.events[i])) {
						anim.events[i] = EventEditorPanel.eventToEdit.Clone();
						break;
					}
				}
			}
			EventEditorPanel.eventToEdit = null;
			CharacterEditor.Instance.RefreshEvents();
		}

	}


}