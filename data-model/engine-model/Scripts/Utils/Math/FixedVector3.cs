using System;
using ProtoBuf;

namespace RetroBread{

	[ProtoContract]
	public struct FixedVector3{

		// Static helper variables
		public static readonly FixedVector3 Back		= new FixedVector3( 0, 0,-1);
		public static readonly FixedVector3 Down		= new FixedVector3( 0,-1, 0);
		public static readonly FixedVector3 Forward		= new FixedVector3( 0, 0, 1);
		public static readonly FixedVector3 Left		= new FixedVector3(-1, 0, 0);
		public static readonly FixedVector3 Right		= new FixedVector3( 1, 0, 0);
		public static readonly FixedVector3 Up			= new FixedVector3( 0, 1, 0);

		public static readonly FixedVector3 One			= new FixedVector3(1, 1, 1);
		public static readonly FixedVector3 Zero		= new FixedVector3(0, 0, 0);

	#region Variables

		// Vector components (x,y)

		private FixedFloat x;
		private FixedFloat y;
		private FixedFloat z;

		[ProtoMember(1)]
		public FixedFloat X {

			get {
				return x;
			}

			set {
				x = value;
				magnitudeIsUpToDate = false;
			}
		}

		[ProtoMember(2)]
		public FixedFloat Y {
			
			get {
				return y;
			}
			
			set {
				y = value;
				magnitudeIsUpToDate = false;
			}
		}

		[ProtoMember(3)]
		public FixedFloat Z {
			
			get {
				return z;
			}
			
			set {
				z = value;
				magnitudeIsUpToDate = false;
			}
		}

		// Magnitude (lazy update)
		private FixedFloat magnitude;
		private bool magnitudeIsUpToDate;
		public FixedFloat Magnitude {
			get{
				if (!magnitudeIsUpToDate) {
					if (x == 0 && y == 0 && z == 0){
						magnitude = 0;
					}else {
						// deal with precision errors if some values are too low
						FixedFloat absX = FixedFloat.Abs(x);
						FixedFloat absY = FixedFloat.Abs(y);
						FixedFloat absZ = FixedFloat.Abs(z);
						if ((
							    (x != 0 && absX < 0.1)
						     || (y != 0 && absY < 0.1)
						     || (z != 0 && absZ < 0.1)
						    )
						    && (absX < 10000 && absY < 10000 && absZ < 10000)
						){
							// multiplications will result in very small values, so let's use a multiplier
							magnitude = FixedFloat.Sqrt((10000*x)*x + (10000*y)*y + (10000*z)*z);
							magnitude /= 100;
						}else {
							magnitude = FixedFloat.Sqrt(x*x + y*y + z*z);
						}
					}
					magnitudeIsUpToDate = true;
				}
				return magnitude;
			}
		}

		public FixedFloat SqrMagnitude{
			get{
				return x*x + y*y + z*z;
			}
		}
		
		public FixedVector3 Normalized {
			get{
				if (Magnitude != 0){

					FixedFloat scale;
					if (Magnitude < 1000) {
						scale = (FixedFloat.One / Magnitude) * 1.001;
						return new FixedVector3(x*scale, y*scale, z*scale);
					}else {
						scale = 10000 / Magnitude;
						scale *= 1.001;
						return new FixedVector3((x*scale) / 10000, (y*scale) / 10000, (z*scale) / 10000);
					}
				}
				return Zero;
			}
		}

		public void Set(FixedFloat x, FixedFloat y, FixedFloat z){
			this.x = x;
			this.y = y;
			this.z = z;
			magnitudeIsUpToDate = false;
		}

	#endregion

	#region Constructors

		public FixedVector3(FixedFloat x, FixedFloat y, FixedFloat z){
			this.x = x;
			this.y = y;
			this.z = z;
			magnitude = 0;
			magnitudeIsUpToDate = false;
		}

	#endregion


	#region Operators

		public static FixedVector3 operator +(FixedVector3 left, FixedVector3 right){
			left.Set(left.x + right.x, left.y + right.y, left.z + right.z);
			return left;
		}

		public static FixedVector3 operator -(FixedVector3 left, FixedVector3 right){
			left.Set(left.x - right.x, left.y - right.y, left.z - right.z);
			return left;
		}

		// Negation
		public static FixedVector3 operator -(FixedVector3 vec){
			vec.Set(-vec.x, -vec.y, -vec.z);
			return vec;
		}


		public static FixedVector3 operator *(FixedVector3 vec, FixedFloat scale){
	//		if (scale != 0 && ((vec.x != 0 && vec.x * scale == 0) || (vec.y != 0 && vec.y * scale == 0) || (vec.z != 0 && vec.z * scale == 0))){
	//			UnityEngine.Debug.Log("Precision error: MULT left");
	//		}
			vec.Set(vec.x * scale, vec.y * scale, vec.z * scale);
			return vec;
		}

		public static FixedVector3 operator *(FixedFloat scale, FixedVector3 vec){
	//		if (scale != 0 && ((vec.x != 0 && vec.x * scale == 0) || (vec.y != 0 && vec.y * scale == 0) || (vec.z != 0 && vec.z * scale == 0))){
	//			UnityEngine.Debug.Log("Precision error: MULT right");
	//		}
			vec.Set(vec.x * scale, vec.y * scale, vec.z * scale);
			return vec;
		}

