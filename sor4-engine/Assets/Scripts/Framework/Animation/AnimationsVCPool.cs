
using System;
using System.Collections.Generic;


// Holds the views and controllers of all animations of a character
class CharacterAnimationsVC{
	public Dictionary<string, Controller<AnimationModel>> controllers { get; private set; }
	public Dictionary<string, View<AnimationModel>> views { get; private set; }
	public View<AnimationModel> defaultView { get; set; }
	public Controller<AnimationModel> defaultController { get; set; }
	// Constructor
	public CharacterAnimationsVC(){
		controllers = new Dictionary<string, Controller<AnimationModel>>();
		views = new Dictionary<string, View<AnimationModel>>();
	}
}



// Holds the views and controllers for all animations of all characters
// There's a single animation model per character,
// using multiple views and controllers, depending on the current animation
// Each controller have information about animation events and transitions
// Each view have information about how to be rendered and how to transition between animations
public class AnimationsVCPool: Singleton<AnimationsVCPool>{

	// Animations views & controllers per character
	private Dictionary<string, CharacterAnimationsVC> charAnimVCs;
	
	// Constructor
	public AnimationsVCPool(){
		charAnimVCs = new Dictionary<string, CharacterAnimationsVC>();
	}

	private void CheckExistance(string charName){
		if (!charAnimVCs.ContainsKey(charName)) charAnimVCs[charName] = new CharacterAnimationsVC();
	}

	// Register a controller for an animation of a character
	public void RegisterController(string charName, string animationName, Controller<AnimationModel> controller){
		CheckExistance(charName);
		charAnimVCs[charName].controllers[animationName] = controller;
	}

	// Set the default controller of a character
	public void SetDefaultController(string charName, Controller<AnimationModel> controller){
		CheckExistance(charName);
		charAnimVCs[charName].defaultController = controller;
	}

	// Get the controller for a certain animation of a character
	public Controller<AnimationModel> GetController(string charName, string animationName){
		Controller<AnimationModel> controller;
		if (charAnimVCs[charName].controllers.TryGetValue(animationName, out controller)){
			return controller;
		}
		// not found, return default
		return charAnimVCs[charName].defaultController;
	}

	// Register a view for an animation of a character
	public void RegisterView(string charName, string animationName, View<AnimationModel> view){
		CheckExistance(charName);
		charAnimVCs[charName].views[animationName] = view;
	}

	// Set the default view for a character
	public void SetDefaultView(string charName, View<AnimationModel> view){
		CheckExistance(charName);
		charAnimVCs[charName].defaultView = view;
	}

	// Get the view for an animation of a character
	public View<AnimationModel> GetView(string charName, string animationName){
		View<AnimationModel> view;
		if (charAnimVCs[charName].views.TryGetValue(animationName, out view)){
			return view;
		}
		// not found, return default
		return charAnimVCs[charName].defaultView;
	}

}


