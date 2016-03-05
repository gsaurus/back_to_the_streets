using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{

	public class CollisionBoxesViewer : MonoBehaviour {

		private List<GameObject> collisionObjects = new List<GameObject>();

		protected Color unselectedColor;
		protected Color selectedColor;
		protected Material viewerMaterial;

		void Awake() {
			unselectedColor = new Color(0, 0, 1, 0.5f);
			selectedColor = new Color(0, 1, 1, 0.5f);
			viewerMaterial = new Material(Shader.Find("Transparent/Diffuse"));
		}


		void OnEnable() {
			CharacterEditor.Instance.OnFrameChangedEvent += Refresh;
			CharacterEditor.Instance.OnCollisionChangedEvent += Refresh;
		}


		void EnsureGameObject(int itemId){
			GameObject obj;
			while (collisionObjects.Count <= itemId) {
				obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
				obj.GetComponent<Renderer>().material = viewerMaterial;
				collisionObjects.Add(obj);
			}
			collisionObjects[itemId].SetActive(true);
		}

		void DisableUnusedBoxes(int numUsedBoxes){
			for (int i = numUsedBoxes; i < collisionObjects.Count; ++i) {
				collisionObjects[i].SetActive(false);
			}
		}


		void UpdateBox(int boxId, Editor.Box box, bool isSelected){
			GameObject obj = collisionObjects[boxId];
			Vector3 scale = (box.pointTwo - box.pointOne).AsVector3();
			Vector3 position = box.pointOne.AsVector3() + scale * 0.5f;
			obj.transform.localScale = scale;
			obj.transform.localPosition = position;
			obj.GetComponent<Renderer>().material.color = isSelected ? selectedColor : unselectedColor;
		}


		void Refresh(){
			int numVisibleBoxes = 0;
			int currentFrame = CharacterEditor.Instance.SelectedFrame;
			CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			CollisionBox currentCollision = CharacterEditor.Instance.CurrentCollision();
			foreach (CollisionBox collision in currentAnim.collisionBoxes) {
				if (collision.enabledFrames[currentFrame]) {
					EnsureGameObject(numVisibleBoxes);
					UpdateBox(numVisibleBoxes, collision.boxesPerFrame[currentFrame], collision == currentCollision);
					++numVisibleBoxes;
				}
			}

			DisableUnusedBoxes(numVisibleBoxes);

			//
		}
	}

}
