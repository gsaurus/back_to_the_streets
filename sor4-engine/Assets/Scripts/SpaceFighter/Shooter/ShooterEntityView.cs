using UnityEngine;
using System.Collections;
using RetroBread;


public class ShooterEntityView: GameEntityView {

	private const uint invincibilityFramesCycle = 20;


	// Visual update
	public override void Update(GameEntityModel model, float deltaTime){

		base.Update(model, deltaTime);

		ShooterEntityModel shooterModel = model as ShooterEntityModel;
		if (shooterModel == null) return;

		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index);
		if (obj == null) return;

		Transform helmet = obj.transform.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 Neck/Bip01 Head/soldierHelmet");
		if (helmet != null){
			bool shouldBeActive = shooterModel.energy >= ShooterEntityController.maxEnergy*0.5f;
			if (helmet.gameObject.activeSelf && !shouldBeActive){
				GameObject lostHelmet = GameObject.Instantiate(Resources.Load("helmet"), helmet.position, helmet.rotation) as GameObject;
			}
			helmet.gameObject.SetActive(shouldBeActive);
		}
		Transform head = obj.transform.transform.Find("head");
		if (head != null){
			head.gameObject.SetActive(shooterModel.energy > 0);
		}

		if (shooterModel.invincibilityFrames > 0){
			// let game object look like a super blinking hero
			Transform t1 = obj.transform.Find("armorBody");
			Transform t2 = obj.transform.Find("armorArms");
			SkinnedMeshRenderer[] comps = new SkinnedMeshRenderer[2];
			comps[0] = t1.gameObject.GetComponent<SkinnedMeshRenderer>();
			comps[1] = t2.gameObject.GetComponent<SkinnedMeshRenderer>();
			float greenComponent = 0;
			if (shooterModel.invincibilityFrames > 1){
				greenComponent = (shooterModel.invincibilityFrames % invincibilityFramesCycle) / (float)invincibilityFramesCycle;
				if ((shooterModel.invincibilityFrames / (invincibilityFramesCycle*0.5f)) % 2 == 0){
					greenComponent = 1 - greenComponent;
				}
			}
			foreach (SkinnedMeshRenderer c in comps){
				c.material.color = new Color(c.material.color.r, greenComponent, c.material.color.b);
			}
		}

		// TODO: update hud stuff
		
	}


	public void OnHit(ShooterEntityModel model){
		PhysicPointModel pointModel = StateManager.state.GetModel(model.physicsModelId) as PhysicPointModel;
		if (pointModel == null || model.energy > 0) return;
		Vector3 bloodPosition = (Vector3)pointModel.position;
		bloodPosition.y += 2.3f;
		GameObject bloodObj = GameObject.Instantiate(Resources.Load("blood"), bloodPosition, Quaternion.identity) as GameObject;
		GameObject obj = UnityObjectsPool.Instance.GetGameObject(model.Index);
		if (obj == null) return;
		Transform headTransform = obj.transform.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 Neck/Bip01 Head");
		if (headTransform != null){
			bloodObj.transform.SetParent(headTransform.transform);
		}
	}

	
	public override bool IsCompatible(GameEntityModel originalModel, GameEntityModel newModel){
		// No local data stored so it's always compatible with any PhysicPointModel
		return true;
	}


}
