using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{


	public enum EntityDelegatorType:int {
		anchor = 0,
		parent = 1,
		collision = 2,
		hitten = 3,
		hitter = 4
	}


	// Anchored entity
	public class AnchoredEntityDelegator: GameEntityReferenceDelegator{

		private int anchorId;

		public AnchoredEntityDelegator(int anchorId){
			this.anchorId = anchorId;
		}

		public ModelReference GetEntityReference(GameEntityModel model){
			if (model.anchoredEntities.Count <= anchorId) return null;
			return model.anchoredEntities[anchorId];
		}

	}


	// Parent entity
	public class ParentEntityDelegator: GameEntityReferenceDelegator{
		
		public ModelReference GetEntityReference(GameEntityModel model){
			return model.parentEntity;
		}

	}


	// Colliding entity
	public class CollisionEntityDelegator: GameEntityReferenceDelegator{

		public ModelReference GetEntityReference(GameEntityModel model){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller == null) return null;
			return controller.lastCollisionEntityId;
		}

	}


	// A hitten entity (this version simplified, no checks made over the hit information)
	public class HittenEntityDelegator: GameEntityReferenceDelegator{

		public ModelReference GetEntityReference(GameEntityModel model){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller == null || controller.lastHits.Count == 0) return null;
			return controller.lastHits[0].entityId;
		}

	}

	// A hitter entity (this version simplified, no checks made over the hit information)
	public class HitterEntityDelegator: GameEntityReferenceDelegator{

		public ModelReference GetEntityReference(GameEntityModel model){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller == null || controller.lastHurts.Count == 0) return null;
			return controller.lastHurts[0].entityId;
		}

	}


	// TODO: other delegators, such as entities from distance, etc..


}
