using ProtoBuf;
using System;

namespace RetroBread{

/// <summary>
/// Based on SimpleRNG by John D. Cook - http://www.johndcook.com
/// SimpleRNG is a simple random number generator based on 
/// George Marsaglia's MWC (multiply with carry) generator.
/// Although it is very simple, it passes Marsaglia's DIEHARD
/// series of random number generator tests.
/// 
/// Original document: http://www.codeproject.com/Articles/25172/Simple-Random-Number-Generation 
/// <summary>
[Serializable]
[ProtoContract]
public class SimpleRandomGenerator: RandomGenerator
{
	[ProtoMember(1)]
	private uint m_z;
	[ProtoMember(2)]
	private uint m_w;

	public SimpleRandomGenerator(){
		// Default seed
		m_w = 521288629;
		m_z = 362436069;
	}


	public SimpleRandomGenerator(long seed){
		SetSeed(seed);
	}

	public void SetSeed(long seed){
		SetSeed((uint)(seed >> 16), (uint)(seed % 4294967296));
	}

	public void SetSeed(uint u, uint v){
		if (u != 0) m_w = u; 
		if (v != 0) m_z = v;
	}

	// This is the heart of the generator.
	// It uses George Marsaglia's MWC algorithm to produce an unsigned integer.
	// See http://www.bobwheeler.com/statistics/Password/MarsagliaPost.txt
	public override uint NextUnsignedInt(){
		m_z = 36969 * (m_z & 65535) + (m_z >> 16);
		m_w = 18000 * (m_w & 65535) + (m_w >> 16);
		return (m_z << 16) + m_w;
	}
}

}


