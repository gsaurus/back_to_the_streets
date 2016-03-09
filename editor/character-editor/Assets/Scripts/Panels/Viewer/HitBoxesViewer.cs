using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{

	public class HitBoxesViewer : BoxesViewer {


		void OnEnable() {
			CharacterEditor.Instance.OnFrameChangedEvent += Refresh;
			CharacterEditor.Instance.OnHitChangedEvent += Refresh;
		}


		void Awake() {
			unselectedColor = new Color(1, 0, 0, 0.5f);
			selectedColor = new Color(1, 1, 0, 0.5f);
			viewerMaterial = new Material(Shader.Find("Transparent/Diffuse"));
		}


		protected override void Refresh(){
			int numVisibleBoxes = 0;
			int currentFrame = CharacterEditor.Instance.SelectedFrame;
			CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null)
				return;
			HitBox currentHit = CharacterEditor.Instance.CurrentHit();
			foreach (HitBox hit in currentAnim.hitBoxes) {
				if (hit.enabledFrames.Count >= currentFrame && hit.enabledFrames[currentFrame]) {
					EnsureGameObject(numVisibleBoxes);
					UpdateBox(numVisibleBoxes, hit.boxesPerFrame[currentFrame], hit == currentHit);
					++numVisibleBoxes;
				}
			}

			DisableUnusedBoxes(numVisibleBoxes);
		
		}
	}

}
