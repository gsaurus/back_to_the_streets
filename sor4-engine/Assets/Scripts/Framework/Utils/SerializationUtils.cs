using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RetroBread{

	// Serializer provides a simple way to serialize / deserialize data
	public class Serializer
	{
		public static Serializer defaultSerializer = new Serializer();
		public static Serializer DefaultSerializer {
			get{
				return defaultSerializer;
			}
		}


		// The data formatter
		private IFormatter formatter;

		public Serializer(IFormatter formatter) {
			this.formatter = formatter;
		}

		// Serializer with default formatter
		public Serializer(){
			formatter = new BinaryFormatter();
		}
		
		// Use a different formatter
		public void SetFormatter(IFormatter newFormatter) {
			formatter = newFormatter;
		}
		
		// Encode an object into a byte array
		public byte[] Serialize(Object obj){
			using (MemoryStream stream = new MemoryStream()){
				formatter.Serialize(stream, obj);
				return stream.ToArray();
			}
		}
		
		// Decode an object from a byte array
		public object Deserialize(byte[] data){
			using (MemoryStream stream = new MemoryStream(data)){
				return formatter.Deserialize(stream);
			}
		}
		
		// Write an object to a stream
		public void Serialize(Object obj, Stream stream) {
			formatter.Serialize(stream, obj);
		}
		
		// Read an object from a stream
		public object Deserialize(Stream stream) {
			return formatter.Deserialize(stream);
		}


	/* // This is here just for reference
	#region Workarounds to the security errors on Webplayer when serializing/deserializing certain containers

		public byte[] SerializeDictionary<K,V>(Dictionary<K,V> dict){
			using (MemoryStream stream = new MemoryStream()){

				formatter.Serialize(stream, dict.Count);
				foreach(KeyValuePair<K,V> pair in dict){
					formatter.Serialize(stream, pair.Key);
					formatter.Serialize(stream, pair.Value);
				}
				return stream.ToArray();
			}

			// This doesn't work, it's here for reference
	//		KeyValuePair<K,V>[] keyValues = new KeyValuePair<K,V>[dict.Count];
	//		int index = 0;
	//		foreach(KeyValuePair<K,V> pair in dict) {
	//			keyValues[index++] = pair;
	//		}
	//		return Serialize(keyValues);
		}

		public Dictionary<K,V> DeserializeDictionary<K,V>(byte[] data){
			using (MemoryStream stream = new MemoryStream(data)){
				int numElements = (int) formatter.Deserialize(stream);
				UnityEngine.Debug.Log("DESERIALIZE num elements: " + numElements);
				Dictionary<K,V> dict = new Dictionary<K,V>(numElements);
				K key; V value;
				for (int i = 0 ; i < numElements ; ++i){
					key = (K) formatter.Deserialize(stream);
					value = (V) formatter.Deserialize(stream);
					UnityEngine.Debug.Log("key: " + key + ", value: " + value);
					dict[key] = value;

				}
				return dict;
			}

			// This doesn't work, it's here for reference
	//		KeyValuePair<K,V>[] keyValues = Deserialize(data) as KeyValuePair<K,V>[];
	//		Dictionary<K,V> dict = new Dictionary<K,V>(keyValues.Length);
	//		foreach(KeyValuePair<K,V> pair in keyValues){
	//			dict[pair.Key] = pair.Value;
	//		}
	//		return dict;
		}

	#endregion
		*/

	}




	// Serializable Dictionary
	// only necessary because BinaryFormatter doesn't work well with dictionaries for the webplayer
	[Serializable]
	public class SerializableDictionary<K,V>: Dictionary<K,V>, ISerializable{
		
		public override void GetObjectData(SerializationInfo info, StreamingContext context){
			info.AddValue("count", Count, typeof(Int32));
			int index = 0;
			foreach(KeyValuePair<K,V> pair in this){
				info.AddValue("k" + index, pair.Key, typeof(K));
				info.AddValue("v" + index, pair.Value, typeof(V));
				++index;
			}
		}
		
		public SerializableDictionary(SerializationInfo info, StreamingContext context):base(){
			int count = (int)info.GetValue("count", typeof(Int32));
			K key; V value;
			for (int i = 0 ; i < count ; ++i){
				key = (K)info.GetValue("k" + i, typeof(K));
				value = (V)info.GetValue("v" + i, typeof(V));
				this[key] = value;
			}
		}
		
		public SerializableDictionary():base(){}
		public SerializableDictionary(int capacity):base(capacity){}
		public SerializableDictionary(IDictionary<K,V> dictionary):base(dictionary){}
		public SerializableDictionary(IEqualityComparer<K> comparer):base(comparer){}
		public SerializableDictionary(int capacity, IEqualityComparer<K> comparer):base(capacity, comparer){}
		public SerializableDictionary(IDictionary<K,V> dictionary, IEqualityComparer<K> comparer):base(dictionary, comparer){}
		
	}



	// Serializable List
	// only necessary because BinaryFormatter doesn't work well with lists for the webplayer
	[Serializable]
	public class SerializableList<T>: List<T>, ISerializable{
		
		public void GetObjectData(SerializationInfo info, StreamingContext context){
			info.AddValue("count", Count, typeof(Int32));
			int index = 0;
			foreach(T value in this){
				info.AddValue("v" + index, value, typeof(T));
				++index;
			}
		}
		
		public SerializableList(SerializationInfo info, StreamingContext context):base(){
			int count = (int)info.GetValue("count", typeof(Int32));
			T value;
			for (int i = 0 ; i < count ; ++i){
				value = (T)info.GetValue("v" + i, typeof(T));
				this.Add(value);
			}
		}
		
		public SerializableList():base(){}
		public SerializableList(int capacity):base(capacity){}
		public SerializableList(IEnumerable<T> collection):base(collection){}
		
	}


	// TODO: add more SerializableClasses on need

}

