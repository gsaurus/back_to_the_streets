using System;
using System.Collections.Generic;


namespace RetroBread{


	public class PhysicPlaneController: Controller<PhysicPlaneModel>{

		// TODO: set position, which also updates the oldPosition..
		private FixedVector3 newPosition;
		private bool hasNewPosition;

		// Update natural physics 
		protected override void Update(PhysicPlaneModel model){
			model.lastOriginPosition = model.origin;
		}

		// Post update used to consolidate direct modifications
		protected override void PostUpdate(PhysicPlaneModel model){
			// New position (teleport)
			if (hasNewPosition){
				model.origin = newPosition;
				model.lastOriginPosition = newPosition;
				hasNewPosition = false;
			}
		}

		// Teleport to a new position
		public void SetPosition(PhysicPlaneModel model, FixedVector3 newPos){
			if (StateManager.state.IsPostUpdating){
				model.lastOriginPosition = model.origin = newPos;
				hasNewPosition = false;
			}else{
				newPosition = newPos;
				hasNewPosition = true;
			}
		}

		// Collision reaction, return true if collision is considered stable (no position modifications occured)
		public virtual bool OnCollision(PhysicWorldModel world, PhysicPointModel pointModel, PhysicPlaneModel planeModel, FixedVector3 intersection){
			// Nothing by default
			return true;
		}

	}


}

