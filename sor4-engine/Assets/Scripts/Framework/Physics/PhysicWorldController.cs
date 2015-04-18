//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18063
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;


namespace RetroBread{


public class PhysicWorldController: Controller<PhysicWorldModel>{
		
	// world static planes, serializable because it can be read from a file
	private SerializableList<PhysicPlaneModel> staticPlanes = null;

	// We keep track of the current world loaded so that
	// if the model world changes, we need to load the new world
	private string currentWorld;

	// When setting next world, it's stored here to take effect on late update
	private string nextWorld;

	// TODO: we can rework this a bit later, perhaps tell what was the old world or even do it some other way
	// A delegate for when the game requests the world to change
	// This should be used to load the respective static planes
	public delegate void OnWorldChangedDelegate(out SerializableList<PhysicPlaneModel> staticPlanes);
	public OnWorldChangedDelegate onWorldChanged = null;

//	// we keep the index of the world here to access it from the game state
//	private ModelReference worldModelIndex = new ModelReference();


	// A delegate for physic points added / removed on the world
	public delegate void OnPointModelChanged(PhysicPointModel model, object context);

	// Keep track of the callbacks for each model
	private Dictionary<PhysicPointModel, Eppy.Tuple<OnPointModelChanged, object>> addPointCallbacks;
	private Dictionary<PhysicPointModel, Eppy.Tuple<OnPointModelChanged, object>> removePointCallbacks;

	// A delegate for physic planes added / removed on the world
	public delegate void OnPlaneModelChanged(PhysicPlaneModel model, object context);
	
	// Keep track of the callbacks for each model
	private Dictionary<PhysicPlaneModel, Eppy.Tuple<OnPlaneModelChanged, object>> addPlaneCallbacks;
	private Dictionary<PhysicPlaneModel, Eppy.Tuple<OnPlaneModelChanged, object>> removePlaneCallbacks;



	// Constructor
	public PhysicWorldController(PhysicWorldModel model){
		addPointCallbacks 	 = new Dictionary<PhysicPointModel, Eppy.Tuple<OnPointModelChanged, object>>();
		removePointCallbacks = new Dictionary<PhysicPointModel, Eppy.Tuple<OnPointModelChanged, object>>();
		addPlaneCallbacks 	 = new Dictionary<PhysicPlaneModel, Eppy.Tuple<OnPlaneModelChanged, object>>();
		removePlaneCallbacks = new Dictionary<PhysicPlaneModel, Eppy.Tuple<OnPlaneModelChanged, object>>();
		UpdateWorld(model);
	}

	public override bool IsCompatible(PhysicWorldModel originalModel, PhysicWorldModel newModel){
		return currentWorld.Equals(newModel.worldName);
	}


#region Update

	// This will take effect on next frame only
	private void ApplyGravityToPoint(PhysicWorldModel world, PhysicPointModel pointModel, PhysicPointController pointController) {
		pointController.AddVelocityAffector(PhysicPointModel.defaultVelocityAffectorName, world.gravity);
	}


	// Find collisions between a point model and a set of world planes
	// For each collision found, notify point and plane controllers so that they can react to the collision
	private void CheckCollisionsAgainstPlanes(PhysicWorldModel world, PhysicPointModel pointModel, PhysicPointController pointController, List<PhysicPlaneModel> planes){
		pointController = pointModel.Controller() as PhysicPointController;
		if (pointController == null) return;

		PhysicPlaneController planeController;
		FixedVector3 intersection;

		// Do a few iterations until collisions get stable, or we reach a limit on iterations
		bool collisionsAreStable = false;
		for (int i = 0 ; i < 3 && !collisionsAreStable ; ++i){
//			if (i > 0) {
//				UnityEngine.Debug.Log("Collision iteration " + i);
//			}
			collisionsAreStable = true;
			foreach(PhysicPlaneModel planeModel in planes){
				if (planeModel.CheckIntersection(pointModel, out intersection)){
					collisionsAreStable &= pointController.OnCollision(world, pointModel, planeModel, intersection);
					planeController = planeModel.Controller() as PhysicPlaneController;
					if (planeController != null){
						collisionsAreStable &= planeController.OnCollision(world, pointModel, planeModel, intersection);
					}
//					if (!collisionsAreStable){
//						// already unstable, break iteration
//						break;
//					}
				}
			}
		}
//		if (!collisionsAreStable){
//			UnityEngine.Debug.LogWarning("Unstable colisions!");
//		}
	}


