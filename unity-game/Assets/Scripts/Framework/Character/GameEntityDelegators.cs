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
			if (model.anchoredEntities == null || model.anchoredEntities.Count <= anchorId) return null;
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

		private int collidingTeam;

		public CollisionEntityDelegator(int collidingTeam){
			this.collidingTeam = collidingTeam;
		}

		public ModelReference GetEntityReference(GameEntityModel model){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller == null) return null;
			if (collidingTeam >= 0) {
				if (WorldUtils.GetEntityTeam(controller.lastCollisionEntityId) == collidingTeam) {
					return controller.lastCollisionEntityId;
				} else {
					return null;
				}
			} else {
				return controller.lastCollisionEntityId;
			}
		}

	}


	// A hitten entity, checks team optionally
	public class HittenEntityDelegator: GameEntityReferenceDelegator{

		private int hittenTeam;

		public HittenEntityDelegator(int hittenTeam){
			this.hittenTeam = hittenTeam;
		}

		public ModelReference GetEntityReference(GameEntityModel model){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller == null || controller.lastHits.Count == 0) return null;
			if (hittenTeam >= 0){
				foreach (HitInformation hit in controller.lastHits){
					if (WorldUtils.GetEntityTeam(hit.entityId) == hittenTeam) {
						return hit.entityId;
					}
				}
				return null;
			}else {
				return controller.lastHits[0].entityId;
			}
		}

	}

	// A hitter entity (this version simplified, no checks made over the hit information)
	public class HitterEntityDelegator: GameEntityReferenceDelegator{

		private int hitterTeam;

		public HitterEntityDelegator(int hitterTeam){
			this.hitterTeam = hitterTeam;
		}

		public ModelReference GetEntityReference(GameEntityModel model){
			GameEntityController controller = model.Controller() as GameEntityController;
			if (controller == null || controller.lastHurts.Count == 0) return null;
			if (hitterTeam >= 0) {
				foreach (HitInformation hurt in controller.lastHurts) {
					if (WorldUtils.GetEntityTeam(hurt.entityId) == hitterTeam) {
						return hurt.entityId;
					}
				}
				return null;
			} else {
				return controller.lastHurts[0].entityId;
			}
		}

	}


	// TODO: other delegators, such as entities from distance, etc..


}
