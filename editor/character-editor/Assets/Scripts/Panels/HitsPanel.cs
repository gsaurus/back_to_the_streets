using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;


namespace RetroBread{

	public class HitsPanel : MonoBehaviour {

		public string boxName = "hit";

		public GameObject hitsList;
		public GameObject removeButton;
		public GameObject enabledToggle;
		public GameObject boxPanel;
		public GameObject copyButton;
		public GameObject copyFramesPanel;
		public GameObject typeDropdown;
		public GameObject parameterContent;

		private SingleSelectionList _hitsList;
		private Button _removeButton;
		private Toggle _enabledToggle;
		private BoxSubPanel _boxPanel;
		private Button _copyButton;
		private Dropdown _typeDropdown;

		private bool refreshing;

		void Awake () {
			_hitsList = hitsList.GetComponent<SingleSelectionList>();
			_removeButton = removeButton.GetComponent<Button>();
			_enabledToggle = enabledToggle.GetComponent<Toggle>();
			_boxPanel = boxPanel.GetComponent<BoxSubPanel>();
			_copyButton = copyButton.GetComponent<Button>();
			_typeDropdown = typeDropdown.GetComponent<Dropdown>();
		}


		void OnEnable(){
			SetupTypeOptions();
			CharacterEditor.Instance.OnAnimationChangedEvent += Refresh;
			CharacterEditor.Instance.OnFrameChangedEvent += OnFrameChanged;
		}

		void SetupTypeOptions(){
			_typeDropdown.ClearOptions();
			string[] types = HitParameterBuilder.Instance.TypesList();
			foreach (string type in types) {
				_typeDropdown.options.Add(new Dropdown.OptionData(type));
			}
			_typeDropdown.RefreshShownValue();
		}



		private void SetPanelActive(bool active){
			_boxPanel.SetInteractible(active);
			_removeButton.interactable = active;
			_enabledToggle.interactable = active;
			_copyButton.interactable = active;
			_typeDropdown.interactable = active;
			UpdateParameter();
		}

		void Refresh(){
			// avoid loops
			if (refreshing) return;
			refreshing = true;
			int selectedItem = _hitsList.SelectedItem;
			_hitsList.Options = new List<string>();
			Editor.CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null || currentAnim.hitBoxes.Count == 0){
				SetPanelActive(false);
				refreshing = false;
				return;
			}
			SetPanelActive(true);
			// populate list of hits
			for (int i = 0 ; i < currentAnim.hitBoxes.Count ; ++i) {
				_hitsList.AddOption(boxName + " " + i);
			}
			_hitsList.SelectedItem = selectedItem;
			CharacterEditor.Instance.SelectedHitId = _hitsList.SelectedItem;
			OnFrameChanged();
			refreshing = false;
		}

		void OnFrameChanged(){
			Editor.CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null || currentAnim.hitBoxes.Count == 0){
				_boxPanel.SetInteractible(false);
				_copyButton.interactable = false;
				_typeDropdown.interactable = false;
				UpdateParameter();
				return;
			}
			Editor.HitBox currentHit = currentAnim.hitBoxes[CharacterEditor.Instance.SelectedHitId];
			int frameId = CharacterEditor.Instance.SelectedFrame;
			currentHit.EnsureBoxExists(frameId);
			Editor.Box currentBox = currentHit.boxesPerFrame[frameId];
			_enabledToggle.isOn = currentHit.enabledFrames[frameId];
			if (currentBox != null){
				_boxPanel.SetPoints(currentBox.pointOne, currentBox.pointTwo);
			}else{
				_boxPanel.SetPoints(FixedVector3.Zero, FixedVector3.Zero);
			}
			_boxPanel.SetInteractible(_enabledToggle.isOn);
			_copyButton.interactable = _enabledToggle.isOn;
			_typeDropdown.interactable = true;
			UpdateParameter();
		}

		public void OnHitSelected(int hitId){
			CharacterEditor.Instance.SelectedHitId = hitId;
			Refresh();
		}

		public void OnFrameEnabled(bool enabled){
			Editor.HitBox currentHit = CharacterEditor.Instance.CurrentHit();
			currentHit.enabledFrames[CharacterEditor.Instance.SelectedFrame] = enabled;
			OnFrameChanged();
			CharacterEditor.Instance.RefreshHits();
		}

		public void OnAddButton(){
			Editor.CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null){
				return;
			}
			currentAnim.hitBoxes.Add(new RetroBread.Editor.HitBox());
			Refresh();
			// select the recently added item
			_hitsList.SelectedItem = _hitsList.OptionsCount-1;
		}

		public void OnRemoveButton(){
			Editor.CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null){
				return;
			}
			currentAnim.hitBoxes.RemoveAt(_hitsList.SelectedItem);
			Refresh();
		}

		public void OnBoxChanged(){
			Editor.HitBox currentHit = CharacterEditor.Instance.GetHitBox(_hitsList.SelectedItem);
			currentHit.boxesPerFrame[CharacterEditor.Instance.SelectedFrame] = new Editor.Box(_boxPanel.GetPoint1(), _boxPanel.GetPoint2());
			CharacterEditor.Instance.RefreshHits();
		}


		public void OnCopyButton(){

			Editor.HitBox currentHit = CharacterEditor.Instance.GetHitBox(_hitsList.SelectedItem);
			Editor.Box originalBox = currentHit.boxesPerFrame[CharacterEditor.Instance.SelectedFrame];

			copyFramesPanel.GetComponent<CopyFramesPanel>().Setup(originalBox, _hitsList.SelectedItem, true);
			copyFramesPanel.SetActive(true);
		}


		public void OnTypeSelected(int type){
			Editor.HitBox currentHit = CharacterEditor.Instance.GetHitBox(_hitsList.SelectedItem);
			if (currentHit.param.type != type) {
				currentHit.param = new RetroBread.Editor.GenericParameter(type);
				if (!refreshing) {
					UpdateParameter();
				}
			}
		}

		private void UpdateParameter(){
			// Cleanup
			foreach(Transform child in parameterContent.transform) {
				GameObject.Destroy(child.gameObject);
			}
			// Setup if enabled
			Editor.HitBox currentHit = CharacterEditor.Instance.GetHitBox(_hitsList.SelectedItem);
			if (currentHit != null && _typeDropdown.interactable) {
				if (currentHit.param.type >= _typeDropdown.options.Count || currentHit.param.type < 0) {
					RetroBread.Debug.LogError("Unknown hit type " + currentHit.param.type + ", parameter data removed");
					currentHit.param = new RetroBread.Editor.GenericParameter();
				}
				_typeDropdown.value = currentHit.param.type;
				HitParameterBuilder.Instance.Build(parameterContent, currentHit.param);
			}
		}

	}


}
