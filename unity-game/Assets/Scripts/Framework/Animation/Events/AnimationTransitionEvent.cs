using System;
using UnityEngine;
using System.Collections.Generic;



namespace RetroBread{


	// Parameterless event
	public class AnimationTransitionEvent: AnimationEvent{
		
		private string nextAnimation;

		private float transitionTime;

		private uint initialFrame;

		// Constructor
		public AnimationTransitionEvent(AnimationTriggerCondition condition, string nextAnimation, float transitionTime = 0.2f , uint initialFrame = 0)
		:base(condition)
		{
			this.nextAnimation = nextAnimation;
			this.transitionTime = transitionTime;
			this.initialFrame = initialFrame;
		}

		
		// Set model's next animation and inform view of transition timing
		protected override void Execute(AnimationModel model){
			model.SetNextAnimation(nextAnimation, initialFrame);
			AnimationView view = model.View() as AnimationView;
			if (view != null){
				view.transitionTime = transitionTime;
			}
		}
	}


}