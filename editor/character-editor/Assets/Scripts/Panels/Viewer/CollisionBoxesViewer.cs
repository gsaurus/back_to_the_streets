using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{

	public class CollisionBoxesViewer : BoxesViewer {


		void OnEnable() {
			CharacterEditor.Instance.OnFrameChangedEvent += Refresh;
			CharacterEditor.Instance.OnCollisionChangedEvent += Refresh;
		}

		void Awake() {
			unselectedColor = new Color(0, 0, 1, 0.5f);
			selectedColor = new Color(0, 1, 1, 0.5f);
			viewerMaterial = new Material(Shader.Find("Transparent/Diffuse"));
		}


		protected override void Refresh(){
			int numVisibleBoxes = 0;
			int currentFrame = CharacterEditor.Instance.SelectedFrame;
			CharacterAnimation currentAnim = CharacterEditor.Instance.CurrentAnimation();
			if (currentAnim == null)
				return;
			CollisionBox currentCollision = CharacterEditor.Instance.CurrentCollision();
			foreach (CollisionBox collision in currentAnim.collisionBoxes) {
				if (collision.enabledFrames.Count >= currentFrame && collision.enabledFrames[currentFrame]) {
					EnsureGameObject(numVisibleBoxes);
					UpdateBox(numVisibleBoxes, collision.boxesPerFrame[currentFrame], collision == currentCollision);
					++numVisibleBoxes;
				}
			}

			DisableUnusedBoxes(numVisibleBoxes);

			//
		}
	}

}
