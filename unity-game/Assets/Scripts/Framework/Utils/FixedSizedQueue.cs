using System.Collections.Generic;

namespace RetroBread{

	public class FixedSizedQueue<T>: Queue<T>
	{
		private int size;
		public int Size {
			get{
				return size;
			}
			set{
				size = value;
				// discard excess
				while (base.Count > Size){
					base.Dequeue();
				}
			}
		}
		
		public FixedSizedQueue(int size):base(size){
			this.Size = size;
		}
		
		public new void Enqueue(T obj){
			base.Enqueue(obj);
			while (base.Count > Size){
				base.Dequeue();
			}
		}
	}

}

