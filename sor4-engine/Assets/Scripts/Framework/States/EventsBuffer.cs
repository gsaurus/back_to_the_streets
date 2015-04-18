using System;
using System.IO;
using System.Collections.Generic;


namespace RetroBread{


	// TODO: improve this later: we can probably do this with arrays, etc

	// Keep track of latest events
	public class EventsBuffer{

		// All events grouped by player. For each player, events are sorted by keyframe
		private Dictionary<uint, SortedList<uint, List<Event>>> allEvents;

		// Keep track of what was the oldest event added since last time it was checked
		// Usefull if we need to rewind the game state to process old events
		private uint oldestEventKeyframeSinceLastCheck;

		// Serializer to encode events to a stream
		private Serializer serializer = Serializer.defaultSerializer;
		public Serializer Serializer {
			private get{ return serializer; }
			set{ serializer = value; }
		}

		public EventsBuffer (){
			Clear();
		}

		// Get the events that happens in a certain keyframe for a certain player
		public List<Event> GetEvents(uint keyframe, uint player) {
			SortedList<uint, List<Event>> playerEvents;
			if (!allEvents.TryGetValue(player, out playerEvents))
				return null;
			List<Event> ret;
			playerEvents.TryGetValue(keyframe, out ret);
			return ret;
		}

		// Add or set a new event and update the oldest event keyframe if necessary
		public void AddEvent(Event newEvent){
			SortedList<uint, List<Event>> playerEvents;
			if (!allEvents.TryGetValue(newEvent.PlayerId, out playerEvents)){
				// first time adding an event for this player
				playerEvents = new SortedList<uint, List<Event>>();
				allEvents[newEvent.PlayerId] = playerEvents;
			}
			List<Event> playerEventsForFrame;
			if (!playerEvents.TryGetValue(newEvent.Keyframe, out playerEventsForFrame)){
				// first time adding an event for this keyframe
				playerEventsForFrame = new List<Event>();
				playerEvents[newEvent.Keyframe] = playerEventsForFrame;
			}
			playerEventsForFrame.Add(newEvent);

			// Keep track of the oldest event added
			if (newEvent.Keyframe < oldestEventKeyframeSinceLastCheck) {
				oldestEventKeyframeSinceLastCheck = newEvent.Keyframe;
			}
		}


		// Get the keyframe of the oldest event added since the last call to this method 
		public uint GetOldestEventKeyframeSinceLastCheck(){
			uint res = oldestEventKeyframeSinceLastCheck;
			oldestEventKeyframeSinceLastCheck = uint.MaxValue;
			return res;
		}


		// Remove old events till the provided keyframe
		// Optionally it writes all removed events to an output stream (such as a file stream)
		public void Flush(uint lastKeyframeToKeep, Stream stream = null) {

			// The strategy here is to recreate the whole structure with only the remaining elements,
			// Since it's cheaper than removing elements one by one.
			// Unfortunately C# collections does not provide useful methods to do this in a more efficient way

			List<List<Event>> removedEvents = null;
			if (stream != null) {
				removedEvents = new List<List<Event>>();
			}

			// Iterate events of each player
			Dictionary<uint, SortedList<uint, List<Event>>> newAllEvents = new Dictionary<uint, SortedList<uint, List<Event>>>(allEvents.Count);
			foreach (KeyValuePair<uint, SortedList<uint, List<Event>>> pair in allEvents) {
				SortedList<uint, List<Event>> eventsList = pair.Value;
				SortedList<uint, List<Event>> newEventsList = new SortedList<uint, List<Event>>(eventsList.Count);
				int i;
				// Iterate player events to remove
				for (i = 0 ; i < eventsList.Count && eventsList.Keys[i] < lastKeyframeToKeep ; ++i) {
					if (removedEvents != null) {
						removedEvents.Add(eventsList.Values[i]);
					}
				}
				// Add all remaining to a new list
				while (i < eventsList.Count){
					newEventsList.Add(eventsList.Keys[i], eventsList.Values[i]);
					++i;
				}
				// finally add the new list to the new allEvents
				if (newEventsList.Count > 0) {
					newAllEvents[pair.Key] = newEventsList;
				}
			}

			// TODO
	//		// In the end we even sort the output list for convenience, and send it to the stream
	//		// Note: we could try to iterate the structure already in keyframe order,
	//		// but we end up doing unhecessary loops, it probably doesn't compensate
	//		if (removedEvents != null){
	//			// sort
	//			removedEvents.Sort(new EventComparerByKeyframe());
	//			// serialize all into stream
	//
	//			foreach(Event outEvent in removedEvents) {
	//				serializer.Serialize(outEvent, stream);
	//			}
	//		}
		}


		// Get rid of all events that happened in newer keyframes
		// Useful when restoring an old state
		public void DiscardNewerEvents(uint newestKeyframeToKeep) {
			// Iterate events of each player
			Dictionary<uint, SortedList<uint, List<Event>>> newAllEvents = new Dictionary<uint, SortedList<uint, List<Event>>>(allEvents.Count);
			foreach (KeyValuePair<uint, SortedList<uint, List<Event>>> pair in allEvents) {
				SortedList<uint, List<Event>> eventsList = pair.Value;
				SortedList<uint, List<Event>> newEventsList = new SortedList<uint, List<Event>>(eventsList.Count);

				foreach(KeyValuePair<uint, List<Event>> listPair in eventsList){
					if (listPair.Key >= newestKeyframeToKeep) break;
					newEventsList.Add(listPair.Key, listPair.Value);
				}
			
				// Add the new list to the new allEvents
				if (newEventsList.Count > 0) {
					newAllEvents[pair.Key] = newEventsList;
				}
			}
			allEvents = newAllEvents;
		}




		// Clear the buffer
		public void Clear(){
			allEvents = new Dictionary<uint, SortedList<uint, List<Event>>>();
			oldestEventKeyframeSinceLastCheck = uint.MaxValue;
		}

	}


}
