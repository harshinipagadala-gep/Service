using System.Diagnostics.CodeAnalysis;
using System.Web.Script.Serialization;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    [ExcludeFromCodeCoverage]
    internal static class JsonConverterHelper
    {

        /// <summary> An object extension method that converts an obj to a JSON. </summary>
         	///
         	/// <param name="obj"> The obj to act on. </param>
         	/// <param name="recursionDepth"> Depth of the recursion. </param>
         	/// <param name="JSONLength"> Length of the JSON. </param>
         	///
         	/// <returns> obj as a string. </returns>
        public static string ToJSON(this object obj, int recursionDepth, int JSONLength)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RecursionLimit = recursionDepth;
            serializer.MaxJsonLength = JSONLength;
            return serializer.Serialize(obj);

        }



    }
}
