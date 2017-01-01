using UnityEngine;
using System.Collections.Generic;

namespace RetroBread{

public static class ConditionUtils<T>{

	public enum ComparisonOperation {
		equal			= 0,
		notEqual		= 1,
		less			= 2,
		lessOrEqual 	= 3,
		greater			= 4,
		greaterOrEqual 	= 5
	}
			

	public static bool Compare(ComparisonOperation operation, int left, int right){
		// compare them
		int result = left.CompareTo(right);
		switch (operation){
			case ComparisonOperation.equal:			return result == 0;
			case ComparisonOperation.notEqual:		return result != 0;
			case ComparisonOperation.less:			return result <  0;
			case ComparisonOperation.lessOrEqual:	return result <= 0;
			case ComparisonOperation.greater:		return result >  0;
			case ComparisonOperation.greaterOrEqual:return result >= 0;
		}
		return false;
	}

	public static List<T> GetNonEmptySubjectOrNil(List<T>[] subjects, int subjectIndex){
		if (subjectIndex < 0 || subjectIndex >= subjects.Length) return null;
		List<T> result = subjects[subjectIndex];
		if (result == null || result.Count == 0) return null;
		return result;
	}

}


}

