using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayID.DataHelper
{
    public static class MongoDynamic
    {
        private static System.Text.RegularExpressions.Regex objectIdReplace = new System.Text.RegularExpressions.Regex(@"ObjectId\((.[a-f0-9]{24}.)\)", System.Text.RegularExpressions.RegexOptions.Compiled);
        /// <summary>
        /// deserializes this bson doc to a .net dynamic object
        /// </summary>
        /// <param name="bson">bson doc to convert to dynamic</param>
        public static dynamic ToDynamic(this BsonDocument bson)
        {
            var json = objectIdReplace.Replace(bson.ToJson(), (s) => s.Groups[1].Value);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
        }
    }
}
