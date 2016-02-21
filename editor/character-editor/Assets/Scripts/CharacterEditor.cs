using UnityEngine;
using System.Collections;
using RetroBread;
using RetroBread.Editor;

public class CharacterEditor : MonoBehaviour {

	private enum EditorMode{
		collisionMode,
		attackMode,
		eventsMode
	};


	private Character character;

	// UI objects references
	public GameObject collisionPanel;
	public GameObject attackPanel;
	public GameObject eventsListPanel;
	public GameObject eventsEditorPanel;


	// Editor mode
	private EditorMode selectedMode = EditorMode.collisionMode;

	// Editor selections
	private int selectedAnimationId;
	private int selectedFrame;

	// From the editor lists
	private int selectedCollisionId;
	private int selectedHitId;
	private int selectedEventId;

	public void LoadCharacter(string characterName){
		// TODO: find the file, deserialize it and load into character
		RetroBread.Storage.Character storageCharacter = null;
		character = Character.LoadFromStorage(storageCharacter);
	}

	public void SaveCharacter(){
		// TODO: serialize and save character into a file
		RetroBread.Storage.Character storageCharacter = character.SaveToStorage();
	}



	private void RefreshEditorPanel(){
		switch (selectedMode){
			case EditorMode.collisionMode:
				SelectCollision(selectedCollisionId);
			break;
			case EditorMode.attackMode:
				SelectHit(selectedHitId);
			break;
			case EditorMode.eventsMode:
				SelectEvent(selectedEventId);
			break;
		}
	}


	public void SelectAnimation(int animationId){

		selectedAnimationId = animationId;

		// Update whatever panel that is visible at the momment
		RefreshEditorPanel();

		// TODO: update model animation & player

	}


	public void SelectFrame(int frameNum){
		selectedFrame = frameNum;
		RefreshEditorPanel();
	}


	public void SelectCollision(int collisionId){
		selectedCollisionId = collisionId;
		// TODO: update collision panel stuff
	}

	public void SelectHit(int hitId){
		selectedHitId = hitId;
		// TODO: update attack panel stuff
	}

	public void SelectEvent(int eventId){
		selectedEventId = eventId;
		// TODO: update event panel stuff
	}



}
