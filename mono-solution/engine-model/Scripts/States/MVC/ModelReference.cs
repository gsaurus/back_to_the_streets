using System;
using ProtoBuf;

namespace RetroBread{

	[ProtoContract]
	public sealed class ModelReference: IComparable<ModelReference>{

		[ProtoMember(1)]
		public const uint InvalidModelIndex = 0;

		[ProtoMember(2)]
		public uint index; // private

		public ModelReference():this(InvalidModelIndex){}

		public ModelReference(uint index){
			this.index = index;
		}

		// explicit update method
		public void UpdateIndex(uint index){
			this.index = index;
		}


		public static implicit operator uint(ModelReference modelRef){
			return modelRef.index;
		}


		public override string ToString (){
			return index.ToString();
		}


	#region operators

		// Implement IComparable CompareTo to provide default sort order.
		public int CompareTo(ModelReference other){
			return index.CompareTo(other.index);
		}

		public override bool Equals(object obj)
		{
			if (obj is ModelReference){
				return ((ModelReference)obj).index == this.index;
			}else{
				return false;
			}
		}
		
		public override int GetHashCode(){
			return index.GetHashCode();
		}


		public static bool operator ==(ModelReference one, ModelReference other){
			if (object.ReferenceEquals(one,null)) return object.ReferenceEquals(other,null);
			if (object.ReferenceEquals(other,null)) return object.ReferenceEquals(one,null);
			return one.index == other.index;
		}
		
		public static bool operator ==(ModelReference one, uint other){
			if (object.ReferenceEquals(one,null)) return false;
			return one.index == other;
		}
		
		public static bool operator ==(uint other, ModelReference one){
			if (object.ReferenceEquals(one,null)) return false;
			return other == one.index;
		}
		public static bool operator !=(ModelReference one, ModelReference other){
			if (object.ReferenceEquals(one,null)) return !object.ReferenceEquals(other,null);
			if (object.ReferenceEquals(other,null)) return !object.ReferenceEquals(one,null);
			return one.index != other.index;
		}
		
		public static bool operator !=(ModelReference one, uint other){
			if (object.ReferenceEquals(one,null)) return true;
			return one.index != other;
		}
		
		public static bool operator !=(uint one, ModelReference other){
			if (object.ReferenceEquals(one,null)) return true;
			return other.index != one;
		}

	#endregion

	}


}

