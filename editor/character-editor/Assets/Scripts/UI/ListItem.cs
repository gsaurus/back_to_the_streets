using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace RetroBread.UIExtensions{

	public class ListItem : MonoBehaviour {

		private int itemId;
		private UnityEvent<int, bool> onItemSelectedCallback;

		public void Setup(int itemId, UnityEvent<int, bool> onItemSelectedCallback){
			this.itemId = itemId;
			this.onItemSelectedCallback = onItemSelectedCallback;
		}


		public void OnToggleValueChanged(bool value){
			onItemSelectedCallback.Invoke(itemId, value);
		}

	}

}