		public static FixedVector3 operator /(FixedVector3 vec, FixedFloat scale){
			FixedFloat mult = FixedFloat.One / scale;
	//		if (mult == 0){
	//			UnityEngine.Debug.Log("Precision error: DIV");
	//		}
			vec.Set (vec.x * mult, vec.y * mult, vec.z * mult);
			return vec;
		}

		public static bool operator ==(FixedVector3 left, FixedVector3 right){
			return left.Equals(right);
		}

		public static bool operator !=(FixedVector3 left, FixedVector3 right){
			return !left.Equals(right);
		}
	#endregion

	#region Overrides

		public override string ToString(){
			return String.Format("Vector3({0}, {1}, {2})", x, y, z);
		}

		public override int GetHashCode(){
			return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
		}

		public override bool Equals(object obj){
			if (!(obj is FixedVector3)){
				return false;
			}
			return this.Equals((FixedVector3)obj);
		}

		public bool Equals(FixedVector3 other){
			return x == other.x && y == other.y && z == other.z;
		}

	#endregion


	#region Object Functions

		public void Normalize(){
			FixedVector3 normalized = Normalized;
			Set(normalized.x, normalized.y, normalized.z);
		}

	#endregion



	#region Static Functions


		public static FixedVector3 Max(FixedVector3 first, FixedVector3 second){
			first.Set(first.x > second.x ? first.x : second.x,
			          first.y > second.y ? first.y : second.y,
			          first.z > second.z ? first.z : second.z
			          );
			return first;
		}

		public static FixedVector3 Min(FixedVector3 first, FixedVector3 second){
			first.Set(first.x < second.x ? first.x : second.x,
			          first.y < second.y ? first.y : second.y,
			          first.z < second.z ? first.z : second.z
			          );
			return first;
		}


		public static FixedVector3 Lerp(FixedVector3 first, FixedVector3 second, FixedFloat blend){
			first.Set(first.x + (second.x - first.x)*blend,
			          first.y + (second.y - first.y)*blend,
			          first.z + (second.z - first.z)*blend
			          );
			return first;
		}


		public static FixedVector3 Clamp(FixedVector3 vec, FixedVector3 min, FixedVector3 max){
			vec.Set (FixedFloat.Clamp(vec.x, min.x, max.x),
			         FixedFloat.Clamp(vec.y, min.y, max.y),
			         FixedFloat.Clamp(vec.z, min.z, max.z)
			        );
			return vec;
		}


		public static FixedVector3 Scale(FixedVector3 vec, FixedVector3 scale){
			vec.Set(vec.x * scale.x, vec.y * scale.y, vec.z * scale.z);
			return vec;
		}


		public static FixedVector3 ClampMagnitude(FixedVector3 vec, FixedFloat maxMagnitude){
			FixedFloat magnitude = vec.Magnitude;
			if (magnitude > maxMagnitude){
				FixedFloat scaleFactor = magnitude / maxMagnitude;
				vec *= scaleFactor;
			}
			return vec;
		}

		public static FixedFloat Angle(FixedVector3 from, FixedVector3 to){
			if (from.Magnitude == FixedFloat.Zero || to.Magnitude == FixedFloat.Zero) return FixedFloat.Zero;
			return FixedFloat.Acos((FixedVector3.Dot(from, to)) / (from.Magnitude * to.Magnitude));
		}


		public static FixedFloat Distance(FixedVector3 from, FixedVector3 to){
			to -= from;
			return to.Magnitude;
		}


		public static FixedFloat Dot(FixedVector3 left, FixedVector3 right){
	//		FixedFloat ret = left.x * right.x + left.y * right.y + left.z * right.z;
	//		if (   (left.x != 0 && right.x != 0 && left.x * right.x == 0)
	//		    || (left.y != 0 && right.y != 0 && left.y* right.y == 0)
	//		    || (left.z != 0 && right.z != 0 && left.z * right.z == 0)
	//		){
	//			UnityEngine.Debug.Log("Precision error, dot became " + ret + " instead of " + ((float)left.x * (float)right.x + (float)left.y * (float)right.y + (float)left.z * (float)right.z));
	//		}
			return left.x * right.x + left.y * right.y + left.z * right.z;;
		}


		public static FixedVector3 Cross(FixedVector3 left, FixedVector3 right){
	//		if (   (left.y != 0 && right.z != 0 && left.y * right.z == 0)
	//		    || (left.z != 0 && right.y != 0 && left.z * right.y == 0)
	//		    || (left.z != 0 && right.x != 0 && left.z * right.x == 0)
	//		    || (left.x != 0 && right.z != 0 && left.x * right.z == 0)
	//		    || (left.x != 0 && right.y != 0 && left.x * right.y == 0)
	//		    || (left.y != 0 && right.x != 0 && left.y * right.x == 0)
	//		){
	//			UnityEngine.Debug.Log("Precision error: CROSS");
	//		}
			left.Set(left.y * right.z - left.z * right.y,
			         left.z * right.x - left.x * right.z,
			         left.x * right.y - left.y * right.x
			        );
			return left;
		}


	#endregion


	}

}
