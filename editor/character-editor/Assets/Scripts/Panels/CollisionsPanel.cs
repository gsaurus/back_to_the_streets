using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;


namespace RetroBread{

	public class CollisionsPanel : MonoBehaviour {

		public string boxName = "collision";

		public GameObject collisionsList;
		public GameObject removeButton;
		public GameObject enabledToggle;
		public GameObject boxPanel;
		public GameObject copyButton;
		public GameObject copyFramesPanel;

		private SingleSelectionList _collisionsList;
		private Button _removeButton;
		private Toggle _enabledToggle;
		private BoxSubPanel _boxPanel;
		private Button _copyButton;

		private bool refreshing;

		void Awake () {
			_collisionsList = collisionsList.GetComponent<SingleSelectionList>();
			_removeButton = removeButton.GetComponent<Button>();
			_enabledToggle = enabledToggle.GetComponent<Toggle>();
			_boxPanel = boxPanel.GetComponent<BoxSubPanel>();
			_copyButton = copyButton.GetComponent<Button>();
		}

		void OnEnable(){
			CharacterEditor.Instance.OnAnimationChangedEvent += Refresh;
			CharacterEditor.Instance.OnFrameChangedEvent += OnFrameChanged;
		}



		private void SetPanelActive(bool active){
			_boxPanel.SetInteractible(active);
			_removeButton.interactable = active;
			_enabledToggle.interactable = active;
			_copyButton.interactable = active;
		}

		void Refresh(){
			// avoid loops
			if (refreshing) return;
			refreshing = true;
			int selectedItem = _collisionsList.SelectedItem;
			_collisionsList.Options = new List<string>();
			Editor.CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null || currentAnim.collisionBoxes.Count == 0){
				SetPanelActive(false);
				refreshing = false;
				return;
			}
			SetPanelActive(true);
			// populate list of collisions
			for (int i = 0 ; i < currentAnim.collisionBoxes.Count ; ++i) {
				_collisionsList.AddOption(boxName + " " + i);
			}
			_collisionsList.SelectedItem = selectedItem;
			CharacterEditor.Instance.SelectedCollisionId = _collisionsList.SelectedItem;
			OnFrameChanged();
			refreshing = false;
		}

		void OnFrameChanged(){
			Editor.CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null || currentAnim.collisionBoxes.Count == 0){
				_boxPanel.SetInteractible(false);
				_copyButton.interactable = false;
				return;
			}
			Editor.CollisionBox currentCollision = currentAnim.collisionBoxes[CharacterEditor.Instance.SelectedCollisionId];
			int frameId = CharacterEditor.Instance.SelectedFrame;
			currentCollision.EnsureBoxExists(frameId);
			Editor.Box currentBox = currentCollision.boxesPerFrame[frameId];
			_enabledToggle.isOn = currentCollision.enabledFrames[frameId];
			if (currentBox != null){
				_boxPanel.SetPoints(currentBox.pointOne, currentBox.pointTwo);
			}else{
				_boxPanel.SetPoints(FixedVector3.Zero, FixedVector3.Zero);
			}
			_boxPanel.SetInteractible(_enabledToggle.isOn);
			_copyButton.interactable = _enabledToggle.isOn;
		}

		public void OnCollisionSelected(int collisionId){
			CharacterEditor.Instance.SelectedCollisionId = collisionId;
			Refresh();
		}

		public void OnFrameEnabled(bool enabled){
			Editor.CollisionBox currentCollision = CharacterEditor.Instance.CurrentCollision();
			currentCollision.enabledFrames[CharacterEditor.Instance.SelectedFrame] = enabled;
			OnFrameChanged();
			CharacterEditor.Instance.RefreshCollisions();
		}

		public void OnAddButton(){
			Editor.CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null){
				return;
			}
			currentAnim.collisionBoxes.Add(new RetroBread.Editor.CollisionBox());
			Refresh();
			// select the recently added item
			_collisionsList.SelectedItem = _collisionsList.OptionsCount-1;
		}

		public void OnRemoveButton(){
			Editor.CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null){
				return;
			}
			currentAnim.collisionBoxes.RemoveAt(_collisionsList.SelectedItem);
			Refresh();
		}

		public void OnBoxChanged(){
			Editor.CollisionBox currentCollision = CharacterEditor.Instance.GetCollisionBox(_collisionsList.SelectedItem);
			currentCollision.boxesPerFrame[CharacterEditor.Instance.SelectedFrame] = new Editor.Box(_boxPanel.GetPoint1(), _boxPanel.GetPoint2());
			CharacterEditor.Instance.RefreshCollisions();
		}


		public void OnCopyButton(){

			Editor.CollisionBox currentCollision = CharacterEditor.Instance.GetCollisionBox(_collisionsList.SelectedItem);
			Editor.Box originalBox = currentCollision.boxesPerFrame[CharacterEditor.Instance.SelectedFrame];

			copyFramesPanel.GetComponent<CopyFramesPanel>().Setup(originalBox, _collisionsList.SelectedItem, false);
			copyFramesPanel.SetActive(true);
		}

	}


}
