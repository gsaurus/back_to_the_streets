
using System;


// Adapted from http://stackoverflow.com/questions/605124/fixed-point-math-in-c
// Also based on https://code.google.com/p/kutil/source/browse/trunk/src/org/kalmeo/util/MathFP.java?r=42
[Serializable]
public struct FixedFloat: IComparable<FixedFloat>
{
	public long RawValue;
	public static int SHIFT_AMOUNT = 14; //12 is 4096, 14 is 16384, 16 is 65536
	public static int PRECISION_LIMIT = (int)Math.Pow(2,SHIFT_AMOUNT);
	
	public static long OneL = 1 << SHIFT_AMOUNT;
	public static int OneI = 1 << SHIFT_AMOUNT;
	public static FixedFloat One = FixedFloat.Create( 1, true );
	public static FixedFloat Zero = FixedFloat.Create( 0, true );
	#region PI, HalfPI, DoublePI
	public static FixedFloat PI = FixedFloat.Create(3.141592653589793);
	public static FixedFloat HalfPI = PI * 0.5;
	public static FixedFloat TwoPI = PI * 2;
	public static FixedFloat DegreesToRadiansConversionRatio = PI / 180;
	public static FixedFloat RadiansToDegreesConversionRatio = 180 / PI;
	#endregion
	
	#region Constructors
	public static FixedFloat Create( long StartingRawValue, bool UseMultiple )
	{
		FixedFloat theFloat;
		theFloat.RawValue = StartingRawValue;
		if ( UseMultiple )
			theFloat.RawValue = theFloat.RawValue << SHIFT_AMOUNT;
		return theFloat;
	}
	public static FixedFloat Create(float floatValue)
	{
		FixedFloat theFloat;
		floatValue *= (float)OneL;
		theFloat.RawValue = (long)Math.Round(floatValue);
		return theFloat;
	}
	public static FixedFloat Create(double DoubleValue)
	{
		FixedFloat theFloat;
		DoubleValue *= (double)OneL;
		theFloat.RawValue = (long)Math.Round(DoubleValue );
		return theFloat;
	}
	#endregion
	
	public int IntValue
	{
		get { return (int)( this.RawValue >> SHIFT_AMOUNT ); }
	}
	
	public int ToInt()
	{
		return (int)( this.RawValue >> SHIFT_AMOUNT );
	}
	
	public double ToDouble()
	{
		return (double)this.RawValue / (double)OneL;
	}

	public float ToFloat()
	{
		return (float)this.RawValue / (float)OneL;
	}
	
	public FixedFloat Inverse
	{
		get { return FixedFloat.Create( -this.RawValue, false ); }
	}
	
//	#region FromParts
//	/// <summary>
//	/// Create a fixed-int number from parts.  For example, to create 1.5 pass in 1 and 500.
//	/// </summary>
//	/// <param name="PreDecimal">The number above the decimal.  For 1.5, this would be 1.</param>
//	/// <param name="PostDecimal">The number below the decimal, to three digits.  
//	/// For 1.5, this would be 500. For 1.005, this would be 5.</param>
//	/// <returns>A fixed-int representation of the number parts</returns>
//	public static FixedFloat FromParts( int PreDecimal, int PostDecimal )
//	{
//		FixedFloat f = FixedFloat.Create( PreDecimal, true );
//		if ( PostDecimal != 0 )
//			f.RawValue += ( FixedFloat.Create( PostDecimal ) / 1000 ).RawValue;
//		
//		return f;
//	}
//	#endregion
	
	#region *
	public static FixedFloat operator *( FixedFloat one, FixedFloat other )
	{
		FixedFloat theFloat;
		theFloat.RawValue = ( one.RawValue * other.RawValue ) >> SHIFT_AMOUNT;
		return theFloat;
	}
	
	public static FixedFloat operator *( FixedFloat one, int multi )
	{
		return one * (FixedFloat)multi;
	}
	
	public static FixedFloat operator *( int multi, FixedFloat one )
	{
		return one * (FixedFloat)multi;
	}
	#endregion
	
	#region /
	public static FixedFloat operator /( FixedFloat one, FixedFloat other )
	{
		FixedFloat theFloat;
		theFloat.RawValue = ( one.RawValue << SHIFT_AMOUNT ) / ( other.RawValue );
		return theFloat;
	}
	
	public static FixedFloat operator /( FixedFloat one, int divisor )
	{
		return one / (FixedFloat)divisor;
	}
	
	public static FixedFloat operator /( int divisor, FixedFloat one )
	{
		return (FixedFloat)divisor / one;
	}
	#endregion
	
