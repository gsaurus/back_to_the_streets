using ProtoBuf;
using System;

namespace RetroBread{

	[Serializable]
	[ProtoContract]
	[ProtoInclude(15041936, typeof(SimpleRandomGenerator))]
	public abstract class RandomGenerator{

		// Random unsigned int between [0, uint.MAX]
		public abstract uint NextUnsignedInt();

		// Random integer between [min, max]
		public int NextInt(int min, int max){
			return (int) (NextUnsignedInt()%(max-min+1) + min);
		}

		// Random Fixed Float between [0, 1]
		public FixedFloat NextFloat(){
			// The magic number below is 1/(2^32 + 2).
			// The result is strictly between 0 and 1.
			return FixedFloat.Create((NextUnsignedInt() + 1.0) * 2.328306435454494e-10);
		}

		public FixedFloat NextFloat(FixedFloat min, FixedFloat max){
			return min + NextFloat() * (max-min);
		}

	}

}