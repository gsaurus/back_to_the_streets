using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RetroBread{

	public abstract class Serializer{
		public static Serializer defaultSerializer = new ProtobufSerializer();
		public static Serializer DefaultSerializer {
			get{
				return defaultSerializer;
			}
		}


		public byte[] Serialize(Object obj){
			using (MemoryStream stream = new MemoryStream()){
				Serialize(stream, obj);
				return stream.ToArray();
			}
		}
	
		public T Deserialize<T>(byte[] data){
			using (MemoryStream stream = new MemoryStream(data)){
				return Deserialize<T>(stream);
			}
		}

		public abstract void Serialize(Stream stream, object obj);
		public abstract T Deserialize<T>(Stream stream);

	}


	// Serializer provides a simple way to serialize / deserialize data
	public class ProtobufSerializer: Serializer
	{

		// The protobuffer serializer
		private RbSerializer rbSerializer;

		public ProtobufSerializer() {
			rbSerializer = new RbSerializer();
		}

		public override void Serialize(Stream stream, object obj){
			rbSerializer.Serialize(stream, obj);
		}

		public override T Deserialize<T>(Stream stream){
			return (T) rbSerializer.Deserialize(stream, null, typeof(T));
		}

	}
	

}

