using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class WorldController:Controller<WorldModel>{

	public const uint totalGameFrames = 9997200; // 2 minutes

//	// velocity applied to horizontal movement
//	public FixedFloat maxHorizontalVelocity = 0.165f;
//	// velocity applied to vertical movement
//	public FixedFloat maxVerticalVelocity = 0.25f;
//
//	public FixedFloat minVerticalVelocity = 0.02f;

	// velocity applied to horizontal movement
	public FixedFloat maxHorizontalVelocity = 0.25f;
	// velocity applied to vertical movement
	public FixedFloat maxVerticalVelocity = 0.3f;
	
	public FixedFloat minVerticalVelocity = 0.02f;

	// how friction is reduced over time
	public FixedFloat frictionReductionFactor = 0.02f;

	public FixedFloat accX = 0.085f;
	public FixedFloat accY = 0.0235f;
	

	// how much turning effects friction
	public FixedFloat frictionIncreaseFactor = 1.0f;

	// How the movement adapts to input changes
	public FixedFloat movementLerpFactor = 0.1f;

	public FixedFloat maxXValue = 0.9f;


#region Initializations

	static WorldController(){
		// setup imutable stuff
	}


	public WorldController(){
		// Nothing to do
	}



	private void HandlePlayerConnections(WorldModel model){
		List<uint> allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers();

		// Remove characters for inactive players
		for (int i = 0 ; i < model.skiers.Length ; ++i){
			if (!allPlayers.Exists(x => x == i)){
				// Doesn't exist anymore, remove
				model.skiers[i] = null;
			}
		}
		
		// Create characters for new players
		foreach(uint playerId in allPlayers){
			if (model.skiers[playerId] == null){
				Model inputModel = new PlayerInputModel(playerId);
				ModelReference inputModelRef = StateManager.state.AddModel(inputModel);
				model.skiers[playerId] = new SkierModel(playerId * 1.5f, 0, inputModelRef);
			}
			
		}
	}
	


#endregion
	


#region Skiers

	private void UpdateSkiers(WorldModel world){

		SkierModel skier;
		// For each tank update it's position and orientation
		for (uint skierId = 0 ; skierId < world.skiers.Length ; ++skierId){
			skier = world.skiers[skierId];
			if (skier != null){

				if (skier.fallenTimer > 0){
					if (--skier.fallenTimer == 0){
						UpdateSkierDirectionBasedOnInput(skier, 0);
					}
				}
				if (skier.frozenTimer > 0){
					if (--skier.frozenTimer == 0){
						UpdateSkierDirectionBasedOnInput(skier, 0);
					}
				}

				// Use input to update skier direction
				if (skier.fallenTimer == 0 && skier.frozenTimer == 0){
					PlayerInputModel inputModel = StateManager.state.GetModel(skier.inputModelRef) as PlayerInputModel;
					if (inputModel != null && inputModel.axis != 0.0f){
						// reversed cose camera is reversed too
						UpdateSkierDirectionBasedOnInput(skier, -inputModel.axis);
					}
				}

				// Update skier position
				UpdateSkierPosition(skier);

				// check collisions
				WorldObjects.HandleCollisionWithWorld(world, skier);
				WorldObjects.HandleCollisionWithOtherSkiers(world, skier);
			}
		}

	}

	private void UpdateSkierDirectionBasedOnInput(SkierModel skier, FixedFloat deltaVel){

		if (deltaVel < 0 && skier.targetVelX < -maxXValue - deltaVel) {
			deltaVel = -maxXValue - skier.targetVelX;
		}else if (deltaVel > 0 && skier.targetVelX > maxXValue - deltaVel) {
			deltaVel = maxXValue - skier.targetVelX;
		}

		skier.targetVelX = skier.targetVelX + deltaVel;
		skier.targetVelY = -FixedFloat.One + FixedFloat.Abs(skier.targetVelX);
		if (skier.targetVelX != 0) {
			skier.targetVelY = FixedFloat.Sin(FixedFloat.Atan2(skier.targetVelY, skier.targetVelX));
		}

		skier.friction = skier.friction + frictionIncreaseFactor * deltaVel;
		skier.friction = FixedFloat.Clamp(skier.friction, -FixedFloat.One, FixedFloat.One);
//		skier.frictionY = FixedFloat.Min(skier.frictionY + frictionIncreaseFactor * FixedFloat.Abs(deltaVel), FixedFloat.One);
//		skier.velX = skier.velX + deltaVel;
//		skier.velX = FixedFloat.Lerp(skier.velX, skier.velX + deltaVel, movementLerpFactor);
//		skier.velY = -FixedFloat.One + FixedFloat.Abs(skier.velX);
//		UnityEngine.Debug.Log("velX: " + skier.velX + ", deltaVel: " + deltaVel);
	}

	private void UpdateSkierPosition(SkierModel skier) {

		if (skier.velX < skier.targetVelX){
			skier.velX = FixedFloat.Min(skier.velX + accX, skier.targetVelX);
		}else if (skier.velX > skier.targetVelX) {
			skier.velX = FixedFloat.Max(skier.velX - accX, skier.targetVelX);
		}

		if (skier.velY < skier.targetVelY){
			skier.velY = FixedFloat.Min(skier.velY + accY, skier.targetVelY);
		}else if (skier.velY > skier.targetVelY) {
			skier.velY = FixedFloat.Max(skier.velY - accY, skier.targetVelY);
		}

		FixedFloat vX = skier.velX * maxHorizontalVelocity;
		FixedFloat vY = skier.velY * maxVerticalVelocity;


		// Update friction
		FixedFloat absFriction = FixedFloat.Abs(skier.friction);
		if (absFriction > FixedFloat.Zero) {
			FixedFloat inversedFriction = (FixedFloat.One - absFriction);
			vX *= inversedFriction;
			vY *= inversedFriction;

			if (skier.friction > 0){
				skier.friction -= frictionReductionFactor;
				if (skier.friction < FixedFloat.Zero) skier.friction = FixedFloat.Zero;
			}else if (skier.friction < 0) {
				skier.friction += frictionReductionFactor;
				if (skier.friction > FixedFloat.Zero) skier.friction = FixedFloat.Zero;
			}
		}
		
		
		if (skier.fallenTimer == 0 && skier.frozenTimer == 0
		    && vY > -minVerticalVelocity
		){
			vY = -minVerticalVelocity;
		}

		skier.x += vX;
		skier.y += vY;


		// TODO: handle collisions and the hell..
	}


#endregion



	protected override void Update(WorldModel model){

		// update track
		FixedFloat minY = 0;
		FixedFloat maxY = 0;
		SkierModel skier;
		// For each tank update it's position and orientation
		for (uint skierId = 0 ; skierId < model.skiers.Length ; ++skierId){
			skier = model.skiers[skierId];
			if (skier != null){
				if (minY == 0 || skier.y > minY){
					minY = skier.y;
				}
				if (maxY == 0 || skier.y < maxY){
					maxY = skier.y;
				}
			}
		}

		WorldObjects.UpdateTrack(model, minY, maxY);

		HandlePlayerConnections(model);
		UpdateSkiers(model);
	}
	
}