	public void SetWorld(PhysicWorldModel world, string worldName){
		if (world != null){
			nextWorld = worldName;
		}
	}


	// Check collisions between physic models, and apply gravity to them
	protected override void Update(PhysicWorldModel world){
	
		PhysicPointModel pointModel;
		PhysicPointController pointController;
		foreach(uint pointModelId in world.pointModels) {
			pointModel = StateManager.state.GetModel(pointModelId) as PhysicPointModel;
			if (pointModel == null) {
				// someone removed it in some other way!!
				continue;
			}
			pointController = pointModel.Controller() as PhysicPointController;
			if (pointController == null){
				// not controlled, can't react to anything
				continue;
			}

			// point VS plane collisions
			List<PhysicPlaneModel> allPlanes = new List<PhysicPlaneModel>();
			// static world planes
			if (staticPlanes != null) {
				allPlanes.AddRange(staticPlanes);
			}
			// dynamic planes
			if (world.planeModels.Count > 0) {
				PhysicPlaneModel planeModel;
				foreach (uint planeId in world.planeModels) {
					planeModel = StateManager.state.GetModel(planeId) as PhysicPlaneModel;
					if (planeModel == null){
						// someone removed it in some other way!!
						continue;
					}
					allPlanes.Add(planeModel);
				}
			}

			// apply gravity
			ApplyGravityToPoint(world, pointModel, pointController);

			CheckCollisionsAgainstPlanes(world, pointModel, pointController, allPlanes);

		}
	}


	void UpdateWorld(PhysicWorldModel model){
		if (currentWorld == null || !currentWorld.Equals(model.worldName)){
			currentWorld = model.worldName;
			if (onWorldChanged != null){
				onWorldChanged(out staticPlanes);
			}else {
				staticPlanes = null;
			}
		}
	}

	protected override void PostUpdate(PhysicWorldModel model){
		if (nextWorld != null){
			model.worldName = nextWorld;
			nextWorld = null;
			UpdateWorld(model);
		}
	}

#endregion


#region Add/Rem Points

	// Add a Physic Point to the world
	public ModelReference AddPoint(PhysicWorldModel world, PhysicPointModel pointModel, OnPointModelChanged callback = null, object context = null){
		addPointCallbacks[pointModel] = new Eppy.Tuple<OnPointModelChanged, object>(callback, context);
		return StateManager.state.AddModel(pointModel, OnPointAdded, world);
	}

	// Remove a Physic Point from the world
	public void RemovePoint(PhysicWorldModel world, PhysicPointModel pointModel, OnPointModelChanged callback = null, object context = null){
		removePointCallbacks[pointModel] = new Eppy.Tuple<OnPointModelChanged, object>(callback, context);
		StateManager.state.RemoveModel(pointModel, OnPointRemoved, world);
	}


	// Delegate for when a point is added, it redirects to the caller delegate
	private void OnPointAdded(Model model, object context){
		PhysicWorldModel worldModel = context as PhysicWorldModel;
		Eppy.Tuple<OnPointModelChanged, object> callbackTuple;
		PhysicPointModel pointModel = model as PhysicPointModel;
		if (pointModel == null || worldModel == null){
			// Something went wrong!
			return;
		}
		if (addPointCallbacks.TryGetValue(pointModel, out callbackTuple)){
			addPointCallbacks.Remove(pointModel);
			worldModel.pointModels.Add(model.Index);
			if (callbackTuple.Item1 != null){
				callbackTuple.Item1(pointModel, callbackTuple.Item2);
			}
		}
	}

