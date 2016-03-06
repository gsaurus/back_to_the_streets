using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{

	public abstract class BoxesViewer : MonoBehaviour {

		private List<GameObject> boxObjects = new List<GameObject>();

		protected Color unselectedColor;
		protected Color selectedColor;
		protected Material viewerMaterial;



		protected void EnsureGameObject(int itemId){
			GameObject obj;
			while (boxObjects.Count <= itemId) {
				obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
				obj.GetComponent<Renderer>().material = viewerMaterial;
				boxObjects.Add(obj);
			}
			boxObjects[itemId].SetActive(true);
		}

		protected void DisableUnusedBoxes(int numUsedBoxes){
			for (int i = numUsedBoxes; i < boxObjects.Count; ++i) {
				boxObjects[i].SetActive(false);
			}
		}


		protected void UpdateBox(int boxId, Editor.Box box, bool isSelected){
			GameObject obj = boxObjects[boxId];
			Vector3 scale = (box.pointTwo - box.pointOne).AsVector3();
			Vector3 position = box.pointOne.AsVector3() + scale * 0.5f;
			obj.transform.localScale = scale;
			obj.transform.localPosition = position;
			obj.GetComponent<Renderer>().material.color = isSelected ? selectedColor : unselectedColor;
		}


		protected abstract void Refresh();

	}

}
