using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System.Collections.Generic;


namespace RetroBread{

	public class CopyFramesPanel : MonoBehaviour {

		public GameObject inputField;

		private InputField _inputField;

		private Editor.Box originalBox;
		private int listId; // collision or hit id
		private bool isHitFrame;


		void Awake () {
			_inputField = inputField.GetComponent<InputField>();
		}

		public void Setup(Editor.Box originalBox, int listId, bool isHitFrame){
			this.originalBox = originalBox;
			this.isHitFrame = isHitFrame;
			this.listId = listId;
		}

		public void Close(){
			this.gameObject.SetActive(false);
		}

		public void OnCopyButton(){
			string text = _inputField.text;
			text = text.Replace(" ", string.Empty);
			char[] separators = {','};
			string[] components = text.Split(separators);

			separators[0] = '-';
			string[] subComponents;
			List<Editor.Box> boxes;
			List<bool> enablings;
			if (isHitFrame){
				Editor.HitBox hitBox = CharacterEditor.Instance.GetHitBox(listId);
				hitBox.EnsureBoxExists(CharacterEditor.Instance.CurrentAnimation().numFrames-1);
				boxes = hitBox.boxesPerFrame;
				enablings = hitBox.enabledFrames;
			}else {
				Editor.CollisionBox collisionBox = CharacterEditor.Instance.GetCollisionBox(listId);
				collisionBox.EnsureBoxExists(CharacterEditor.Instance.CurrentAnimation().numFrames-1);
				boxes = collisionBox.boxesPerFrame;
				enablings = collisionBox.enabledFrames;
			}

			foreach (string component in components){
				subComponents = component.Split(separators);
				if (subComponents.Length == 2){
					int first;
					int last;
					if (int.TryParse(subComponents[0], out first) && int.TryParse(subComponents[1], out last)){
						first = Mathf.Clamp(first, 1, boxes.Count);
						last = Mathf.Clamp(last, first, boxes.Count);
						PopulateList(first-1, last-1, boxes, enablings);
					}
				}else{
					int frameId;
					if (int.TryParse(component, out frameId)){
						frameId = Mathf.Clamp(frameId, 1, boxes.Count);
						SetBox(frameId-1, boxes, enablings);
					}
				}
			}

			Close();
		}


		private void PopulateList(int firstFrame, int lastFrame, List<Editor.Box> boxes, List<bool> enablings){
			if (firstFrame > lastFrame || firstFrame < 0) return;
			for (int i = firstFrame; i <= lastFrame && i < boxes.Count ; ++i){
				SetBox(i, boxes, enablings);
			}
		}

		void SetBox(int boxId, List<Editor.Box> boxes, List<bool> enablings){
			boxes[boxId] = new Editor.Box(originalBox.pointOne, originalBox.pointTwo);
			enablings[boxId] = true;
		}

	}


}