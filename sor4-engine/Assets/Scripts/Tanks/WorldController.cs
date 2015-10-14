using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class WorldController:Controller<WorldModel>{

	public const uint totalGameFrames = 9997200; // 2 minutes

	public static FixedFloat maxTankVelocity = 0.04f;

	static WorldController(){
		// setup imutable stuff
	}


	public WorldController(){
		// Nothing to do
	}



	private void HandlePlayerConnections(WorldModel model){
		List<uint> allPlayers = NetworkCenter.Instance.GetAllNumbersOfConnectedPlayers();

		// Remove characters for inactive players
		for (int i = 0 ; i < model.tanks.Length ; ++i){
			if (!allPlayers.Exists(x => x == i)){
				// Doesn't exist anymore, remove
				model.tanks[i] = null;
			}
		}
		
		// Create characters for new players
		foreach(uint playerId in allPlayers){
			if (model.tanks[playerId] == null){
				Model inputModel = new PlayerInputModel(playerId, 1, 1);
				ModelReference inputModelRef = StateManager.state.AddModel(inputModel);
				model.tanks[playerId] = new TankModel(InitialPositionForTankId(playerId), 3, 0, inputModelRef);
			}
			
		}
	}


	private FixedVector3 InitialPositionForTankId(uint tankId){
		// ...
		switch (tankId) {
			case 0: return new FixedVector3(0,0,0);
			case 1: return new FixedVector3(WorldModel.MaxWidth - 1, 0, 0);
			case 2: return new FixedVector3(WorldModel.MaxWidth - 1, WorldModel.MaxHeight - 1, 0);
			case 3: return new FixedVector3(0, WorldModel.MaxHeight - 1, 0);
			case 4: return new FixedVector3(WorldModel.MaxWidth * 0.5f, 0, 0);
			case 5: return new FixedVector3(WorldModel.MaxWidth * 0.5f,WorldModel.MaxHeight - 1, 0);
			case 6: return new FixedVector3(0, WorldModel.MaxHeight * 0.5f,0);
			case 7: return new FixedVector3(WorldModel.MaxWidth - 1, WorldModel.MaxHeight * 0.5f,0);
		}
		return new FixedVector3(1,1,0);
	}



	private void UpdateTanks(WorldModel world){
		FixedVector3 previousTankPosition;
		FixedVector3 velocity = FixedVector3.Zero;
		PlayerInputModel inputModel;
		FixedFloat movingAngle;
		FixedFloat targetAngle;
		foreach (TankModel tank in world.tanks){
			if (tank != null){
				previousTankPosition = tank.position;
				inputModel = StateManager.state.GetModel(tank.inputModelRef) as PlayerInputModel;
				if (inputModel != null && inputModel.axis[0] != FixedVector3.Zero) {
					UpdateTankPosition(world, tank, inputModel);
					velocity = tank.position - previousTankPosition;
					targetAngle = FixedFloat.Atan2(inputModel.axis[0].Z, inputModel.axis[0].X);
					if (targetAngle < 0){
						targetAngle += FixedFloat.TwoPI;
					}
				}else {
					movingAngle = tank.movingAngle;
					targetAngle = tank.shootingTargetAngle;
				}
				UpdateTankDirection(tank, velocity, targetAngle);
			}
		}
	}


	private void UpdateTankPosition(WorldModel world, TankModel tank, PlayerInputModel inputModel){
		inputModel.axis[0].Normalize();
		FixedVector3 targetPosition = tank.position + new FixedVector3(inputModel.axis[0].X * maxTankVelocity, inputModel.axis[0].Z * maxTankVelocity, 0);
		tank.position = CollisionResponse(world, tank.position, targetPosition, 0.999f);
	}


	// Collision of a point in the world
	private FixedVector3 CollisionResponse(WorldModel world, FixedVector3 origin, FixedVector3 target){
		return target;
	}

	// Collision of something smaller than a square, in the world
	private FixedVector3 CollisionResponse(WorldModel world, FixedVector3 origin, FixedVector3 target, FixedFloat size){
	
		// check boundaries
		target = FixedVector3.Clamp(target, FixedVector3.Zero, new FixedVector3(WorldModel.MaxWidth-1, WorldModel.MaxHeight-1, 0));

		// check collisions with neighbours
		FixedFloat vx, vy;
		vx = target.X - origin.X;
		vy = target.Y - origin.Y;

		int targetBlockX = (int)target.X;
		int targetBlockY = (int)target.Y;

		int originalBlockX = (int)origin.X;
		int originalBlockY = (int)origin.Y;

		bool collisionA, collisionB;
		FixedFloat tempRef;

		if (target.X != targetBlockX){
			int nextOriginBlockY = (int)(origin.Y + size);
			if (vx > 0){
				int nextBlockX = (int) (target.X + size);
				collisionA = world.MapValue(nextBlockX, originalBlockY) != 0;
				collisionB = (nextOriginBlockY != originalBlockY && world.MapValue(nextBlockX, nextOriginBlockY) != 0);

				if (collisionA || collisionB){
					tempRef = target.Y;
					if (TryToFit(collisionA, collisionB, ref tempRef, originalBlockY, size)){
						tempRef -= (tempRef - origin.Y) * 0.25f;
					}else{
						target.X = FixedFloat.Min(target.X, nextBlockX - size - 0.0001);
					}
					target.Y = tempRef;
				}
			}else if (vx < 0){
				collisionA = world.MapValue(targetBlockX, originalBlockY) != 0;
				collisionB = (nextOriginBlockY != originalBlockY && world.MapValue(targetBlockX, nextOriginBlockY) != 0);

				if (collisionA || collisionB){
					tempRef = target.Y;
					if (TryToFit(collisionA, collisionB, ref tempRef, originalBlockY, size)){
						tempRef -= (tempRef - origin.Y) * 0.25f;
					}else{
						target.X = targetBlockX + 1;
					}
					target.Y = tempRef;
				}
			}
		}

		if (target.Y != targetBlockY){
			int nextOriginBlockX = (int)(origin.X + size);
			if (vy > 0){
				int nextBlockY = (int) (target.Y + size);
				collisionA = world.MapValue(originalBlockX, nextBlockY) != 0;
				collisionB = nextOriginBlockX != originalBlockX && world.MapValue(nextOriginBlockX, nextBlockY) != 0;

				if (collisionA || collisionB){
					tempRef = target.X;
					if (TryToFit(collisionA, collisionB, ref tempRef, originalBlockX, size)){
						tempRef -= (tempRef - origin.X) * 0.25f;
					}else{
						target.Y = FixedFloat.Min(target.Y, nextBlockY - size - 0.0001);
					}
					target.X = tempRef;
				}
			}else if (vy < 0){
				collisionA = world.MapValue(originalBlockX, targetBlockY) != 0;
				collisionB = (nextOriginBlockX != originalBlockX && world.MapValue(nextOriginBlockX, targetBlockY) != 0);

				if (collisionA || collisionB){
					tempRef = target.X;
					if (TryToFit(collisionA, collisionB, ref tempRef, originalBlockX, size)){
						tempRef -= (tempRef - origin.X) * 0.25f;
					}else{
						target.Y = targetBlockY + 1;
					}
					target.X = tempRef;
				}
			}
		}

		return target;
	}

	private bool TryToFit(bool collisionA, bool collisionB, ref FixedFloat target, FixedFloat targetBlock, FixedFloat size){
		if (collisionA != collisionB){
			FixedFloat mantissa = (target - targetBlock);
			FixedFloat factor;
			if (collisionA && mantissa > 0.65f){
				factor = (mantissa - 0.65) / 0.35;
				target = FixedFloat.Min(target + factor * 0.06f, targetBlock + 1);
				return factor > 0.2f;
			}else if (collisionB && mantissa < 0.35f){
				factor = mantissa / 0.35;
				factor = 1 - factor;
				target = FixedFloat.Max(target - factor * 0.06f, targetBlock);
				return factor > 0.2f;
			}
		}
		return false;
	}


	private void UpdateTankDirection(TankModel tank, FixedVector3 velocity, FixedFloat targetAngle){

		if (velocity.Magnitude > 0.001f){
			FixedFloat movingAngle = FixedFloat.Atan2(velocity.Y, velocity.X);
			if (movingAngle < 0) movingAngle += FixedFloat.TwoPI;
			// Moving direction
			tank.movingAngle = UpdateDirection(tank.movingAngle, movingAngle, 0.095f, true);
		}

		// Turret
		tank.shootingTargetAngle = targetAngle;
		FixedFloat localTarget = targetAngle - tank.movingAngle;
		if (localTarget < 0) localTarget += FixedFloat.TwoPI;
		tank.shootingAngle = UpdateDirection(tank.shootingAngle, localTarget, 0.05f, false);
		/*
		tank.shootingTargetAngle = targetAngle;
		FixedFloat worldTurretAngle = tank.shootingAngle + tank.movingAngle;
		if (worldTurretAngle > FixedFloat.TwoPI) worldTurretAngle -= FixedFloat.TwoPI;

//		bool logStuff = worldTurretAngle != targetAngle;
//		if (logStuff)
//			UnityEngine.Debug.Log("turret local: " + tank.shootingAngle + ", global: " + worldTurretAngle);
		worldTurretAngle = UpdateDirection(worldTurretAngle, targetAngle, 0.05f);
		tank.shootingAngle = worldTurretAngle - tank.movingAngle;
		if (tank.shootingAngle < 0) tank.shootingAngle += FixedFloat.TwoPI;
//		if (logStuff)
//			UnityEngine.Debug.Log("turret new local: " + tank.shootingAngle + ", global: " + worldTurretAngle);
*/

	}


	private FixedFloat UpdateDirection(FixedFloat original, FixedFloat target, FixedFloat delta, bool goAround){

		FixedFloat difference = FixedFloat.Abs(target - original);
		if (difference > FixedFloat.PI){
			difference = FixedFloat.TwoPI - difference;
			if (target > original){
				target -= FixedFloat.TwoPI;
			}else {
				target += FixedFloat.TwoPI;
			}
		}
		if (difference != 0){
			if (difference < delta + 0.001f) {
				original = target;
			}else {
				if (goAround && difference > FixedFloat.HalfPI) {
					target = target + FixedFloat.PI;
					if (target > FixedFloat.TwoPI) target -= FixedFloat.TwoPI;
				}
				if (original < target) {
					original += delta;
					if (original > FixedFloat.TwoPI){
						original -= FixedFloat.TwoPI;
					}
				}else{
					original -= delta;
					if (original < 0){
						original += FixedFloat.TwoPI;
					}
				}
			}
		}
		return original;
	}

	
//	// old method
//	private FixedVector3 CollisionResponse(WorldModel world, FixedVector3 origin, FixedVector3 target, FixedFloat size){
//		
//		// check boundaries
//		target = FixedVector3.Clamp(target, FixedVector3.Zero, new FixedVector3(WorldModel.MaxWidth-1, WorldModel.MaxHeight-1, 0));
//		
//		int moveBackX, moveBackY;
//		moveBackX = target.X - origin.X < 0 ? 1 : 0;
//		moveBackY = target.Y - origin.Y < 0 ? 1 : 0;
//		
//		int xi, yi, xf, yf;
//		xi = (int)(target.X);
//		yi = (int)(target.Y);
//		xf = (int)(target.X + size);
//		yf = (int)(target.Y + size);
//		
//		FixedFloat newX, newY;
//		
//		int mapValue;
//		for (int x = xi ; x <= xf ; ++x) {
//			for (int y = yi ; y <= yf ; ++y) {
//				mapValue = world.MapValue(x, y);
//				if (mapValue != 0){
//					// collision! Try an adjustment
//					newX = xi + moveBackX;
//					newY = yi + moveBackY;
//					if (FixedFloat.Abs(newX - target.X) < FixedFloat.Abs(newY - target.Y)) {
//						target.X = newX;
//					}else {
//						target.Y = newY;
//					}
//					// one more time, recursively
//					return CollisionResponse(world, origin, target);
//				}
//			}
//		}
//		
//		return target;
//	}


	protected override void Update(WorldModel model){
		
		HandlePlayerConnections(model);

		UpdateTanks(model);

	}
	
}

