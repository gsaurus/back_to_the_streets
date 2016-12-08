using UnityEngine;
using System.Collections.Generic;
using System.IO;
using RetroBread;
using RetroBread.Editor;

namespace RetroBread{

// Class to hold handy type conversions to/from unity
public static class UnityExtensions{

	public static UnityEngine.Vector3 AsVector3(this FixedVector3 src){
		return new UnityEngine.Vector3((float)src.X, (float)src.Y, (float)src.Z);
	}

	public static FixedVector3 AsFixedVetor3(this UnityEngine.Vector3 src){
		return new FixedVector3(src.x, src.y, src.z);
	}

	public static Transform FindDeepChild(this Transform aParent, string aName){
		foreach(Transform child in aParent){
			if(child.name == aName )
				return child;
			Transform result = child.FindDeepChild(aName);
			if (result != null)
				return result;
		}
		return null;
	}

}

// Central class
// Other editor classes use it to access data and model, and set selections
public class CharacterEditor : SingletonMonoBehaviour<CharacterEditor> {

	public static string charactersDataPath;
	public static string charactersModelsPath;

	public static string dataExtension = ".bytes";

	public static string skinsDelimiter = ":";


	// The character being edited
	public Character character { get ; private set; }

	// The skin model visible in the editor
	public GameObject characterModel { get; private set; }

	// References to param UI prefabs
	public GameObject boolToggleParam;
	public GameObject floatInputFieldParam;
	public GameObject intDropdownParam;
	public GameObject intInputFieldParam;
	public GameObject stringDropdownParam;
	public GameObject stringInputFieldParam;
	public GameObject intListInputFieldParam;
	public GameObject stringListInputFieldParam;


	// Events when something changes
	public delegate void OnSomethingChanged();
	public event OnSomethingChanged OnCharacterChangedEvent;
	public event OnSomethingChanged OnAnimationChangedEvent;
	public event OnSomethingChanged OnFrameChangedEvent;
	public event OnSomethingChanged OnCollisionChangedEvent;
	public event OnSomethingChanged OnHitChangedEvent;
	public event OnSomethingChanged OnEventChangedEvent;
	public event OnSomethingChanged OnSkinChangedEvent;


	public Dictionary<string, int> currentSkinAnimationLengths = new Dictionary<string, int>();

	// Editor selections
	private int selectedAnimationId;
	public int SelectedAnimationId {
		get{ return selectedAnimationId; }
		set{
			if (selectedAnimationId != value) {
				selectedAnimationId = value;
				if (OnAnimationChangedEvent != null) 	OnAnimationChangedEvent();
				if (OnFrameChangedEvent		!= null)	OnFrameChangedEvent();
			}
		}
	}
	private int selectedFrame;
	public int SelectedFrame {
		get{ return selectedFrame; }
		set{
			if (selectedFrame != value) {
				selectedFrame = value;
				if (OnFrameChangedEvent != null) {
					OnFrameChangedEvent();
				}
			}
		}
	}
	private int selectedCollisionId;
	public int SelectedCollisionId {
		get{ return selectedCollisionId; }
		set{
			if (selectedCollisionId != value) {
				selectedCollisionId = value;
				if (OnCollisionChangedEvent != null) {
					OnCollisionChangedEvent();
				}
			}
		}
	}
	private int selectedHitId;
	public int SelectedHitId {
		get{ return selectedHitId; }
		set {
			if (selectedHitId != value) {
				selectedHitId = value;
				if (OnHitChangedEvent != null) {
					OnHitChangedEvent();
				}
			}
		}
	}
	private int selectedEventId;
	public int SelectedEventId{
		get{ return selectedEventId; }
		set{
			if (selectedEventId != value) {
				selectedEventId = value;
				if (OnEventChangedEvent != null) {
					OnEventChangedEvent();
				}
			}
		}
	}

	// NOTE: decided to take this feature off
	// temporary import information, used once a skin is selected
//		private List<string> collisionImportList;
//		private List<string> hitImportList;



	static CharacterEditor(){
		// Setup debug
		RetroBread.Debug.Instance = new UnityDebug();
	}

