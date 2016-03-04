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

		private SingleSelectionList _collisionsList;
		private Button _removeButton;
		private Toggle _enabledToggle;
		private BoxSubPanel _boxPanel;

		private bool refreshing;

		void Awake () {
			_collisionsList = collisionsList.GetComponent<SingleSelectionList>();
			_removeButton = removeButton.GetComponent<Button>();
			_enabledToggle = enabledToggle.GetComponent<Toggle>();
			_boxPanel = boxPanel.GetComponent<BoxSubPanel>();
		}

		void OnEnable(){
			CharacterEditor.Instance.OnCharacterChangedEvent += Refresh;
			CharacterEditor.Instance.OnAnimationChangedEvent += Refresh;
			CharacterEditor.Instance.OnFrameChangedEvent += OnFrameChanged;
		}



		private void SetPanelActive(bool active){
			_boxPanel.SetInteractible(active);
			_removeButton.interactable = active;
			_enabledToggle.interactable = active;
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
			OnFrameChanged();
			refreshing = false;
		}

		void OnFrameChanged(){
			Editor.CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null || currentAnim.collisionBoxes.Count == 0){
				_boxPanel.SetInteractible(false);
				return;
			}
			_boxPanel.SetInteractible(true);
			Editor.CollisionBox currentCollision = currentAnim.collisionBoxes[_collisionsList.SelectedItem];
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
		}

		public void OnCollisionSelected(int collisionId){
			Refresh();
		}

		public void OnFrameEnabled(bool enabled){
			Editor.CollisionBox currentCollision = CharacterEditor.Instance.GetCollisionBox(_collisionsList.SelectedItem);
			currentCollision.enabledFrames[CharacterEditor.Instance.SelectedFrame] = enabled;
			OnFrameChanged();
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
		}

	}


}
