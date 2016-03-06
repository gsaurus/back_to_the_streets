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
			CharacterEditor.Instance.OnEventChangedEvent += RefreshEventsList;
			RefreshPanel();
		}

#region refresh UI items

		void RefreshPanel(){
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
			if (allEventsSorted.Count == 0) return;
			ConditionalEvent selectedEvent = allEventsSorted[_eventsList.SelectedItem];
			List<string> newOptions = new List<string>();
			List<CharacterAnimation> eventAnims = allEvents[selectedEvent];
			foreach (CharacterAnimation anim in eventAnims) {
				newOptions.Add(anim.name);
			}
			_animationsList.Options = newOptions;
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
		}


#endregion



#region UI events

		void OnEventSelected(int itemId){
			RefreshAnimationsList();
			_removeButton.interactable = _animationsList.OptionsCount > 0;
		}

		void OnDropdownAnimationSelected(int itemId){
		
		}

		void OnListAnimationSelected(int itemId){
		
		}

		void OnAddButton(){
		
		}

		void OnEditButton(){
		
		}
			
		void OnRemoveButton(){
		
		}


		void Close(){
			gameObject.SetActive(false);
		}

#endregion

	}


}