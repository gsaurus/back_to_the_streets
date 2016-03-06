using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using RetroBread.UIExtensions;

[System.Serializable]
public class UnityIntEvent : UnityEvent<int>{}
[System.Serializable]
public class UnityIntBoolEvent : UnityEvent<int, bool>{}

public class SingleSelectionList : MonoBehaviour {

	public UnityIntEvent onValueChanged;

	private int selectedItem;
	public int SelectedItem{
		get { return selectedItem; }
		set {
			// TODO: do in a better way: iterate children and update
			// This is the lazy way
			selectedItem = value;
			Refresh();
		}
	}

	[SerializeField]
	private List<string> _options;
	public List<string> Options {
		// getter returns a copy
		get{ return new List<string>(_options); }
		// setter copies the list and refreshes
		set{ _options = new List<string>(value); Refresh(); }
	}


	public int OptionsCount { get { return _options.Count; } }

	public string SelectedOption {
		get {
			return _options.Count == 0 ? null : _options[selectedItem];
		}
	}


	public Transform contentObject;
	public GameObject itemsPrefab;


	public void AddOption(string option){
		_options.Add(option);
		Refresh();
	}

	public void RemoveOption(int optionId){
		_options.RemoveAt(optionId);
		Refresh();
	}

	public void RemoveSelectedOption(){
		_options.RemoveAt(selectedItem);
		Refresh();
	}


	private void onItemSelected(int itemId, bool selected){
		if (selected) {
			selectedItem = itemId;
			onValueChanged.Invoke(itemId);
		}
	}


	private void Refresh(){
		// remove all items
		foreach (Transform child in contentObject){
			GameObject.Destroy(child.gameObject);
		}

		RectTransform contentRect = contentObject.gameObject.GetComponent<RectTransform>();
		ToggleGroup toggleGroup = contentObject.gameObject.GetComponent<ToggleGroup>();
		contentRect.sizeDelta = new Vector2(0, 24 * _options.Count);
		// Recreate items
		GameObject listItemObj;
		Text itemText;
		Toggle itemToggle;
		ListItem listItemComponent;
		UnityEvent<int, bool> onItemSelectedEvent = new UnityIntBoolEvent();
		onItemSelectedEvent.AddListener(onItemSelected);
		if (selectedItem >= _options.Count){
			selectedItem = _options.Count-1;
		}
		if (selectedItem < 0) {
			selectedItem = 0;
		}
		for (int i = 0 ; i < _options.Count ; ++i){
			listItemObj = GameObject.Instantiate(itemsPrefab);
			itemToggle = listItemObj.GetComponent<Toggle>();
			listItemComponent = listItemObj.GetComponent<ListItem>();
			listItemComponent.Setup(i, onItemSelectedEvent);
			itemText = listItemObj.GetComponentInChildren<Text>();
			itemText.text = _options[i];
			listItemObj.transform.SetParent(contentRect);
			itemToggle.group = toggleGroup;

			if (i == selectedItem){
				itemToggle.isOn = true;
			}
		}

	}

	public void Start(){
		Refresh();
	}
		

}
