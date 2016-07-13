using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace RetroBread{


	public static class CharacterLoader{

		public static string charactersDataPath;
		public static string charactersModelsPath;

		public static string dataExtension = ".bytes";
		public static string prefabDelimiter = ":";

		private static int invalidBoxId = -1;

		// Warning: TODO: keep a pool of Boxes, to use less memory

		// store here a list of known characters
		// WARNING: TODO: Doesn't have to keep this, just a bool telling loaded or not
		// all info should go to a character controller
		private static Dictionary<string, Storage.Character> loadedCharacters = new Dictionary<string, Storage.Character>();

		// Portraits by portrait name
		private static Dictionary<string, Sprite> loadedPortraitSprites = new Dictionary<string, Sprite>();


		static CharacterLoader(){
			// Windows fail on loading assets from outside directories.. permission issues?..
//			if (charactersDataPath == null) charactersDataPath = Directory.GetCurrentDirectory() + "/Data/Characters/Data/";
//			if (charactersModelsPath == null) charactersModelsPath = Directory.GetCurrentDirectory() + "/Data/Characters/Models/";
			if (charactersDataPath == null) charactersDataPath = Application.streamingAssetsPath + "/Characters/Data/";
			if (charactersModelsPath == null) charactersModelsPath = Application.streamingAssetsPath + "/Characters/Models/";
		}


		// Get a character portrait by entity id
		public static Sprite GetCharacterPortrait(uint modelId){
			GameEntityModel ownerModel = StateManager.state.GetModel(modelId) as GameEntityModel;
			if (ownerModel == null) return null;
			AnimationModel animModel = StateManager.state.GetModel(ownerModel.animationModelId) as AnimationModel;
			if (animModel == null) return null;
			string prefabName = null;
			if (animModel.viewModelName != null){
				prefabName = animModel.viewModelName;
			}else{
				prefabName = animModel.characterName;
			}
			Storage.Character storageCharacter;
			if (!loadedCharacters.TryGetValue(animModel.characterName, out storageCharacter)) return null;
			int index = Array.FindIndex<string>(storageCharacter.viewModels, x => x.Equals(prefabName));
			if (index < 0) return null;
			string spriteName = storageCharacter.portraits[index];
			if (!loadedPortraitSprites.ContainsKey(spriteName)) return null;

			Sprite portrait;
			loadedPortraitSprites.TryGetValue(spriteName, out portrait);
			return portrait;
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

		// Get all anchor names for a character
		public static List<string> GetCharacterAnchorNames(string characterName){
			Storage.Character storageCharacter;
			if (loadedCharacters.TryGetValue(characterName, out storageCharacter)) {
				if (storageCharacter.viewAnchors != null){
					return new List<string>(storageCharacter.viewAnchors);
				}
			}
			return null;
		}


		// Load Character view model, and portrait
		public static GameObject LoadViewModel(string characterName, string prefabName){
			string[] pathItems = prefabName.Split(prefabDelimiter.ToCharArray());
			Storage.Character storageCharacter = null;
			if (characterName != null){
				loadedCharacters.TryGetValue(characterName, out storageCharacter);
			}
			string skinName;
			if (pathItems != null && pathItems.Length > 1) {
				string url = "file://" + charactersModelsPath + pathItems[0];
				WWW www = WWW.LoadFromCacheOrDownload(url, 1);
				if (www.assetBundle == null) {
					Debug.LogError("Failed to load character bundle at " + url);
				}
				// Load model prefab from bundle
				GameObject prefab = www.assetBundle.LoadAsset<GameObject>(pathItems[1]);

				// Load portrait from bundle
				// TODO: refactor: duplicate code
				if (storageCharacter != null) {
					int index = Array.FindIndex<string>(storageCharacter.viewModels, x => x.Equals(prefabName));
					if (index >= 0){
						string spriteName = storageCharacter.portraits[index];
						if (!loadedPortraitSprites.ContainsKey(spriteName)){
							Sprite sprite = www.assetBundle.LoadAsset<Sprite>(spriteName);
							loadedPortraitSprites[spriteName] = sprite;
						}
					}
				}

				www.assetBundle.Unload(false);
				if (prefab != null) {
					return prefab;
				}
				skinName = pathItems[1];
			}else {
				skinName = prefabName;
			}

			// Load portrait from resources
			// TODO: refactor: duplicate code
			if (storageCharacter != null) {
				int index = Array.FindIndex<string>(storageCharacter.viewModels, x => x.Equals(pathItems[1]));
				if (index >= 0){
					string spriteName = storageCharacter.portraits[index];
					if (!loadedPortraitSprites.ContainsKey(spriteName)){
						Sprite sprite = Resources.Load<Sprite>(spriteName);
						loadedPortraitSprites[spriteName] = sprite;
					}
				}
			}

			// Load skin from resources
			return Resources.Load<GameObject>(skinName);
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
					Debug.LogError("Can't find character data " + characterName);
				}

				// Load character into game character format (view, controller)
				SetupCharacter(storageCharacter);
			}

		}


		private static AnimationEvent ReadEvent(Storage.Character charData, Storage.CharacterEvent storageEvent, out int keyFrame, Storage.CharacterAnimation animation){
			// Build event
			AnimationTriggerCondition condition = CharacterConditionsBuilder.Build(charData, storageEvent.conditionIds, out keyFrame, animation);
			AnimationEvent e = CharacterEventsBuilder.Build(charData, storageEvent.eventIds);
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
						if (keyFrame != CharacterConditionsBuilder.invalidKeyframe) {
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
					Storage.CollisionBox storageCollisionBox;
					for(int collisionId = 0 ; collisionId < animation.collisionBoxes.Length ; ++collisionId){
						storageCollisionBox = animation.collisionBoxes[collisionId];
						// for each frame of each box
						for (int frame = 0; frame < storageCollisionBox.boxIds.Length ; ++frame){
							boxIndex = storageCollisionBox.boxIds[frame];
							if (boxIndex != invalidBoxId){
								storageBox = charData.boxes[boxIndex];
								frameData = GetFrameData(framesData, frame);
								frameData.collisions.Add(new CollisionBox(new Box(storageBox.pointOne, storageBox.pointTwo), collisionId));
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
					Storage.HitBox storageHitBox;
					for(int hitId = 0 ; hitId < animation.hitBoxes.Length ; ++hitId){
						storageHitBox = animation.hitBoxes[hitId];
						param = charData.genericParameters[storageHitBox.paramId];
						hitData = CharacterHitsBuilder.Build(param);
						if (hitData == null) continue;
						hitData.hitboxID = hitId;
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
					if (finalFramedata != null) {
						finalFramedata.ComputeBoundingBoxes();
					}
				}
				// Store frames data on animation controller
				controller.SetFramesData(framesData);

			}

			// Note: for now storage character data is stored locally,
			// Skins and anchor names accessed through storage data
			// TODO: Perhaps store skins and anchor names separatedly, maybe somewhere else,
			// instead of keeping storage data in memory..

		}

		private static FrameData GetFrameData(FrameData[] framesData, int index){
			if (framesData[index] == null){
				framesData[index] = new FrameData();
			}
			return framesData[index];
		}
			


	}



	
}