	#region %
	public static FixedFloat operator %( FixedFloat one, FixedFloat other )
	{
		FixedFloat theFloat;
		theFloat.RawValue = ( one.RawValue ) % ( other.RawValue );
		return theFloat;
	}
	
	public static FixedFloat operator %( FixedFloat one, int divisor )
	{
		return one % (FixedFloat)divisor;
	}
	
	public static FixedFloat operator %( int divisor, FixedFloat one )
	{
		return (FixedFloat)divisor % one;
	}
	#endregion
	
	#region +
	public static FixedFloat operator +( FixedFloat one, FixedFloat other )
	{
		FixedFloat theFloat;
		theFloat.RawValue = one.RawValue + other.RawValue;
		return theFloat;
	}
	
	public static FixedFloat operator +( FixedFloat one, int other )
	{
		return one + (FixedFloat)other;
	}
	
	public static FixedFloat operator +( int other, FixedFloat one )
	{
		return one + (FixedFloat)other;
	}
	#endregion
	
	#region -
	public static FixedFloat operator -( FixedFloat one, FixedFloat other )
	{
		FixedFloat theFloat;
		theFloat.RawValue = one.RawValue - other.RawValue;
		return theFloat;
	}
	
	public static FixedFloat operator -( FixedFloat one, int other )
	{
		return one - (FixedFloat)other;
	}
	
	public static FixedFloat operator -( int other, FixedFloat one )
	{
		return (FixedFloat)other - one;
	}


	public static FixedFloat operator -(FixedFloat one )
	{
		FixedFloat theFloat;
		theFloat.RawValue = -one.RawValue;
		return theFloat;
	}

	#endregion
	
	#region ==
	public static bool operator ==( FixedFloat one, FixedFloat other )
	{
		return one.RawValue == other.RawValue;
	}
	
	public static bool operator ==( FixedFloat one, int other )
	{
		return one == (FixedFloat)other;
	}
	
	public static bool operator ==( int other, FixedFloat one )
	{
		return (FixedFloat)other == one;
	}
	#endregion
	
	#region !=
	public static bool operator !=( FixedFloat one, FixedFloat other )
	{
		return one.RawValue != other.RawValue;
	}
	
	public static bool operator !=( FixedFloat one, int other )
	{
		return one != (FixedFloat)other;
	}
	
	public static bool operator !=( int other, FixedFloat one )
	{
		return (FixedFloat)other != one;
	}
	#endregion
	
	#region >=
	public static bool operator >=( FixedFloat one, FixedFloat other )
	{
		return one.RawValue >= other.RawValue;
	}
	
	public static bool operator >=( FixedFloat one, int other )
	{
		return one >= (FixedFloat)other;
	}
	
	public static bool operator >=( int other, FixedFloat one )
	{
		return (FixedFloat)other >= one;
	}
	#endregion
	
	#region <=
	public static bool operator <=( FixedFloat one, FixedFloat other )
	{
		return one.RawValue <= other.RawValue;
	}
	
	public static bool operator <=( FixedFloat one, int other )
	{
		return one <= (FixedFloat)other;
	}
	
	public static bool operator <=( int other, FixedFloat one )
	{
		return (FixedFloat)other <= one;
	}
	#endregion
	
	#region >
	public static bool operator >( FixedFloat one, FixedFloat other )
	{
		return one.RawValue > other.RawValue;
	}
	
	public static bool operator >( FixedFloat one, int other )
	{
		return one > (FixedFloat)other;
	}
	
	public static bool operator >( int other, FixedFloat one )
	{
		return (FixedFloat)other > one;
	}
	#endregion
	
	#region <
	public static bool operator <( FixedFloat one, FixedFloat other )
	{
		return one.RawValue < other.RawValue;
	}
	
	public static bool operator <( FixedFloat one, int other )
	{
		return one < (FixedFloat)other;
	}
	
	public static bool operator <( int other, FixedFloat one )
	{
		return (FixedFloat)other < one;
	}
	#endregion


	#region IComparable

	// Implement IComparable CompareTo to provide default sort order.
	public int CompareTo(FixedFloat otherFloat){
		return RawValue.CompareTo(otherFloat);
	}


	#endregion


	#region conversion operators

	public static explicit operator int(FixedFloat src )
	{
		return src.ToInt();
	}

	public static explicit operator float(FixedFloat src )
	{
		return src.ToFloat();
	}

	public static explicit operator double(FixedFloat src )
	{
		return src.ToDouble();
	}
	
	public static implicit operator FixedFloat(int src )
	{
		return FixedFloat.Create(src, true );
	}
	
