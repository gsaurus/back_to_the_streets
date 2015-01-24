
using System;
using UnityEngine;
using System.Collections.Generic;


// Holds the controllers for all animations
// There's a single animation model per character,
// using multiple controllers, depending on the current animation
// Each controller have information about animation events and transitions
// and perhaps extra custom behaviour if necessary
public class AnimationsManager: Singleton<AnimationsManager>{

	private Dictionary<string, AnimationController> controllers;
	

	public AnimationsManager(){
		controllers = new Dictionary<string, AnimationController>();
	}

	public void RegisterController(string animationName, AnimationController controller){
		controllers[animationName] = controller;
	}

	public AnimationController GetController(string animationName){
		AnimationController controller;
		if (controllers.TryGetValue(animationName, out controller)){
			return controller;
		}
		// Woops!!
		return null;
	}

}