	void Awake(){
		// Windows fail on loading assets from outside directories.. permission issues?..
//			if (charactersDataPath == null) charactersDataPath = Directory.GetCurrentDirectory() + "/Data/Characters/Data/";
//			if (charactersModelsPath == null) charactersModelsPath = Directory.GetCurrentDirectory() + "/Data/Characters/Models/";
		if (charactersDataPath == null) charactersDataPath = Application.streamingAssetsPath + "/Characters/Data/";
		if (charactersModelsPath == null) charactersModelsPath = Application.streamingAssetsPath + "/Characters/Models/";
		// Cache is unwanted on editor - everything changes all the time
		Caching.CleanCache();
	}
		

	void Reset(){
		
		// Clear skin
		currentSkinAnimationLengths = new Dictionary<string, int>();
		if (characterModel != null) {
			GameObject.Destroy(characterModel);
			characterModel = null;
			if (OnSkinChangedEvent != null) OnSkinChangedEvent();
		}

		// Reset selections
		selectedAnimationId = 0;
		selectedFrame = 0;
		selectedCollisionId = 0;
		selectedHitId = 0;
		selectedEventId = 0;

		// Pick a skin
		if (character.viewModels != null && character.viewModels.Count > 0) {
			string[] pathItems = character.viewModels[0].Split(skinsDelimiter.ToCharArray());
			if (pathItems != null && pathItems.Length > 1) {
				if (!SetSkin(pathItems[0], pathItems[1])){ // TODO: OnSkinChanged event is called inside, potentially dangerous
					character.viewModels.Clear();
				}
			}
		}

		if (OnCharacterChangedEvent != null)	OnCharacterChangedEvent();
		if (OnAnimationChangedEvent != null) 	OnAnimationChangedEvent();
		if (OnFrameChangedEvent		!= null)	OnFrameChangedEvent();
		if (OnCollisionChangedEvent != null) 	OnCollisionChangedEvent();
		if (OnHitChangedEvent 		!= null)	OnHitChangedEvent();
		if (OnEventChangedEvent 	!= null) 	OnEventChangedEvent();

	}



	public void CreateCharacter(string characterName){ //, List<string> collisionImportList, List<string> hitImportList){ // NOTE: decided to take this feature off

		// Fresh new character
		character = new Character(characterName);
		SaveCharacter();

		// NOTE: decided to take this feature off
//			// store import data temporarily
//			this.collisionImportList = collisionImportList;
//			this.hitImportList = hitImportList;

		Reset();
	}


	// NOTE: Decided to take this feature out
	// NOTE: This method is incomplete, in case I need to finish it
//		private void ImportCollisionAndHitData(){
//			if (characterModel == null || (collisionImportList == null && hitImportList == null)) {
//				return
//			}
//
//			// Find all the transforms to look at
//			List<Transform> collisionTransforms = new List<Transform>(collisionImportList != null ? collisionImportList.Count : 0);
//			List<Transform> hitTransforms = new List<Transform>(hitImportList != null ? hitImportList.Count : 0);
//			Transform item;
//
//			foreach (string collisionName in collisionImportList) {
//				item = characterModel.transform.FindDeepChild(collisionName);
//				if (item != null) {
//					collisionTransforms.Add(item);
//				}
//			}
//			foreach (string hitName in hitImportList) {
//				item = characterModel.transform.FindDeepChild(hitName);
//				if (item != null) {
//					hitTransforms.Add(item);
//				}
//			}
//			List<List<Transform>> allItems = new List<List<Transform>>(2);
//			allItems.Add(collisionTransforms);
//			allItems.Add(hitTransforms);
//
//			// For each animation, find the data for each item in each frame!
//			RuntimeAnimatorController controller = animator.runtimeAnimatorController;
//			if (controller != null){
//				foreach(AnimationClip clip in controller.animationClips){
//					List<List<List<ImportedData>>> importedData;
//					importedData = AutoImporter.ImportFromAnimationClip(characterModel, clip, allItems);
//					if (importedData != null) {
//						// convert to animation collisions / hits data
//					}
//				}
//			}
//
//			// clear import lists
//			collisionImportList = null;
//			hitImportList = null;
//		}



