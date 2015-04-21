using System;
using System.Collections.Generic;


namespace RetroBread{


	// TODO: improve this later: we can probably do this with double linked list or circular array, etc
	// Then we don't need to flush it manually, it can discard old states when new states are added,
	// based on the buffer's limit

	// This buffer should keep states history every few frames
	// So that it can be used to retrieve an older state for state reconstruction
	public class StatesBuffer{

		// We store them serialized since it's the fastest way of deep-copying states
		// We may implement it differently in the future
		private Dictionary<uint, byte[]> bufferedStates;

		// Serializer to encode/decode states into/from history buffer
		private Serializer serializer = Serializer.defaultSerializer;
		public Serializer Serializer {
			private get{ return serializer; }
			set{ serializer = value; }
		}

		public StatesBuffer(){
			Clear();
		}



		// Find the latest state closer to keyframe and return it
		public State GetLatestState(uint keyframe){

			if (bufferedStates.Count == 0) return null;

			byte[] data;

			if (!bufferedStates.TryGetValue(keyframe, out data)){
				List<uint> keys = new List<uint>(bufferedStates.Keys);
				keys.Sort();
				if (keyframe < keys[0]){
					// keyframe is older than anything in the buffer!
					return null;
				}
				int closestIndex = keys.BinarySearch(keyframe);
				if (closestIndex < 0){
					closestIndex = ~closestIndex-1; // this is the closest we found in the keys list
				}
				data = bufferedStates[keys[closestIndex]];
			}

			return serializer.Deserialize<InternalState>(data);
				
		}

		// Return the oldest state in the buffer
		public State GetOldestState(){
			if (bufferedStates.Count == 0) return null;
			byte[] data;
			List<uint> keys = new List<uint>(bufferedStates.Keys);
			keys.Sort();
			data = bufferedStates[keys[0]];
			return serializer.Deserialize<InternalState>(data);
		}


		// Deserializes a buffered state
		public State GetState(uint keyframe){
			byte[] data;
			if (!bufferedStates.TryGetValue(keyframe, out data)){
				return null;
			}
			return serializer.Deserialize<InternalState>(data);
		}
		
		
		// Buffers a serialization of the state
		public void SetState(State state) {
			bufferedStates[state.Keyframe] = serializer.Serialize(state);
		}

		// Get rid of all states that happened in older keyframes
		public void DiscardOlderStates(uint oldestKeyframeToKeep) {
			Dictionary<uint, byte[]> finalStates = new Dictionary<uint, byte[]>(bufferedStates.Count);
			foreach (KeyValuePair<uint, byte[]> entry in bufferedStates) {
				if (entry.Key >= oldestKeyframeToKeep) {
					finalStates.Add(entry.Key, entry.Value);
				}
			}
			bufferedStates = finalStates;
		}

		// Get rid of all states that happened in newer keyframes
		// Useful when restoring an older state
		public void DiscardNewerStates(uint newestKeyframeToKeep) {
			Dictionary<uint, byte[]> finalStates = new Dictionary<uint, byte[]>(bufferedStates.Count);
			foreach (KeyValuePair<uint, byte[]> entry in bufferedStates) {
				if (entry.Key <= newestKeyframeToKeep) {
					finalStates.Add(entry.Key, entry.Value);
				}
			}
			bufferedStates = finalStates;
		}


		// Clear the buffer
		public void Clear() {
			bufferedStates = new Dictionary<uint, byte[]>();
		}


	}


}

