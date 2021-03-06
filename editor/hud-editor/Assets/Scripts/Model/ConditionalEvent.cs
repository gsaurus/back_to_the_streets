﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{


	public class ConditionalEventComparer : IEqualityComparer<ConditionalEvent>{
		public bool Equals (ConditionalEvent e1, ConditionalEvent e2){ 
			if (e1.conditions.Count != e2.conditions.Count || e1.events.Count != e2.events.Count) {
				return false;
			}
			for (int i = 0; i < e1.conditions.Count; ++i) {
				if (!e1.conditions[i].IsEqual(e2.conditions[i])){
					return false;
				}
			}
			for (int i = 0; i < e1.events.Count; ++i) {
				if (!e1.events[i].IsEqual(e2.events[i])){
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(ConditionalEvent e){
			int hashCode = 4397;
			foreach (GenericParameter param in e.conditions) {
				hashCode = hashCode * 307 + param.DeepHashCode();
			}
			foreach (GenericParameter param in e.events) {
				hashCode = hashCode * 17 + param.DeepHashCode();
			}
			return hashCode;
		}
	}



	public class ConditionalEvent {

		public List<GenericParameter> conditions;
		public List<GenericParameter> events;

		// This is a temporary builder variable
		// it's populated during save, and discarded at save end 
		private Storage.GenericEvent storageEvent;



		public ConditionalEvent(){
			conditions = new List<GenericParameter>();
			events = new List<GenericParameter>();
			storageEvent = null;
		}


		public ConditionalEvent Clone(){
			ConditionalEvent clone = new ConditionalEvent();
			clone.conditions = new List<GenericParameter>(conditions.Count);
			clone.events = new List<GenericParameter>(events.Count);
			foreach (GenericParameter param in conditions) {
				clone.conditions.Add(param.Clone());
			}
			foreach (GenericParameter param in events) {
				clone.events.Add(param.Clone());
			}
			return clone;
		}



		public static ConditionalEvent LoadFromStorage(Storage.GenericEvent storageEvent, Storage.HUD storageHud){

			ConditionalEvent newEvent = new ConditionalEvent();

			// Populate conditions
			newEvent.conditions = new List<GenericParameter>(storageEvent.conditionIds.Length);
			foreach (int conditionId in storageEvent.conditionIds){
				newEvent.conditions.Add(GenericParameter.LoadFromStorage(storageHud.genericParameters[conditionId]));
			}

			// Populate events
			newEvent.events = new List<GenericParameter>(storageEvent.eventIds.Length);
			foreach (int eventId in storageEvent.eventIds){
				newEvent.events.Add(GenericParameter.LoadFromStorage(storageHud.genericParameters[eventId]));
			}

			return newEvent;
		}


		public Storage.GenericEvent SaveToStorage(){
			Storage.GenericEvent ret = storageEvent;
			storageEvent = null;
			return ret;
		}


		public void BuildStorage(List<GenericParameter> genericParams){
			storageEvent = new Storage.GenericEvent();
			storageEvent.conditionIds = new int[conditions.Count];
			storageEvent.eventIds = new int[events.Count];
			GenericParameter searchParam;
			int paramIndex;
			for (int i = 0 ; i < conditions.Count ; ++i){
				searchParam = conditions[i];
				paramIndex = genericParams.FindIndex(x => x.IsEqual(searchParam));
				if (paramIndex < 0){
					paramIndex = genericParams.Count;
					genericParams.Add(searchParam);
				}
				storageEvent.conditionIds[i] = paramIndex;
			}
			for (int i = 0 ; i < events.Count ; ++i){
				searchParam = events[i];
				paramIndex = genericParams.FindIndex(x => x.IsEqual(searchParam));
				if (paramIndex < 0){
					paramIndex = genericParams.Count;
					genericParams.Add(searchParam);
				}
				storageEvent.eventIds[i] = paramIndex;
			}
		}

		public override string ToString(){
			string finalString = "";
			if (conditions.Count > 0) {
				for (int i = 0; i < conditions.Count - 1; ++i) {
					finalString += ConditionParameterBuilder.Instance.ToString(conditions[i]) + " && ";
				}
				finalString += ConditionParameterBuilder.Instance.ToString(conditions[conditions.Count - 1]);
			}
			finalString += " --> ";
			if (events.Count > 0) {
				for (int i = 0; i < events.Count - 1; ++i) {
					finalString += EventParameterBuilder.Instance.ToString(events[i]) + " && ";
				}
				finalString += EventParameterBuilder.Instance.ToString(events[events.Count - 1]);
			} else {
				finalString += "<no action>";
			}
			return finalString;
		}

			
	}


}