	private void LoadAnimationsLength(TextAsset asset){
		currentSkinAnimationLengths = new Dictionary<string, int>();
		byte[] bytes = asset.bytes;
		BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
		uint numAnimations = reader.ReadUInt32();
		string animName;
		int animLength;
		for (int i = 0 ; i < numAnimations ; ++i){
			animName = reader.ReadString();
			animLength = reader.ReadInt32();
			currentSkinAnimationLengths[animName] = animLength;
		}
	}

	// Load
	public void LoadCharacter(string characterName){

		// Open file stream and deserialize it
		FileInfo charFile = new FileInfo(charactersDataPath + characterName + dataExtension);
		FileStream charStream = charFile.OpenRead();
		RbStorageSerializer serializer = new RbStorageSerializer();
		Storage.Character storageCharacter = serializer.Deserialize(charStream, null, typeof(Storage.Character)) as Storage.Character;
		charStream.Close();
		// Load character into editor character format
		character = Character.LoadFromStorage(storageCharacter);
		Reset();
	}


	// Save
	public void SaveCharacter(){
		// Convert to storage format
		Storage.Character storageCharacter = character.SaveToStorage();

		// Open file stream and serialize it
		FileInfo charFile = new FileInfo (charactersDataPath + storageCharacter.name + dataExtension);
		FileStream charStream = charFile.Open(FileMode.Create, FileAccess.Write);
		RbStorageSerializer serializer = new RbStorageSerializer();
		serializer.Serialize(charStream, storageCharacter);
		charStream.Close();
	}


	// Select a skin (2D or 3D model)
	public bool SetSkin(string bundleName, string modelName){
		string url = "file://" + charactersModelsPath + bundleName;
		WWW www = WWW.LoadFromCacheOrDownload(url, 1);
		if (www.assetBundle == null) {
			Debug.LogError("Couldn't load bundle at " + url);
			return false;
		}
		GameObject prefab = www.assetBundle.LoadAsset(modelName) as GameObject;

		if (prefab == null) {
			www.assetBundle.Unload(false);
			return false;
		}

		// Load animations length, because of Unity hidding a way of getting it in runtime
		TextAsset animationsLengthData = www.assetBundle.LoadAsset<TextAsset>(modelName + " clipInfo");
		LoadAnimationsLength(animationsLengthData);
		// Update known animations
		Animator charAnimator = prefab.GetComponent<Animator>();
		int beforeAnimsCount = character.animations == null ? 0 : character.animations.Count;
		UpdateKnownAnimations(charAnimator);

		// Set skin
		SetSkin(prefab, modelName);
		www.assetBundle.Unload(false);
		if (beforeAnimsCount != character.animations.Count) {
			if (OnCharacterChangedEvent != null) OnCharacterChangedEvent();
		}
		if (OnSkinChangedEvent != null) OnSkinChangedEvent();
		return true;
	}

	private void SetSkin(GameObject prefab, string modelName){
		if (characterModel != null) {
			GameObject.Destroy(characterModel);
		}
		characterModel = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;


		// NOTE: decided to take this feature off
//			// Import data if import is pending
//			ImportCollisionAndHitData();

		// pause animations
		Animator charAnimator = characterModel.GetComponent<Animator>();
		charAnimator.speed = 0;
	}


