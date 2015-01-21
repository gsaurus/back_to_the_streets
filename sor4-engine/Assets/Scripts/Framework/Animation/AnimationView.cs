
using System;
using UnityEngine;
using System.Collections.Generic;




// 
public class AnimationView:View<AnimationModel>{
	
	Transform instance;


	public AnimationView(Transform prefab){
		instance = MonoBehaviour.Instantiate(prefab);
	}


	// Visual update
	public virtual void Update(AnimationModel model, float deltaTime){

		// TODO: interpolation
		AnimationState anim = instance.animation[model.name]; 
		anim.time = model.currentFrame * StateManager.Instance.UpdateRate;
	}


	public virtual bool IsCompatibleWithModel(AnimationModel model){
		// TODO: check if the instance is of same type...
		return false;
	}
	
	
	public virtual void OnDestroy(){
		MonoBehaviour.Destroy(instance);
	}
	
}

