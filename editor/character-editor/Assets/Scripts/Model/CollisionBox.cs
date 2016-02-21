using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class CollisionBox {

		public List<bool> enabledFrames;
		public List<Box> boxesPerFrame;

		// This is a temporary builder variable
		// it's populated during save, and discarded at save end 
		private Storage.CollisionBox storageBox;


		public CollisionBox(){
			// Nothing to do here
		}


		public static CollisionBox LoadFromStorage(Storage.CollisionBox storageBox, Storage.Character storageCharacter){

			CollisionBox newBox = new CollisionBox();

			// Populate boxes
			newBox.boxesPerFrame = new List<Box>(storageBox.boxIds.Length);
			newBox.enabledFrames = new List<bool>(storageBox.boxIds.Length); 
			Box addedBox;
			foreach(int boxId in storageBox.boxIds){
				addedBox = Box.LoadFromStorage(storageCharacter.boxes[boxId]);
				newBox.boxesPerFrame.Add(addedBox);
				newBox.enabledFrames.Add(addedBox != null);
			}

			return newBox;

		}


		public Storage.CollisionBox SaveToStorage(){
			Storage.CollisionBox ret = storageBox;
			storageBox = null;
			return ret;
		}


		public void BuildStorage(List<Box> boxes, List<GenericParameter> genericParams){
			storageBox = new Storage.CollisionBox();
			storageBox.boxIds = new int[boxesPerFrame.Count];
			Box searchBox;
			int boxIndex;
			for (int i = 0 ; i < boxesPerFrame.Count ; ++i){
				searchBox = boxesPerFrame[i];
				if (!enabledFrames[i] || searchBox == null) {
					storageBox.boxIds[i] = -1;
				}else{
					boxIndex = boxes.FindIndex(x => x.IsEqual(searchBox));
					if (boxIndex < 0){
						boxIndex = boxes.Count;
						boxes.Add(searchBox);
					}
					storageBox.boxIds[i] = boxIndex;
				}
			}
		}

	}


}