	// Any animation on the model becomes a logical animation
	private void UpdateKnownAnimations(Animator animator){
		List<string> knownAnimations = AnimationNames();
		List<string> existingAnimations = new List<string>();
		RuntimeAnimatorController controller = animator.runtimeAnimatorController;
		int clipLength;
		if (controller != null){
			foreach(AnimationClip clip in controller.animationClips){
				//Debug.Log("len: " + clip.averageDuration + " / " + Time.fixedDeltaTime + " = " + (int) (clip.averageDuration / Time.fixedDeltaTime));
				existingAnimations.Add(clip.name);
				// TODO: option to enforce animations length?
				// Next two lines enforce animations lenght
//					currentSkinAnimationLengths.TryGetValue(clip.name, out clipLength);
//					character.animations[character.animations.FindIndex(x => x.name.Equals(clip.name))].numFrames = clipLength;
				if (!knownAnimations.Contains(clip.name)) {
					// New animation!
					// WARNING: currently clip.averageDuration is not accessible at runtime outside editor
					//clipLength = clip.averageDuration;
					currentSkinAnimationLengths.TryGetValue(clip.name, out clipLength);
					CharacterAnimation newAnim = new CharacterAnimation(clip.name, clipLength);
					character.animations.Add(newAnim);
				}
			}
		}
		// TODO: add option to remove / clean unexisting animations, for now this is just a handy thing
//			foreach(string animName in knownAnimations){
//				if (!existingAnimations.Contains(animName)) {
//					character.animations.RemoveAt(character.animations.FindIndex(x => x.name.Equals(animName)));
//				}
//			}
	}


#region Handy getters/setters


    public string[] AvailableSubjects(){
        ConditionalEvent eventToEdit = EventEditorPanel.eventToEdit;
        if (eventToEdit == null) eventToEdit = CurrentEvent();
        if (eventToEdit == null) return null;
        string[] defaultSubjects = SubjectParameterBuilder.predefinedSubjectsList;
        string[] extraSubjects = eventToEdit.SubjectsToString();
        string[] res = defaultSubjects;
        if (extraSubjects != null && extraSubjects.Length > 0){
            res = new string[defaultSubjects.Length + extraSubjects.Length];
            defaultSubjects.CopyTo(res, 0);
            extraSubjects.CopyTo(res, defaultSubjects.Length);
        }
        return res;
    }


	public List<string> AnimationNames(){
		if (character.animations == null)
			return new List<string>();
		List<string> animNamesList = new List<string>(character.animations.Count);
		foreach (CharacterAnimation anim in character.animations) {
			animNamesList.Add(anim.name);
		}
		return animNamesList;
	}

	public CharacterAnimation CurrentAnimation(){
		if (character == null || character.animations == null || character.animations.Count == 0) {
			return null;
		}
		return character.animations[selectedAnimationId];
	}

	public CollisionBox CurrentCollision(){
		CharacterAnimation currentAnim = CurrentAnimation();
		if (currentAnim == null || currentAnim.collisionBoxes.Count == 0) {
			return null;
		}
		return currentAnim.collisionBoxes[selectedCollisionId];
	}

	public HitBox CurrentHit(){
		CharacterAnimation currentAnim = CurrentAnimation();
		if (currentAnim == null || currentAnim.hitBoxes.Count == 0) {
			return null;
		}
		return currentAnim.hitBoxes[selectedHitId];
	}


	public ConditionalEvent CurrentEvent(){
		CharacterAnimation currentAnim = CurrentAnimation();
		if (currentAnim == null || currentAnim.events.Count == 0) {
			return null;
		}
		return currentAnim.events[selectedEventId];
	}

	public CollisionBox GetCollisionBox(int collisionId){
		Editor.CharacterAnimation currentAnim = CurrentAnimation();
		if (currentAnim == null || collisionId >= currentAnim.collisionBoxes.Count){
			return null;
		}
		Editor.CollisionBox currentCollision = currentAnim.collisionBoxes[collisionId];
		currentCollision.EnsureBoxExists(selectedFrame);
		return currentCollision;
	}

	public HitBox GetHitBox(int hitId){
		Editor.CharacterAnimation currentAnim = CurrentAnimation();
		if (currentAnim == null || hitId >= currentAnim.hitBoxes.Count){
			return null;
		}
		HitBox currentHit = currentAnim.hitBoxes[hitId];
		currentHit.EnsureBoxExists(selectedFrame);
		return currentHit;
	}

	public void RefreshCollisions(){
		OnCollisionChangedEvent();
	}

	public void RefreshHits(){
		OnHitChangedEvent();
	}

	public void RefreshEvents(){
		OnEventChangedEvent();
	}


#endregion


}

}