	public static implicit operator FixedFloat(long src )
	{
		return FixedFloat.Create(src, true );
	}
	
	public static implicit operator FixedFloat(ulong src )
	{
		return FixedFloat.Create((long)src, true );
	}

	public static implicit operator FixedFloat(float src)
	{
		return FixedFloat.Create(src);
	}


	public static implicit operator FixedFloat(double src)
	{
		return FixedFloat.Create(src);
	}

	#endregion
	
	public static FixedFloat operator <<( FixedFloat one, int Amount )
	{
		return FixedFloat.Create( one.RawValue << Amount, false );
	}
	
	public static FixedFloat operator >>( FixedFloat one, int Amount )
	{
		return FixedFloat.Create( one.RawValue >> Amount, false );
	}
	
	public override bool Equals( object obj )
	{
		if ( obj is FixedFloat )
			return ( (FixedFloat)obj ).RawValue == this.RawValue;
		else
			return false;
	}
	
	public override int GetHashCode()
	{
		return RawValue.GetHashCode();
	}
	
	public override string ToString()
	{
		return this.ToFloat().ToString();
	}


	
	#region Sqrt
	public static FixedFloat SqrtNewtonMethod(FixedFloat f, int NumberOfIterations){
		if ( f.RawValue == 0 ) return FixedFloat.Zero;
		// Clamp, it may be < 0 due to precision errors
		if ( f.RawValue < 0 ){
			f.RawValue = 0;
		}

		FixedFloat k = (f >> 1) + FixedFloat.One;
		for ( int i = 0; i < NumberOfIterations; i++ )
			k = ( k + ( f / k ) ) >> 1;

		// Clamp, just in case it goes wild with precision errors
		// TODO: use an epsilon error margin instead
		if ( k.RawValue < 0 ){
			f.RawValue = 0;
		}
		return k;
	}
	
	public static FixedFloat Sqrt( FixedFloat f )
	{
		byte numberOfIterations = 8;
		if (f > 100 )
			numberOfIterations = 12; // SHIFT_AMOUNT?
		else if (f > 1000 )
			numberOfIterations = 16;
		return SqrtNewtonMethod(f, numberOfIterations);
	}

	// Fast inverse square root used in Quake III
//	public unsafe static FixedFloat Sqrt(FixedFloat number){
//		long i;
//		FixedFloat x, y;
//		const FixedFloat f = 1.5F;
//		
//		x = number * 0.5F;
//		y  = number;
//		i  = * ( long * ) &y;
//		i  = 0x5f375a86 - ( i >> 1 );
//		y  = * ( float * ) &i;
//		y  = y * ( f - ( x * y * y ) );
//		y  = y * ( f - ( x * y * y ) );
//		return number * y;
//	}

	#endregion
	
	#region Sin
	public static FixedFloat Sin(FixedFloat angle)
	{
		// normalize input angle
		for ( ; angle < 0; angle += FixedFloat.TwoPI) ;
		if (angle > FixedFloat.TwoPI){
			angle %= FixedFloat.TwoPI;
		}

		// convert to degrees
		angle *= RadiansToDegreesConversionRatio;
		int index = angle.ToInt();	  // index is the integer part of the angle
		FixedFloat t = angle - index; // t is the fractionary part of the angle

		// treat each of the quadrants
		if ( index <= 90 )
			return sin_lookup(index, t );
		if ( index <= 180 )
			return sin_lookup(180 - index, t );
		if ( index <= 270 )
			return sin_lookup(index - 180, t ).Inverse;
		else
			return sin_lookup(360 - index, t ).Inverse;
	}
	
	private static FixedFloat sin_lookup(int i, FixedFloat t)
	{
		if (t > 0){
			// interpolation between table[i] and table[i+1]
			return SIN_TABLE[i] + (SIN_TABLE[i + 1] - SIN_TABLE[i]) * t;
		}else{
			// exact table value
			return SIN_TABLE[i];
		}
	}
	
//	private static int[] SIN_TABLE_12 = {
//		0, 71, 142, 214, 285, 357, 428, 499, 570, 641, 
//		711, 781, 851, 921, 990, 1060, 1128, 1197, 1265, 1333, 
//		1400, 1468, 1534, 1600, 1665, 1730, 1795, 1859, 1922, 1985, 
//		2048, 2109, 2170, 2230, 2290, 2349, 2407, 2464, 2521, 2577, 
//		2632, 2686, 2740, 2793, 2845, 2896, 2946, 2995, 3043, 3091, 
//		3137, 3183, 3227, 3271, 3313, 3355, 3395, 3434, 3473, 3510, 
//		3547, 3582, 3616, 3649, 3681, 3712, 3741, 3770, 3797, 3823, 
//		3849, 3872, 3895, 3917, 3937, 3956, 3974, 3991, 4006, 4020, 
//		4033, 4045, 4056, 4065, 4073, 4080, 4086, 4090, 4093, 4095, 
//		4096
//	};

