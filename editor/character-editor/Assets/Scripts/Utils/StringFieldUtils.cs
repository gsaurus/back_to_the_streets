using System;

namespace RetroBread
{
	public static class StringFieldUtils
	{
	public static bool TryEvaluate(string expression, out float result)
     {
         var doc = new System.Xml.XPath.XPathDocument(new System.IO.StringReader("<r/>"));
         var nav = doc.CreateNavigator();
         var newString = expression;
         //newString = (new System.Text.RegularExpressions.Regex(@"([\+\-\*])")).Replace(newString, " ${1} ");
         newString = newString.Replace("/", " div ").Replace("%", " mod ");
         try{
         	newString = nav.Evaluate("number(" + newString + ")").ToString();
     	}catch(Exception e){
     		result = 0;
     		return false;
     	}
         return float.TryParse(newString, out result);
     }
	}
}

