using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace RetroBread{


	public class PlayerPanel : MonoBehaviour {

		// Object items
		public GameObject slider;
		public GameObject playButton;
		public GameObject frameInput;
		public GameObject sizeInput;
		public GameObject collisionsList;
		public GameObject hitsList;

		// Component items
		private Slider _slider;
		private InputField _frameInput;
		private InputField _sizeInput; 

		// Player variables
		private bool isPlaying;
		// when slider or input fields change internally, don't want callback responses
		private bool internalChange;


		void Awake(){
			_slider = slider.GetComponent<Slider>();
			_frameInput = frameInput.GetComponent<InputField>();
			_sizeInput = sizeInput.GetComponent<InputField>();
		}


		void OnEnable () {
			CharacterEditor.Instance.OnCharacterChangedEvent += OnCharacterChanged;
			CharacterEditor.Instance.OnAnimationChangedEvent += OnAnimationChanged;
			CharacterEditor.Instance.OnFrameChangedEvent += OnFrameChanged;
			CharacterEditor.Instance.OnSkinChangedEvent += OnCharacterChanged;
			CharacterEditor.Instance.OnCollisionChangedEvent += OnCollisionChanged;
			CharacterEditor.Instance.OnHitChangedEvent += OnHitChanged;
		}

#region Event notifications

		void OnCharacterChanged(){
			UpdateModelAnimation();
		}

		void OnAnimationChanged(){
			_sizeInput.text = "" + CurrentAnimationSize();
			UpdateModelAnimation();
		}

		void OnFrameChanged(){
			internalChange = true;
			_frameInput.text = "" + CharacterEditor.Instance.SelectedFrame;
			_slider.value = CharacterEditor.Instance.SelectedFrame * 1.0f / CurrentAnimationSize();
			UpdateModelAnimation();
			internalChange = false;
		}

		void OnCollisionChanged(){
		
		}

		void OnHitChanged(){
		
		}

#endregion

#region Player UI

		public void OnBeginButton(){
			CharacterEditor.Instance.SelectedFrame = 0;
		}

		public void OnBackButton(){
			if (CharacterEditor.Instance.SelectedFrame > 0) {
				CharacterEditor.Instance.SelectedFrame -= 1;
			}
		}

		public void OnPlayButton(){
			isPlaying = !isPlaying;
			Text playButtonText = playButton.GetComponentInChildren<Text>();
			playButtonText.text = isPlaying ? "||" : ">";
		}

		public void OnForwardButton(){
			if (CharacterEditor.Instance.SelectedFrame < CurrentAnimationSize()-1) {
				CharacterEditor.Instance.SelectedFrame += 1;
			}
		}

		public void OnEndButton(){
			CharacterEditor.Instance.SelectedFrame = CurrentAnimationSize() - 1;
		}


		public void OnSliderChanged(float value){
			if (internalChange) return;
			int newFrame = Mathf.RoundToInt(value * (CharacterEditor.Instance.CurrentAnimation().numFrames-1));
			CharacterEditor.Instance.SelectedFrame = newFrame;
		}

		public void OnFrameFieldChanged(string newFrameString){
			if (internalChange) return;
			int newFrame;
			if (int.TryParse(newFrameString, out newFrame) && newFrame >= 0 && newFrame < CurrentAnimationSize()) {
				CharacterEditor.Instance.SelectedFrame = newFrame;
			}
		}

		public void OnSizeFieldChanged(string newSizeString){
			if (internalChange) return;
			int newSize;
			if (int.TryParse(newSizeString, out newSize) && newSize > 0) {
				CharacterEditor.Instance.CurrentAnimation().numFrames = newSize;
				if (CharacterEditor.Instance.SelectedFrame >= CurrentAnimationSize()) {
					CharacterEditor.Instance.SelectedFrame = CurrentAnimationSize() - 1;
				}
			}
		}


		// Update is called once per frame
		void FixedUpdate() {
			if (isPlaying) {
				int targetFrame = CharacterEditor.Instance.SelectedFrame + 1;
				if (targetFrame >= CurrentAnimationSize()) {
					targetFrame = 0;
				}
				CharacterEditor.Instance.SelectedFrame = targetFrame;
			}
		}

#endregion


#region Helper methods


		private void UpdateModelAnimation(){
			if (CharacterEditor.Instance.characterModel == null) return;
			Animator animator = CharacterEditor.Instance.characterModel.GetComponent<Animator>();
			UpdateAnimation(
				animator,
				CharacterEditor.Instance.CurrentAnimation().name,
				CharacterEditor.Instance.SelectedFrame
			);
		}


		private void UpdateAnimation(Animator animator, string animationName, int frame){
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName)){
				animator.Play(animationName);
			}
			float animLen = CurrentAnimationLength(animator);
			float newAnimationTime = frame / animLen;
			animator.Play(animationName, 0, newAnimationTime);
		}


		private int CurrentAnimationSize(){
			Editor.CharacterAnimation anim = CharacterEditor.Instance.CurrentAnimation();
			return anim != null ? anim.numFrames : 0;
		}

		private float CurrentAnimationLength(Animator animator){
			// WARNING: currently clip.averageDuration is not accessible at runtime outside editor
			AnimatorClipInfo[] clipsInfo = animator.GetCurrentAnimatorClipInfo(0);
			//return clipsInfo[0].clip.averageDuration / Time.fixedDeltaTime;
			int length;
			CharacterEditor.Instance.currentSkinAnimationLengths.TryGetValue(clipsInfo[0].clip.name, out length);
			return length;
		}

#endregion


	}

}
