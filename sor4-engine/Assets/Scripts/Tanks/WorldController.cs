using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class WorldController:Controller<WorldModel>{

	public const uint totalGameFrames = 9997200; // 2 minutes

	public static FixedFloat maxTankVelocity = 0.04f;
	public static FixedFloat maxBulletVelocity = 0.12f;

	private static FixedVector3 worldLimit;


#region Initializations

	static WorldController(){
		// setup imutable stuff
		worldLimit = new FixedVector3(WorldModel.MaxWidth-1, WorldModel.MaxHeight-1, 0);
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


#endregion


#region Auxiliar Functions

	private bool IsObstacleForTank(WorldModel world, int x, int y){
		int blockValue = world.MapValue(x, y);
		return blockValue != 0 && blockValue != 3;
	}

	private bool IsObstacleForBullet(WorldModel world, int x, int y){
		int blockValue = world.MapValue(x, y);
		return blockValue == 1 || blockValue == 4;
	}
	
#endregion


#region Tanks

	private void UpdateTanks(WorldModel world){

		FixedVector3 previousTankPosition;
		FixedVector3 velocity = FixedVector3.Zero;
		PlayerInputModel inputModel;
		PlayerInputController inputController;
		FixedFloat targetAngle;
		TankModel tank;
		// For each tank update it's position and orientation
		for (int tankId = 0 ; tankId < world.tanks.Length ; ++tankId){
			tank = world.tanks[tankId];
			if (tank != null){
				previousTankPosition = tank.position;
				inputModel = StateManager.state.GetModel(tank.inputModelRef) as PlayerInputModel;
				if (inputModel != null && inputModel.axis[0] != FixedVector3.Zero) {
					velocity = new FixedVector3(inputModel.axis[0].X * maxTankVelocity, inputModel.axis[0].Z * maxTankVelocity, 0);
					if (UpdateTankPosition(world, tank, inputModel, velocity)){
						if (tank.position != previousTankPosition){
							velocity = tank.position - previousTankPosition;
						}
						//velocity = FixedVector3.Lerp(tank.position - previousTankPosition, velocity, 0.002f);
					}
					targetAngle = FixedFloat.Atan2(inputModel.axis[0].Z, inputModel.axis[0].X);
				}else {
					targetAngle = tank.turretTargetAngle;
				}
				UpdateTankDirection(tank, velocity, targetAngle);

				// check fire!!
				if (inputModel != null) {
					inputController = inputModel.Controller() as PlayerInputController;
					if (inputController != null && inputController.IsButtonPressed(inputModel, 0)){
						BulletModel bullet = world.CreateBulletForPlayer(tankId);
						if (bullet != null){
							FixedFloat fireAngle = tank.orientationAngle + tank.turretAngle;
							bullet.velocity = new FixedVector3(FixedFloat.Cos(fireAngle), FixedFloat.Sin(fireAngle), 0);
							bullet.position = tank.position + new FixedVector3(0.5f, 0.5f, 0) + bullet.velocity * (0.51f - maxBulletVelocity);
							bullet.velocity *= maxBulletVelocity;
						}
					}
				}
			}
		}
	}


	private bool UpdateTankPosition(WorldModel world, TankModel tank, PlayerInputModel inputModel, FixedVector3 velocity){

		// Update position based on input and tank orientation
		// NOTE: remove dot product here if necessary to gain performance here
		inputModel.axis[0].Normalize();
		FixedVector3 tankOrientation = new FixedVector3(FixedFloat.Cos(tank.orientationAngle), FixedFloat.Sin(tank.orientationAngle), 0);
		FixedFloat dotProduct = FixedVector3.Dot(velocity, tankOrientation);
		velocity = velocity.Normalized * FixedFloat.Abs(dotProduct);

		FixedVector3 targetPosition = tank.position + velocity;

		// TODO: check circular collisions with other tanks

		// check square collisions with the world
		bool hardCollison;
		tank.position = CollisionResponse(world, tank.position, targetPosition, 0.999f, out hardCollison);
		return hardCollison;
	}


	// Collision of something smaller than a square, in the world
	private FixedVector3 CollisionResponse(WorldModel world, FixedVector3 origin, FixedVector3 target, FixedFloat size, out bool hardCollision){
	
		// check boundaries
		FixedVector3 clampedTarget = FixedVector3.Clamp(target, FixedVector3.Zero, worldLimit);
		hardCollision = clampedTarget != target;
		target = clampedTarget;

		// check collisions with neighbours
		FixedFloat vx, vy;
		vx = target.X - origin.X;
		vy = target.Y - origin.Y;

		int targetBlockX = (int)target.X;
		int targetBlockY = (int)target.Y;

		int originalBlockX = (int)origin.X;
		int originalBlockY = (int)origin.Y;

		bool collisionA, collisionB;
		bool gotFirstCollision = false;
		FixedFloat tempRef;

		// currently giving priority to vertical movement over horizontal
		if (target.Y != targetBlockY){
			int nextOriginBlockX = (int)(origin.X + size);
			if (vy > 0){
				int nextBlockY = (int) (target.Y + size);
				collisionA = IsObstacleForTank(world, originalBlockX, nextBlockY);
				collisionB = nextOriginBlockX != originalBlockX && IsObstacleForTank(world, nextOriginBlockX, nextBlockY);
				
				if (collisionA || collisionB){
					tempRef = target.X;
					if (TryToFit(collisionA, collisionB, ref tempRef, originalBlockX, size)){
						tempRef -= (tempRef - origin.X) * 0.25f;
					}else{
						target.Y = FixedFloat.Min(target.Y, nextBlockY - size - 0.0001);
						hardCollision = true;
					}
					target.X = tempRef;
					gotFirstCollision = collisionA != collisionB;
				}
			}else if (vy < 0){
				collisionA = IsObstacleForTank(world, originalBlockX, targetBlockY);
				collisionB = nextOriginBlockX != originalBlockX && IsObstacleForTank(world, nextOriginBlockX, targetBlockY);
				
				if (collisionA || collisionB){
					tempRef = target.X;
					if (TryToFit(collisionA, collisionB, ref tempRef, originalBlockX, size)){
						tempRef -= (tempRef - origin.X) * 0.25f;
					}else{
						target.Y = targetBlockY + 1;
						hardCollision = true;
					}
					target.X = tempRef;
					gotFirstCollision = collisionA != collisionB;
				}
			}
		}


		if (!gotFirstCollision && target.X != targetBlockX){
			int nextOriginBlockY = (int)(origin.Y + size);
			if (vx > 0){
				int nextBlockX = (int) (target.X + size);
				collisionA = IsObstacleForTank(world, nextBlockX, originalBlockY);
				collisionB = nextOriginBlockY != originalBlockY && IsObstacleForTank(world, nextBlockX, nextOriginBlockY);

				if (collisionA || collisionB){
					tempRef = target.Y;
					if (TryToFit(collisionA, collisionB, ref tempRef, originalBlockY, size)){
						tempRef -= (tempRef - origin.Y) * 0.25f;

					}else{
						target.X = FixedFloat.Min(target.X, nextBlockX - size - 0.0001);
						hardCollision = true;
					}
					target.Y = tempRef;
				}
			}else if (vx < 0){
				collisionA = IsObstacleForTank(world, targetBlockX, originalBlockY);
				collisionB = nextOriginBlockY != originalBlockY && IsObstacleForTank(world, targetBlockX, nextOriginBlockY);

				if (collisionA || collisionB){
					tempRef = target.Y;
					if (TryToFit(collisionA, collisionB, ref tempRef, originalBlockY, size)){
						tempRef -= (tempRef - origin.Y) * 0.25f;
					}else{
						target.X = targetBlockX + 1;
						hardCollision = true;
					}
					target.Y = tempRef;
				}
			}
		}

		return target;
	}

	private bool TryToFit(bool collisionA, bool collisionB, ref FixedFloat target, FixedFloat targetBlock, FixedFloat size){
		if (collisionA != collisionB){
			FixedFloat mantissa = (target - targetBlock);
			FixedFloat factor;
			if (collisionA && mantissa > 0.6f){
				factor = (mantissa - 0.6) / 0.4;
				target = FixedFloat.Min(target + factor * 0.05f, targetBlock + 1);
				return factor > 0.5f;
			}else if (collisionB && mantissa < 0.4f){
				factor = mantissa / 0.4;
				factor = 1 - factor;
				target = FixedFloat.Max(target - factor * 0.05f, targetBlock);
				return factor > 0.5f;
			}
		}
		return false;
	}


	private void UpdateTankDirection(TankModel tank, FixedVector3 velocity, FixedFloat targetAngle){

		if (tank.position.X > 5) {
			int a = 0;
			a = a + 1;
		}

		if (velocity.Magnitude > 0.001f){
			FixedFloat movingAngle = FixedFloat.Atan2(velocity.Y, velocity.X);
			// Moving direction
			tank.orientationAngle = UpdateDirection(tank.orientationAngle, movingAngle, 0.09f, true, ref tank.movingBackwards);
		}

		// Turret
		tank.turretTargetAngle = targetAngle;
		FixedFloat localTarget = targetAngle - tank.orientationAngle;
		bool falseBool = false;
		tank.turretAngle = UpdateDirection(tank.turretAngle, localTarget, 0.05f, false, ref falseBool);

	}


	private FixedFloat UpdateDirection(FixedFloat original, FixedFloat target, FixedFloat delta, bool useBackwardsDirection, ref bool isMovingBackwards){
		// Normalize angles
		original = FixedFloat.NormalizedAngle(original);
		target = FixedFloat.NormalizedAngle(target);

		FixedFloat difference = FixedFloat.Abs(target - original);

		isMovingBackwards = false;
		if ((useBackwardsDirection && difference > FixedFloat.HalfPI) || difference > FixedFloat.PI){
			difference = FixedFloat.Abs(FixedFloat.TwoPI - difference);
		}
		if (difference != 0){
			if (difference < delta + 0.001f) {
				original = target;
			}
			else if (useBackwardsDirection && FixedFloat.Abs(difference - FixedFloat.PI) < delta + 0.001f) {
				isMovingBackwards = true;
				original = target + FixedFloat.PI;
			}else {
				if (useBackwardsDirection && difference > FixedFloat.HalfPI) {
					isMovingBackwards = true;
					target = target + FixedFloat.PI;
					if (target > FixedFloat.TwoPI) target -= FixedFloat.TwoPI;
				}
				// recheck if target is left or right, including around the 
				if (FixedFloat.Abs(target - original) > FixedFloat.PI){
					if (target > original) target -= FixedFloat.TwoPI;
					else original -= FixedFloat.TwoPI;
				}
				if (original < target) {
					original += delta;
				}else{
					original -= delta;
				}
			}
		}
		return original;
	}


#endregion


#region Bullets
	
	private void UpdateBullets(WorldModel world){

		// For each bullet update it's position and check for collisions
		FixedVector3 lastPos;
		int previousBlockX, previousBlockY;
		int blockX, blockY;
		bool collidedX, collidedY;
		foreach (BulletModel bullet in world.bullets){
			if (bullet != null){
				// save previous location
				lastPos = bullet.position;

				// Move!
				bullet.position += bullet.velocity;

				// check collisions against world limit
				collidedX = false;
				collidedY = false;;
				if (bullet.position.X < 0) {
					bullet.position.X = 0;
					collidedX = true;
				}else if (bullet.position.X >= WorldModel.MaxWidth) {
					bullet.position.X = WorldModel.MaxWidth - 0.001f;
					collidedX = true;
				}
				if (bullet.position.Y < 0) {
					bullet.position.Y = 0;
					collidedY = true;
				}else if (bullet.position.Y >= WorldModel.MaxHeight) {
					bullet.position.Y = WorldModel.MaxHeight - 0.001f;
					collidedY = true;
				}


				// check if changing matrix cell
				previousBlockX = (int)lastPos.X;
				previousBlockY = (int)lastPos.Y;
				blockX = (int)bullet.position.X;
				blockY = (int)bullet.position.Y;

				// if so, check collision with the world
				if ((blockX != previousBlockX || blockY != previousBlockY) && IsObstacleForBullet(world, blockX, blockY)) {
					if (!collidedX && blockX != previousBlockX && IsObstacleForBullet(world, blockX, previousBlockY)) {
						if (bullet.velocity.X > 0) bullet.position.X = blockX;
						else bullet.position.X = blockX + 1;
						collidedX = true;
					}
					if (!collidedY && blockY != previousBlockY && IsObstacleForBullet(world, previousBlockX, blockY)){
						if (bullet.velocity.Y > 0) bullet.position.Y = blockY;
						else bullet.position.Y = blockY + 1;
						collidedY = true;
					}
					// second round
					// warning TODO: disable this if not going to bounce on walls
					if (!collidedX && !collidedY) {
						bool newCollision = IsObstacleForBullet(world, blockX, blockY);
						if (!collidedX && blockX != previousBlockX && newCollision){
							if (bullet.velocity.X > 0) bullet.position.X = blockX;
							else bullet.position.X = blockX + 1;
							collidedX = true;
						}
						if (!collidedY && blockY != previousBlockY && newCollision){
							if (bullet.velocity.Y > 0) bullet.position.Y = blockY;
							else bullet.position.Y = blockY + 1;
							collidedY = true;
						}
					}
				}

				// collision response
				if (collidedX) bullet.velocity.X *= -1;
				if (collidedY) bullet.velocity.Y *= -1;
			}
		}
	}


#endregion


	protected override void Update(WorldModel model){
		
		HandlePlayerConnections(model);
		UpdateBullets(model);
		UpdateTanks(model);
	}
	
}

