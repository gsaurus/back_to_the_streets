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

	private int lastSelectedItem;

	[SerializeField]
	private List<string> _options;
	public List<string> options {
		private get{ return _options; }
		set{ _options = value; Refresh(); }
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


	private void onItemSelected(int itemId, bool selected){
		if (selected) {
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
		contentRect.sizeDelta = new Vector2(0, 24 * options.Count);
		// Recreate items
		GameObject listItemObj;
		Text itemText;
		Toggle itemToggle;
		ListItem listItemComponent;
		UnityEvent<int, bool> onItemSelectedEvent = new UnityIntBoolEvent();
		onItemSelectedEvent.AddListener(onItemSelected);
		if (lastSelectedItem > _options.Count){
			lastSelectedItem = _options.Count-1;
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

			if (i == lastSelectedItem){
				itemToggle.isOn = true;
			}
		}

	}

	public void Start(){
		Refresh();
	}

}
