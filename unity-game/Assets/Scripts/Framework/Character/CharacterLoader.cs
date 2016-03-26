using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace RetroBread{


	public static class CharacterLoader{

		public static string charactersDataPath;
		public static string charactersModelsPath;

		public static string dataExtension = ".bytes";

		public static string skinsDelimiter = ":";

		private static int invalidBoxId = -1;

		// Warning: TODO: keep a pool of Boxes, to use less memory

		// store here a list of known characters
		// WARNING: TODO: Doesn't have to keep this, just a bool telling loaded or not
		// all info should go to a character controller
		private static Dictionary<string, Storage.Character> loadedCharacters = new Dictionary<string, Storage.Character>();



		static CharacterLoader(){
			// Windows fail on loading assets from outside directories.. permission issues?..
//			if (charactersDataPath == null) charactersDataPath = Directory.GetCurrentDirectory() + "/Data/Characters/Data/";
//			if (charactersModelsPath == null) charactersModelsPath = Directory.GetCurrentDirectory() + "/Data/Characters/Models/";
			if (charactersDataPath == null) charactersDataPath = Application.streamingAssetsPath + "/Characters/Data/";
			if (charactersModelsPath == null) charactersModelsPath = Application.streamingAssetsPath + "/Characters/Models/";
			Caching.CleanCache();
		}


		// Get a character skin name
		public static string GetCharacterSkinName(string characterName, uint skinId){
			Storage.Character storageCharacter;
			if (loadedCharacters.TryGetValue(characterName, out storageCharacter)) {
				if (storageCharacter.viewModels != null && storageCharacter.viewModels.Length > 0){
					if (skinId >= storageCharacter.viewModels.Length) {
						skinId = 0; // reset to default skin
					}
					return storageCharacter.viewModels[skinId];
				}
			}
			return null;
		}


		// Load Character view model
		public static GameObject LoadViewModel(string prefabName){
			string[] pathItems = prefabName.Split(skinsDelimiter.ToCharArray());
			if (pathItems != null && pathItems.Length > 1) {
				string url = "file://" + charactersModelsPath + pathItems[0];
				WWW www = WWW.LoadFromCacheOrDownload(url, 1);
				if (www.assetBundle == null) {
					Debug.LogError("Failed to load bundle at " + url);
				}
				GameObject prefab = www.assetBundle.LoadAsset(pathItems[1]) as GameObject;
				www.assetBundle.Unload(false);
				if (prefab != null) {
					return prefab;
				}
				return Resources.Load(pathItems[1]) as GameObject;
			}

			return Resources.Load(prefabName) as GameObject;
		}



		// Load Character logical data
		public static void LoadCharacter(string characterName){

			Storage.Character storageCharacter;
			if (!loadedCharacters.TryGetValue(characterName, out storageCharacter)){
				// Open file stream and deserialize it
				FileInfo charFile = new FileInfo(charactersDataPath + characterName + dataExtension);
				FileStream charStream = charFile.OpenRead();
				RbStorageSerializer serializer = new RbStorageSerializer();
				storageCharacter = serializer.Deserialize(charStream, null, typeof(Storage.Character)) as Storage.Character;
				if (storageCharacter != null) {
					loadedCharacters.Add(characterName, storageCharacter);
				} else {
					Debug.LogError("Can't find character " + characterName);
				}

				// Load character into game character format (view, controller)
				SetupCharacter(storageCharacter);
			}

		}


		private static AnimationEvent ReadEvent(Storage.Character charData, Storage.CharacterEvent storageEvent, out int keyFrame, Storage.CharacterAnimation animation){
			// Build event
			AnimationTriggerCondition condition = ConditionsBuilder.Build(charData, storageEvent.conditionIds, out keyFrame, animation);
			AnimationEvent e = EventsBuilder.Build(charData, storageEvent.eventIds);
			e.condition = condition;
			return e;
		}


		private static void SetupCharacter(Storage.Character charData){
			
			string charName = charData.name;

			// Register animation view for this character
			AnimationView view = new AnimationView();
			AnimationsVCPool.Instance.SetDefaultView(charName, view);

			// Setup each animation
			AnimationController controller;
			AnimationEvent animEvent;
			int keyFrame;

			foreach(Storage.CharacterAnimation animation in charData.animations){

				// Register controller for this animation
				controller = new AnimationController();
				AnimationsVCPool.Instance.RegisterController(charName, animation.name, controller);

				// Setup animation events
				if (animation.events != null) {
					foreach (Storage.CharacterEvent e in animation.events) {
						animEvent = ReadEvent(charData, e, out keyFrame, animation);
						if (keyFrame != ConditionsBuilder.invalidKeyframe) {
							controller.AddKeyframeEvent((uint)keyFrame, animEvent);
						} else {
							controller.AddGeneralEvent(animEvent);
						}
					}
				}


				// Setup frame data
				// WARNING TODO: pool of boxes and pool of HitData
				FrameData[] framesData = new FrameData[animation.numFrames];
				FrameData frameData;
		
				// Collisions
				if (animation.collisionBoxes != null){
					Storage.Box storageBox;
					int boxIndex;
					// For each box
					foreach(Storage.CollisionBox storageCollisionBox in animation.collisionBoxes){
						// for each frame of each box
						for (int i = 0; i < storageCollisionBox.boxIds.Length ; ++i){
							boxIndex = storageCollisionBox.boxIds[i];
							if (boxIndex != invalidBoxId){
								storageBox = charData.boxes[boxIndex];
								frameData = GetFrameData(framesData, i);
								frameData.collisions.Add(new Box(storageBox.pointOne, storageBox.pointTwo));
							}
						} // each frame
					} // each storageCollisionBox
				}

				// Hits
				if (animation.hitBoxes != null){
					Storage.Box storageBox;
					Storage.GenericParameter param;
					HitData hitData;
					int boxIndex;
					// For each box
					foreach(Storage.HitBox storageHitBox in animation.hitBoxes){
						param = charData.genericParameters[storageHitBox.paramId];
						hitData = HitsBuilder.Build(param);
						if (hitData == null) continue;
						// for each frame of each box
						for (int i = 0; i < storageHitBox.boxIds.Length ; ++i){
							boxIndex = storageHitBox.boxIds[i];
							if (boxIndex != invalidBoxId){
								storageBox = charData.boxes[boxIndex];
								frameData = GetFrameData(framesData, i);
								frameData.hits.Add(new HitBox(new Box(storageBox.pointOne, storageBox.pointTwo), hitData));
							}
						} // each frame
					} // each storageHitBox
				}

				// Precompute bounding boxes
				foreach (FrameData finalFramedata in framesData) {
					finalFramedata.ComputeBoundingBoxes();
				}
				// Store frames data on animation controller
				controller.SetFramesData(framesData);

			}

			// TODO: what about view anchors ?

			// Skins?..


		}

		private static FrameData GetFrameData(FrameData[] framesData, int index){
			if (framesData[index] == null){
				framesData[index] = new FrameData();
			}
			return framesData[index];
		}
			


	}



	
}
