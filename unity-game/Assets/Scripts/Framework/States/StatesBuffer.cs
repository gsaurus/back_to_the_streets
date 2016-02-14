using System;
using System.Collections.Generic;


namespace RetroBread{


	// TODO: improve this later: we can probably do this with double linked list or circular array, etc
	// Then we don't need to flush it manually, it can discard old states when new states are added,
	// based on the buffer's limit

	// This buffer should keep states history every few frames
	// So that it can be used to retrieve an older state for state reconstruction
	public interface StatesBuffer{
		// Find the latest state closer to keyframe and return it
		State GetLatestState(uint keyframe);
		
		// Return the oldest state in the buffer
		State GetOldestState();
		
		// Deserializes a buffered state
		State GetState(uint keyframe);
		
		
		// Buffers a serialization of the state
		void SetState(State state);
		
		// Get rid of all states that happened in older keyframes
		void DiscardOlderStates(uint oldestKeyframeToKeep);
		
		// Get rid of all states that happened in newer keyframes
		// Useful when restoring an older state
		void DiscardNewerStates(uint newestKeyframeToKeep);
		
		
		// Clear the buffer
		void Clear();
	}

}

