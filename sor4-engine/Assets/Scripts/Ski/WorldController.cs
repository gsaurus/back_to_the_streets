using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class WorldController:Controller<WorldModel>{

	public const uint totalGameFrames = 9997200; // 2 minutes

	public const uint framesToStart = 178;

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
	public FixedFloat accY = 0.0190f;
	

	// how much turning effects friction
	public FixedFloat frictionIncreaseFactor = 1.0f;

	// How the movement adapts to input changes
	public FixedFloat movementLerpFactor = 0.1f;

	public FixedFloat maxXValue = 0.9f;

	private SkierModel topSkierRef;
	private SkierModel bottomSkierRef;


	private bool alreadyCrossedGoal;


#region Initializations

	static WorldController(){
		// setup imutable stuff
	}


	public WorldController(){
		alreadyCrossedGoal = false;
	}



	private void HandlePlayerConnections(WorldModel model){

		List<uint> allPlayers;

		if (model.skiers [0] == null) {
			// initialize skiers
			if (StateManager.Instance.IsNetworked) {
				allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers ();
			} else {
				allPlayers = new List<uint> ();
				allPlayers.Add (0);
				int maxPlayers = UnityEngine.Random.Range (3, 6);
				for (uint i = 1; i < maxPlayers; ++i)
					allPlayers.Add(i);
			}
			
			// Create characters for new players
			int playerPosition = StateManager.Instance.IsNetworked ? -1 : UnityEngine.Random.Range (0, allPlayers.Count);
			FixedFloat playerX = 0;
			FixedFloat distanceBetweenPlayers = 2.2f;
			foreach (uint playerId in allPlayers) {
				if (model.skiers [playerId] == null) {
					Model inputModel = new PlayerInputModel (playerId);
					ModelReference inputModelRef = StateManager.state.AddModel (inputModel);
					playerX = (int)playerId * distanceBetweenPlayers;
					if (playerPosition >= 0) {
						if (playerId == 0) {
							playerX = playerPosition * distanceBetweenPlayers;
						} else if (playerId == playerPosition) {
							playerX = 0;
						}
					}
					model.skiers [playerId] = new SkierModel (playerX, 0, inputModelRef);
				}
			}
		}



		// Remove characters for inactive players
		if (StateManager.Instance.IsNetworked) {
			allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers();
			for (int i = 0; i < model.skiers.Length; ++i) {
				if (!allPlayers.Exists (x => x == i)) {
					// Doesn't exist anymore, remove
					model.skiers[i] = null;
				}
			}
		}
	}
	


#endregion
	


#region Skiers



	private void CheckTopAndBottomSkiers(WorldModel world){
	
		SkierModel skier;
		// For each tank update it's position and orientation
		FixedFloat topY = 0;
		FixedFloat bottomY = 0;
		topSkierRef = null;
		bottomSkierRef = null;
		for (uint skierId = 0 ; skierId < world.skiers.Length ; ++skierId){
			skier = world.skiers[skierId];
			if (skier != null){
				if (topSkierRef == null || skier.y < topY){
					topY = skier.y;
					topSkierRef = skier;
				}
				else if (bottomSkierRef == null || skier.y > bottomY){
					bottomY = skier.y;
					bottomSkierRef = skier;
				}
			}
		}
	}


	private void UpdateSkiers(WorldModel world){

		CheckTopAndBottomSkiers(world);

		SkierModel skier;
		int ownPlayerNumber = StateManager.Instance.IsNetworked ? NetworkCenter.Instance.GetPlayerNumber() : 0;
		// For each tank update it's position and orientation
		for (uint skierId = 0 ; skierId < world.skiers.Length ; ++skierId){
			skier = world.skiers[skierId];
			if (skier != null){
				bool crossedGoal = skier.y < -WorldObjects.finalGoalDistance;
				if (!crossedGoal) {
					if (skier.fallenTimer > 0) {
						if (--skier.fallenTimer == 0) {
							UpdateSkierDirectionBasedOnInput(skier, 0);
						}
					}
					if (skier.frozenTimer > 0) {
						if (--skier.frozenTimer == 0) {
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
						else if (!StateManager.Instance.IsNetworked && skierId > 0) {
							FixedFloat botTargetAxis = WorldObjects.GetTargetAxisForBot(world, skier);
							UpdateSkierDirectionBasedOnInput(skier, botTargetAxis);
							if (skier.y > world.skiers[0].y + 3.5) {
								skier.velX *= 1.02f;
								skier.velY *= 1.02f;
							}
						}
					}
				}



				// Update skier position
				UpdateSkierPosition(skier);

				// check collisions
				if (StateManager.Instance.IsNetworked || skierId == 0 || skier.y < world.skiers [0].y + 4) {
					WorldObjects.HandleCollisionWithWorld(world, skier);
					WorldObjects.HandleCollisionWithOtherSkiers(world, skier);
				}

				if (crossedGoal && skierId == ownPlayerNumber && !alreadyCrossedGoal) {
					alreadyCrossedGoal = true;
					ClockCounter.Instance.Stop();
					GuiMenus.Instance.MarkToRestart();
				}
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

		bool crossedGoal = skier.y < -WorldObjects.finalGoalDistance;
		if (crossedGoal && skier.targetVelX != 0 && skier.targetVelY != 0) {
			if (skier.targetVelX > 0) skier.targetVelX += 0.03f;
			else skier.targetVelX -= 0.03f;
			FixedFloat angle = FixedFloat.Atan2 (skier.targetVelY, skier.targetVelX);
			skier.targetVelY = FixedFloat.Sin(angle);
			if (skier.targetVelX > 1 || skier.targetVelX < -1) {
				skier.targetVelX = skier.targetVelY = 0;
			}
			//skier.targetVelX = FixedFloat.Cos(angle);
			//skier.targetVelX = 0.0f;
			//skier.targetVelY = 0.0f;
		}

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

		if (!crossedGoal) {
			if (skier.fallenTimer == 0 && skier.frozenTimer == 0
				&& vY > -minVerticalVelocity) {
				vY = -minVerticalVelocity;
			}
			if (skier == topSkierRef){
				vX *= 0.9f;
				vY *= 0.9f;
			}else if (skier == bottomSkierRef){
				vX *= 1.1f;
				vY *= 1.1f;
			}
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

		if (StateManager.state.Keyframe > framesToStart){
			// Update skiers only after initial countdown
			UpdateSkiers(model);
		}
	}
	
}