	// Delegate for when a point is removed, it redirects to the caller delegate
	private void OnPointRemoved(Model model, object context){
		Eppy.Tuple<OnPointModelChanged, object> callbackTuple;
		PhysicPointModel pointModel = model as PhysicPointModel;
		PhysicWorldModel worldModel = context as PhysicWorldModel;
		if (pointModel == null || worldModel == null){
			// Something went wrong!
			return;
		}
		if (removePointCallbacks.TryGetValue(pointModel, out callbackTuple)){
			if (callbackTuple.Item1 != null){
				callbackTuple.Item1(pointModel, callbackTuple.Item2);
			}
			removePointCallbacks.Remove(pointModel);
			worldModel.pointModels.Remove(model.Index);
		}
	}

#endregion

#region Add/Rem Planes

	// Add a Physic Plane to the world
	public void AddPlane(PhysicWorldModel worldModel, OnPlaneModelChanged callback, object context, params FixedVector3[] paramPoints){
		PhysicPlaneModel planeModel = new PhysicPlaneModel(paramPoints);
		addPlaneCallbacks[planeModel] = new Eppy.Tuple<OnPlaneModelChanged, object>(callback, context);
		StateManager.state.AddModel(planeModel, OnPlaneAdded, worldModel);
	}

	// Add a Physic Plane to the world
	public void AddPlane(PhysicWorldModel worldModel, PhysicPlaneModel planeModel, OnPlaneModelChanged callback = null, object context = null){
		addPlaneCallbacks[planeModel] = new Eppy.Tuple<OnPlaneModelChanged, object>(callback, context);
		StateManager.state.AddModel(planeModel, OnPlaneAdded, worldModel);
	}
	
	// Remove a Physic Plane from the world
	public void RemovePlane(PhysicWorldModel worldModel, PhysicPlaneModel planeModel, OnPlaneModelChanged callback = null, object context = null){
		removePlaneCallbacks[planeModel] = new Eppy.Tuple<OnPlaneModelChanged, object>(callback, context);
		StateManager.state.RemoveModel(planeModel, OnPlaneRemoved, worldModel);
	}
	
	
	// Delegate for when a plane is added, it redirects to the caller delegate
	private void OnPlaneAdded(Model model, object context){
		Eppy.Tuple<OnPlaneModelChanged, object> callbackTuple;
		PhysicPlaneModel planeModel = model as PhysicPlaneModel;
		PhysicWorldModel worldModel = context as PhysicWorldModel;
		if (planeModel == null || worldModel == null){
			// Something went wrong!
			return;
		}
		if (addPlaneCallbacks.TryGetValue(planeModel, out callbackTuple)){
			addPlaneCallbacks.Remove(planeModel);
			worldModel.planeModels.Add(model.Index);
			if (callbackTuple.Item1 != null){
				callbackTuple.Item1(planeModel, callbackTuple.Item2);
			}
		}
	}
	
	// Delegate for when a plane is removed, it redirects to the caller delegate
	private void OnPlaneRemoved(Model model, object context){
		Eppy.Tuple<OnPlaneModelChanged, object> callbackTuple;
		PhysicPlaneModel planeModel = model as PhysicPlaneModel;
		PhysicWorldModel worldModel = context as PhysicWorldModel;
		if (planeModel == null || worldModel == null){
			// Something went wrong!
			return;
		}
		if (removePlaneCallbacks.TryGetValue(planeModel, out callbackTuple)){
			if (callbackTuple.Item1 != null){
				callbackTuple.Item1(planeModel, callbackTuple.Item2);
			}
			removePlaneCallbacks.Remove(planeModel);
			worldModel.planeModels.Remove(model.Index);
		}
	}

#endregion

}



}

