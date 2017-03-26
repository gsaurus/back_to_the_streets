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
			boxesPerFrame = new List<Box>();
			enabledFrames = new List<bool>(); 
		}

		// method to enforce number of boxes per frame
		public void EnsureBoxExists(int boxId){
			while (boxesPerFrame.Count < boxId + 1){
				boxesPerFrame.Add(new Box());
				enabledFrames.Add(false);
			}
		}


		public static CollisionBox LoadFromStorage(Storage.CollisionBox storageBox, Storage.Character storageCharacter){

			CollisionBox newBox = new CollisionBox();
			int[] boxIds = storageBox.boxIds;
			if (boxIds != null && boxIds.Length > 0){
				// Populate boxes
				newBox.boxesPerFrame = new List<Box>(boxIds.Length);
				newBox.enabledFrames = new List<bool>(boxIds.Length); 
				foreach(int boxId in storageBox.boxIds){
					if (boxId == Box.invalidBoxId) {
						newBox.enabledFrames.Add(false);
						newBox.boxesPerFrame.Add(new Box());
					}else {
						newBox.enabledFrames.Add(true);
						newBox.boxesPerFrame.Add(Box.LoadFromStorage(storageCharacter.boxes[boxId]));
					}
				}
			}

			return newBox;

		}


		public Storage.CollisionBox SaveToStorage(){
			Storage.CollisionBox ret = storageBox;
			storageBox = null;
			return ret;
		}


		public void BuildStorage(List<Box> boxes, int numFrames){
			storageBox = new Storage.CollisionBox();
			storageBox.boxIds = new int[numFrames];
			Box searchBox;
			int boxIndex;
			for (int i = 0 ; i < numFrames ; ++i){
				searchBox = i < boxesPerFrame.Count ? boxesPerFrame[i] : null;
				if (searchBox == null || !enabledFrames[i]) {
					storageBox.boxIds[i] = Box.invalidBoxId;
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