	private static FixedFloat[] SIN_TABLE = {
		0,				0.017333984375,	0.03466796875,	0.05224609375,	0.069580078125,	0.087158203125,	0.1044921875,	0.121826171875,	0.13916015625,	0.156494140625,	
		0.173583984375,	0.190673828125,	0.207763671875,	0.224853515625,	0.24169921875,	0.2587890625,	0.275390625,	0.292236328125,	0.308837890625,	0.325439453125,	
		0.341796875,	0.3583984375,	0.37451171875,	0.390625,		0.406494140625,	0.42236328125,	0.438232421875,	0.453857421875,	0.46923828125,	0.484619140625,	
		0.5,			0.514892578125,	0.52978515625,	0.54443359375,	0.55908203125,	0.573486328125,	0.587646484375,	0.6015625,		0.615478515625,	0.629150390625,	
		0.642578125,	0.65576171875,	0.6689453125,	0.681884765625,	0.694580078125,	0.70703125,		0.71923828125,	0.731201171875,	0.742919921875,	0.754638671875,	
		0.765869140625,	0.777099609375,	0.787841796875,	0.798583984375,	0.808837890625,	0.819091796875,	0.828857421875,	0.83837890625,	0.847900390625,	0.85693359375,	
		0.865966796875,	0.87451171875,	0.8828125,		0.890869140625,	0.898681640625,	0.90625,		0.913330078125,	0.92041015625,	0.927001953125,	0.933349609375,	
		0.939697265625,	0.9453125,		0.950927734375,	0.956298828125,	0.961181640625,	0.9658203125,	0.97021484375,	0.974365234375,	0.97802734375,	0.9814453125,	
		0.984619140625,	0.987548828125,	0.990234375,	0.992431640625,	0.994384765625,	0.99609375,		0.99755859375,	0.99853515625,	0.999267578125,	0.999755859375, 
		1
	};
	#endregion
	
	private static FixedFloat mul( FixedFloat f1, FixedFloat f2 )
	{
		return f1 * f2;
	}
	
	#region Cos, Tan, Asin, Acons
	public static FixedFloat Cos(FixedFloat angle){
		return Sin(angle + HalfPI);
	}
	
	public static FixedFloat Tan(FixedFloat angle){
		return Sin(angle) / Cos(angle);
	}
	

	public static FixedFloat Asin( FixedFloat value ){
		bool isNegative = value < 0;
		value = Abs(value);

		// clamp value (it may be higher than 1 due to precision errors
		if (value > FixedFloat.One){
			value = FixedFloat.One;
		}
		
		FixedFloat result = mul( mul( mul( mul(0.008648812770843505859375, value ) -
		                                  0.035755634307861328125, value ) +
		                             0.0846664905548095703125, value ) -
		                        0.214124500751495361328125, value ) +
							1.570787847042083740234375;
		result = HalfPI - Sqrt(FixedFloat.One - value)*result;
		
		return isNegative ? result.Inverse : result;
	}

	public static FixedFloat Acos(FixedFloat f) {
		return HalfPI - Asin(f);
	}



	#endregion
	
	#region ATan, ATan2
	public static FixedFloat Atan( FixedFloat f ){
		return Asin(f / Sqrt(FixedFloat.One + f*f) );
	}
	
	public static FixedFloat Atan2( FixedFloat f1, FixedFloat f2 ){
		if ( f2.RawValue == 0 && f1.RawValue == 0 )
			return FixedFloat.Zero;

		FixedFloat result;
		if ( f2 > 0 ){
			return Atan(f1/f2);
		}else if ( f2 < 0 ){
			result = Atan(Abs(f1/f2));
		}else{
			result = HalfPI;
		}

		return f1 >= 0 ? result : result.Inverse;

	}
	#endregion


	#region Abs, Min, Max

	public static FixedFloat Abs(FixedFloat f){
		return f < 0 ? f.Inverse : f;
	}

	public static FixedFloat Max(FixedFloat f1, FixedFloat f2){
		return f1 > f2 ? f1 : f2;
	}

	public static FixedFloat Min(FixedFloat f1, FixedFloat f2){
		return f1 < f2 ? f1 : f2;
	}
	#endregion

}

