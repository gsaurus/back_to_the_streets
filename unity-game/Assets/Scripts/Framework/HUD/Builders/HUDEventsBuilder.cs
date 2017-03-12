//using System;
//using System.Collections.Generic;
//using UnityEngine;
//
//namespace RetroBread{
//
//
//	public static class HudEventsBuilder {
//
//		// Event builders indexed by type directly on array
//		private delegate GenericEvent<HUDViewBehaviour> BuilderAction(Storage.GenericParameter param);
//		private static BuilderAction[] builderActions = {
//			BuildPlayAnimation,					// 0: play(win)
//			BuildSetAnimationParam,				// 1: energy=23
//			BuildSetTexture,					// 2: texture(axel)
//			BuildSetText,						// 3: label(name)
//			BuildSpawnEffect					// 4: spawnFX(sparks)
//		};
//
//
//		// The public builder method
//		public static GenericEvent<HUDViewBehaviour> Build(Storage.HUD hudData, int[] eventIds){
//			List<GenericEvent<HUDViewBehaviour>> events = new List<GenericEvent<HUDViewBehaviour>>(eventIds.Length);
//			GenericEvent<HUDViewBehaviour> e;
//			foreach (int eventId in eventIds) {
//				e = BuildFromParameter(hudData.genericParameters[eventId]);
//				if (e != null) {
//					events.Add(e);
//				}
//			}
//			if (events.Count > 0) {
//				if (events.Count == 1) {
//					return events[0];
//				}
//				return new EventsList<HUDViewBehaviour>(null, events);
//			}
//			return null;
//		}
//
//
//		// Build a single event
//		private static GenericEvent<HUDViewBehaviour> BuildFromParameter(Storage.GenericParameter parameter){
//			int callIndex = parameter.type;
//			if (callIndex < builderActions.Length) {
//				return builderActions[callIndex](parameter);
//			}
//			Debug.Log("HudEventsBuilder: Unknown event type: " + parameter.type);
//			return null;
//		}
//
//
//
//		// Build a point
//		static FixedVector3 BuildFixedVector3(Storage.GenericParameter parameter, int startFloatIndex = 0){
//			FixedFloat x = parameter.SafeFloat(startFloatIndex);
//			FixedFloat y = parameter.SafeFloat(startFloatIndex + 1);
//			FixedFloat z = parameter.SafeFloat(startFloatIndex + 2);
//			return new FixedVector3(x, y, z);
//		}
//
//
//#region Helper Classes
//
//		// Condition that selects a different condition depending on hit/grab delegation options
//		private class HUDEventDelegationChecker:GenericEvent<HUDViewBehaviour>{
//			private GenericEvent<HUDViewBehaviour> nonDelegationEvent;
//			private GenericEvent<HUDViewBehaviour> delegationEvent;
//
//			public HUDEventDelegationChecker(GenericEvent<HUDViewBehaviour> nonDelegationEvent, GenericEvent<HUDViewBehaviour> delegationEvent){
//				this.nonDelegationEvent = nonDelegationEvent;
//				this.delegationEvent = delegationEvent;
//			}
//
//		public override void Execute(HUDViewBehaviour model, List<GenericEventSubject<HUDViewBehaviour>> subjects){
//				Storage.HUDObject hudObj = model.hudObjectData;
//				if (hudObj.attackAndGrabDelegation) {
//					delegationEvent.Execute(model, subjects);
//				} else {
//					nonDelegationEvent.Execute(model, subjects);
//				}
//			}
//		}
//
//		// Hitten or grabbed character's delegation
//		private class HUDInteractionDelegationEvent:GenericEvent<HUDViewBehaviour>{
//		public delegate void EventExecutionDelegate(HUDViewBehaviour model, List<GenericEventSubject<HUDViewBehaviour>> subjects, GameEntityModel entity);
//			private EventExecutionDelegate eventExecutionDelegate;
//			private ModelReference lastEntityReference = null;
//
//			public HUDInteractionDelegationEvent(EventExecutionDelegate eventDelegate){
//				eventExecutionDelegate = eventDelegate;
//			}
//
//			// Execute event trough the delegate
//		public override void Execute(HUDViewBehaviour model, List<GenericEventSubject<HUDViewBehaviour>> subjects){
//				Storage.HUDObject hudObj = model.hudObjectData;
//				if (hudObj == null) return;
//				ModelReference exception = lastEntityReference == null ? new ModelReference() : lastEntityReference;
//				GameEntityModel entity = WorldUtils.GetInteractionEntityWithEntityFromTeam(hudObj.teamId, hudObj.playerId, exception);
//				if (entity == null && lastEntityReference != null && lastEntityReference != ModelReference.InvalidModelIndex) {
//					entity = StateManager.state.GetModel(lastEntityReference) as GameEntityModel;
//				}
//				lastEntityReference = entity == null ? new ModelReference() : entity.Index;
//				if (entity != null) {
//					eventExecutionDelegate(model, subjects, entity);
//				}
//			}
//
//		}
//
//#endregion
//
//
//#region Events
//
//
////		new BuildPlayAnimation(),					// 0: play(win)
////		new BuildSetAnimationParam(),				// 1: energy=23
////		new BuildSetTexture(),						// 2: texture(axel)
////		new BuildSetText(),							// 3: label(name)
////		new BuildSpawnEffect()						// 4: spawnFX(sparks)
//
//
//		// 0: play(win)
//		private static GenericEvent<HUDViewBehaviour> BuildPlayAnimation(Storage.GenericParameter parameter){
//			return new SimpleEvent<HUDViewBehaviour>(null,
//				delegate(HUDViewBehaviour model){
//					Animator animator = model.gameObject.GetComponent<Animator>();
//					if (animator == null) return;
//					animator.CrossFade(parameter.SafeString(0), (float)parameter.SafeFloat(0));
//				}
//			);
//		}
//
//
//		// 1: energy=23
//		private static GenericEvent<HUDViewBehaviour> BuildSetAnimationParam(Storage.GenericParameter parameter){
//			string paramName = parameter.SafeString(0);
//			float delay = (float) parameter.SafeFloat(1);
//			float duration = (float) parameter.SafeFloat(2);
//			switch (parameter.SafeInt(0)) {
//				case 0: // variable
//					return new HUDEventDelegationChecker(
//						new SimpleEvent<HUDViewBehaviour>(null,
//							delegate(HUDViewBehaviour model){
//								Storage.HUDObject hudObj = model.hudObjectData;
//								if (hudObj == null) return;
//								GameEntityModel entity = WorldUtils.GetEntityFromTeam(hudObj.teamId, hudObj.playerId);
//								if (entity == null) return;
//								int variableValue;
//								if (entity.customVariables.TryGetValue(parameter.SafeString(1), out variableValue)){
//									int minVal = parameter.SafeInt(1);
//									float interpolationValue = (float)(variableValue - minVal) / (float) (parameter.SafeInt(2) - minVal);
//									interpolationValue = Mathf.Clamp(interpolationValue, 0, 1);
//									model.ScheduleVariableUpdate(paramName, interpolationValue , delay, duration);
//								}
//							}
//						),
//						new HUDInteractionDelegationEvent(
//							delegate(HUDViewBehaviour model, GameEntityModel entity){
//								int variableValue;
//								if (entity.customVariables.TryGetValue(parameter.SafeString(1), out variableValue)){
//									int minVal = parameter.SafeInt(1);
//									float interpolationValue = (float)(variableValue - minVal) / (float) (parameter.SafeInt(2) - minVal);
//									interpolationValue = Mathf.Clamp(interpolationValue, 0, 1);
//									model.ScheduleVariableUpdate(paramName, interpolationValue , delay, duration);
//								}
//							}
//						)
//					);
//				default: // custom
//					return new SimpleEvent<HUDViewBehaviour>(null,
//						delegate(HUDViewBehaviour model){
//							model.ScheduleVariableUpdate(paramName, (float) parameter.SafeFloat(0), delay, duration);
//						}
//					);
//			}
//		}
//
//
//		// 2: texture(axel)
//		private static GenericEvent<HUDViewBehaviour> BuildSetTexture(Storage.GenericParameter parameter){
//			int type = parameter.SafeInt(0);
//			switch (type) {
//				case 0: // portrait
//					return new HUDEventDelegationChecker(
//						new SimpleEvent<HUDViewBehaviour>(null,
//							delegate(HUDViewBehaviour model){
//								Storage.HUDObject hudObj = model.hudObjectData;
//								if (hudObj == null) return;
//								GameEntityModel entity = WorldUtils.GetEntityFromTeam(hudObj.teamId, hudObj.playerId);
//								if (entity == null) return;
//								Sprite sprite = CharacterLoader.GetCharacterPortrait(entity.Index);
//								UnityEngine.UI.Image image = model.gameObject.GetComponent<UnityEngine.UI.Image>();
//								if (image != null){
//									image.sprite = sprite;
//								}else {
//									SpriteRenderer renderer = model.gameObject.GetComponent<SpriteRenderer>();
//									if (renderer != null) {
//										renderer.sprite = sprite;
//									}
//								}
//							}
//						),
//						new HUDInteractionDelegationEvent(
//							delegate(HUDViewBehaviour model, GameEntityModel entity){
//								Sprite sprite = CharacterLoader.GetCharacterPortrait(entity.Index);
//								UnityEngine.UI.Image image = model.gameObject.GetComponent<UnityEngine.UI.Image>();
//								if (image != null){
//									image.sprite = sprite;
//								}else {
//									SpriteRenderer renderer = model.gameObject.GetComponent<SpriteRenderer>();
//									if (renderer != null) {
//										renderer.sprite = sprite;
//									}
//								}
//							}
//						)
//					);
//				default: // other
//					return new SimpleEvent<HUDViewBehaviour>(null,
//						delegate(HUDViewBehaviour model) {
//							SpriteRenderer renderer = model.gameObject.GetComponent<SpriteRenderer>();
//							if (renderer == null) return;
//							renderer.sprite = Resources.Load<Sprite>(parameter.SafeString(0));
//						}
//					);
//			}
//		}
//
//
//		// 3: label(name)
//		private static GenericEvent<HUDViewBehaviour> BuildSetText(Storage.GenericParameter parameter){
//			int type = parameter.SafeInt(0);
//			switch (type) {
//				case 0: // character name
//					return new HUDEventDelegationChecker(
//						new SimpleEvent<HUDViewBehaviour>(null,
//							delegate(HUDViewBehaviour model){
//								UnityEngine.UI.Text text = model.gameObject.GetComponent<UnityEngine.UI.Text>();
//								if (text == null) return;
//								Storage.HUDObject hudObj = model.hudObjectData;
//								if (hudObj == null) return;
//								GameEntityModel entity = WorldUtils.GetEntityFromTeam(hudObj.teamId, hudObj.playerId);
//								if (entity == null) return;
//								AnimationModel animModel = StateManager.state.GetModel(entity.animationModelId) as AnimationModel;
//								if (animModel == null) return;
//								text.text =  animModel.characterName;
//							}
//						),
//						new HUDInteractionDelegationEvent(
//							delegate(HUDViewBehaviour model, GameEntityModel entity){
//								UnityEngine.UI.Text text = model.gameObject.GetComponent<UnityEngine.UI.Text>();
//								if (text == null) return;
//								AnimationModel animModel = StateManager.state.GetModel(entity.animationModelId) as AnimationModel;
//								if (animModel == null) return;
//								text.text =  animModel.characterName;
//							}
//						)
//					);
//				case 1:
//					// variable
//					return new HUDEventDelegationChecker(
//						new SimpleEvent<HUDViewBehaviour>(null,
//							delegate(HUDViewBehaviour model){
//								Storage.HUDObject hudObj = model.hudObjectData;
//								if (hudObj == null) return;
//								GameEntityModel entity = WorldUtils.GetEntityFromTeam(hudObj.teamId, hudObj.playerId);
//								if (entity == null) return;
//								UnityEngine.UI.Text text = model.gameObject.GetComponent<UnityEngine.UI.Text>();
//								if (text == null) return;
//								int value;
//								entity.customVariables.TryGetValue(parameter.SafeString(0), out value);
//								text.text = value + "";
//							}
//						),
//						new HUDInteractionDelegationEvent(
//							delegate(HUDViewBehaviour model, GameEntityModel entity){
//								UnityEngine.UI.Text text = model.gameObject.GetComponent<UnityEngine.UI.Text>();
//								if (text == null) return;
//								int value;
//								entity.customVariables.TryGetValue(parameter.SafeString(0), out value);
//								text.text = value + "";
//							}
//						)
//					);
//				default: // custom
//					return new SimpleEvent<HUDViewBehaviour>(null,
//						delegate(HUDViewBehaviour model) {
//							UnityEngine.UI.Text text = model.gameObject.GetComponent<UnityEngine.UI.Text>();
//							if (text == null) return;
//							text.text = parameter.SafeString(1);
//						}
//					);
//			}
//		}
//
//
//		// 4: spawnFX(sparks)
//		private static GenericEvent<HUDViewBehaviour> BuildSpawnEffect(Storage.GenericParameter parameter){
//			FixedVector3 offset = BuildFixedVector3(parameter);
//			string prefabName = parameter.SafeString(0);
//			int locationType = parameter.SafeInt(0);
//			int lifetime = parameter.SafeInt(1);
//			bool localSpace = parameter.SafeBool(0);
//
//			// {"self", "anchor", "hit intersection", "hurt intersection"}
//			PentaEntityAnimationEvent<string, int, FixedVector3, bool, GameEntityView.ConvertGameToViewCoordinates>.EventExecutionDelegate theDelegate = null;
//			switch (locationType) {
//				case 0:
//					// HUD element
//					return new SimpleEvent<HUDViewBehaviour>(null,
//						delegate(HUDViewBehaviour model) {
//							// TODO: spawn at HUD
//							RetroBread.Debug.LogError("Spawn effect at HUD not supported yet");
//						}
//					);
//				case 1:
//					// character
//					theDelegate = GameEntityView.SpawnAtSelf;
//					break;
//				case 2:
//					// Anchor, TODO: which anchor?
//					RetroBread.Debug.LogError("Spawn at anchor not supported yet");
//					break;
//				case 3: 
//					// hit
//					theDelegate = GameEntityView.SpawnAtHitIntersection;
//					break;
//				case 4:
//					// hurt
//					theDelegate = GameEntityView.SpawnAtHurtIntersection;
//					break;
//			}
//
//			return new HUDEventDelegationChecker(
//				new SimpleEvent<HUDViewBehaviour>(null,
//					delegate(HUDViewBehaviour model){
//						Storage.HUDObject hudObj = model.hudObjectData;
//						if (hudObj == null) return;
//						GameEntityModel entity = WorldUtils.GetEntityFromTeam(hudObj.teamId, hudObj.playerId);
//						if (entity == null) return;
//						theDelegate(entity, prefabName, lifetime, offset, localSpace, PhysicPoint2DView.ConvertGameToViewCoordinates);
//					}
//				),
//				new HUDInteractionDelegationEvent(
//					delegate(HUDViewBehaviour model, GameEntityModel entity){
//						theDelegate(entity, prefabName, lifetime, offset, localSpace, PhysicPoint2DView.ConvertGameToViewCoordinates);
//					}
//				)
//			);
//		}
//
//#endregion
//
//	}
//
//}
