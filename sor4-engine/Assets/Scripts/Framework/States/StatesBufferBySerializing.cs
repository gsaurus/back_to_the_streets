//using System;
//using System.Collections.Generic;
//
//namespace RetroBread{
//
//	public class StatesBufferBySerializing{
//		
//		// history of states, by keyframe
//		private Dictionary<uint, byte[]> bufferedStates;
//		
//		public StatesBufferBySerializing(){
//			Clear();
//		}
//		
//		
//		// Find the latest state closer to keyframe and return it
//		public State GetLatestState(uint keyframe){
//			
//			if (bufferedStates.Count == 0) return null;
//			
//			State state = null;
//			
//			if (!bufferedStates.TryGetValue(keyframe, out state)){
//				List<uint> keys = new List<uint>(bufferedStates.Keys);
//				keys.Sort();
//				if (keyframe < keys[0]){
//					// keyframe is older than anything in the buffer!
//					return null;
//				}
//				int closestIndex = keys.BinarySearch(keyframe);
//				if (closestIndex < 0){
//					closestIndex = ~closestIndex-1; // this is the closest we found in the keys list
//				}
//				state = bufferedStates[keys[closestIndex]];
//			}
//			
//			return state;
//			
//		}
//		
//		// Return the oldest state in the buffer
//		public State GetOldestState(){
//			if (bufferedStates.Count == 0) return null;
//			List<uint> keys = new List<uint>(bufferedStates.Keys);
//			keys.Sort();
//			return bufferedStates[keys[0]];
//		}
//		
//		
//		// Deserializes a buffered state
//		public State GetState(uint keyframe){
//			State state = null;
//			bufferedStates.TryGetValue(keyframe, out state);
//			return state;
//		}
//		
//		
//		// Buffers a serialization of the state
//		public void SetState(State state) {
//			bufferedStates[state.Keyframe] = state.Clone();
//		}
//		
//		// Get rid of all states that happened in older keyframes
//		public void DiscardOlderStates(uint oldestKeyframeToKeep) {
//			Dictionary<uint, State> finalStates = new Dictionary<uint, State>(bufferedStates.Count);
//			foreach (KeyValuePair<uint, State> entry in bufferedStates) {
//				if (entry.Key >= oldestKeyframeToKeep) {
//					finalStates.Add(entry.Key, entry.Value);
//				}
//			}
//			bufferedStates = finalStates;
//		}
//		
//		// Get rid of all states that happened in newer keyframes
//		// Useful when restoring an older state
//		public void DiscardNewerStates(uint newestKeyframeToKeep) {
//			Dictionary<uint, State> finalStates = new Dictionary<uint, State>(bufferedStates.Count);
//			foreach (KeyValuePair<uint, State> entry in bufferedStates) {
//				if (entry.Key <= newestKeyframeToKeep) {
//					finalStates.Add(entry.Key, entry.Value);
//				}
//			}
//			bufferedStates = finalStates;
//		}
//		
//		
//		// Clear the buffer
//		public void Clear() {
//			bufferedStates = new Dictionary<uint, State>();
//		}
//		
//		
//	}
//
//}
//
