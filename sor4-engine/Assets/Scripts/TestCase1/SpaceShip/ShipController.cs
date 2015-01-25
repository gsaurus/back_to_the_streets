
using System;
using UnityEngine;
using System.Collections.Generic;


public class ShipController:Controller<ShipModel>{

	//private BulletModel createdBulletModel;


	private void EnsureSubModels(ShipModel model){
		if (model.physicsModelId == StateManager.invalidModelIndex) {
			// Add a new PhysicPointModel to the world physics state
		}
		if (model.animationModelId == StateManager.invalidModelIndex) {
			// Add a new animation model to the game state
		}
	}

	// delegate for when physic point model is added to the game state
	private void OnPhysicPointModelCreated(Model model, object mainModelObj){
		ShipModel shipModel = mainModelObj as ShipModel;
		shipModel.physicsModelId = model.Index;
	}
	// delegate for when animation model is added to the game state
	private void OnAnimationModelCreated(Model model, object mainModelObj){
		ShipModel shipModel = mainModelObj as ShipModel;
		shipModel.animationModelId = model.Index;
	}

	public override void Update(ShipModel model){

		EnsureSubModels(model);

		PhysicPointModel pointModel = StateManager.state.GetModel(model.physicsModelId) as PhysicPointModel;
		List<Event> playerEvents = StateManager.Instance.GetEventsForPlayer(model.player);
		if (playerEvents != null){
			ShipInputEvent shipEvent;
			foreach (Event e in playerEvents){
				shipEvent = e as ShipInputEvent;

				if (shipEvent != null) {
					switch (shipEvent.type){
						case ShipInputType.Left:{
							model.leftHolded = shipEvent.state == ShipInputState.Pressed;
						}break;
						case ShipInputType.Right:{
							model.rightHolded = shipEvent.state == ShipInputState.Pressed;
						} break;
						case ShipInputType.Up:{
							model.upHolded = shipEvent.state == ShipInputState.Pressed;
						}break;
						case ShipInputType.Down:{
							model.downHolded = shipEvent.state == ShipInputState.Pressed;
						}break;
						case ShipInputType.Jump:{
							if (shipEvent.state == ShipInputState.Pressed) {
								// TODO: check if it's landed
								// JUMP!!!
								//AddDefaultVelocityAffector(new FixedVector3(0,0.2,0));
								pointModel.velocityAffectors[PhysicPointModel.defaultVelocityAffectorName] += new FixedVector3(0,0.2,0);
							}
						}break;
						case ShipInputType.Fire:{
							if (shipEvent.state == ShipInputState.Pressed) {
								// FIRE!!!
								BulletModel createdBulletModel = new BulletModel(
									model.player,
									pointModel.position.X,
									pointModel.position.Y + 0.5f * (model.player % 2 != 0 ? 1.0f : -1.0f),
									pointModel.position.Z
								);
								StateManager.state.AddModel(createdBulletModel);
							}
						}break;
					} // end switch
				} // end if
			} // end foreach
		} // end if

		// Apply input movement
		FixedVector3 vel = FixedVector3.Zero;
		if (model.leftHolded != model.rightHolded){
			vel.X = 0.075f * (model.leftHolded ? -1f : 1f);
		}
		if (model.upHolded != model.downHolded){
			vel.Z = 0.125f * (model.upHolded ? 1f : -1f);
		}
		pointModel.velocityAffectors["inputMovement"] = vel;
		//SetVelocityAffector("inputMovement", vel);

	}


	public override void PostUpdate(PhysicPointModel model){
		base.PostUpdate(model);
	}
	
}

