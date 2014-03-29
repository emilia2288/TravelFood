using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using Facebook;
using Newtonsoft.Json;

namespace TravelFood
{
    public class PlaceModule
    {
        private static FacebookAccess fa = new FacebookAccess();
        private static DataAccess da = new DataAccess();

        private static List<string> GetPlaceTopic()
        {
            string strQry = @"
SELECT [CID]
  FROM [tf_PlaceTopic];
";
            DataTable dt = da.GetDataTableFromQuery(strQry);
            List<string> strPlaceTopic = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                strPlaceTopic.Add(dr[0].ToString());
            }

            return strPlaceTopic;
        }

        public static string[] GetPlaceID(float q_fLatitude, float q_fLongitude)
        {
            // Get Place Topic
            List<string> li_PlaceTopic = GetPlaceTopic();

            // 取得附近地標
            string strQry = @"search?type=place&center={0},{1}&distance=1500&offset={2}";
            int intOffset = 0;
            dynamic results = fa.GetFacebookDataByAPI(string.Format(strQry, q_fLatitude, q_fLongitude, intOffset));
            
            // 解析 place
            List<string> liPlace = new List<string>();
            bool hasPaging = true;
            results = results.data;
            while (hasPaging)
            {
                foreach (dynamic place in results)
                {
                    // Get category_list
                    if (place.category_list != null)
                    {
                        dynamic category_list = place.category_list;
                        foreach (dynamic category in category_list)
                        {
                            string strCID = category.id;
                            string strPID = place.id;
                            if (li_PlaceTopic.Contains(strCID))
                            {
                                // Get ID
                                liPlace.Add(strPID);
                                break;
                            }
                        }
                    }
                }

                intOffset += results.Count;
                results = fa.GetFacebookDataByAPI(string.Format(strQry, q_fLatitude, q_fLongitude, intOffset));
                results = results.data;
                if (results.ToString() == @"[]") { hasPaging = false; }
            }

            return liPlace.ToArray();
        }
        
        public static string GetPlaceByDistance(string[] strPlace, float q_fLatitude, float q_fLongitude)
        {
            string strPlaces = string.Empty;
            for (int i = 0; i < strPlace.Length; i++)
            {
                string strPID = strPlace[i].ToString();
                if (i != strPlace.Length - 1) { strPlaces += string.Format(@"'{0}',", strPID); }
                else { strPlaces += string.Format(@"'{0}'", strPID); }
            }

            string strQry = @"
SELECT name, checkins, location.street, location.latitude, location.longitude, page_url, website, pic
  FROM page 
 WHERE page_id IN (
    SELECT page_id
      FROM place 
     WHERE page_id IN ({0})
     ORDER BY distance(latitude, longitude, '{1}', '{2}')
     LIMIT 10)
";
            
            return fa.GetFacebookDataByFQL(string.Format(strQry, strPlaces, q_fLatitude, q_fLongitude)).ToString();
        }

        public static string GetPlaceByCheckins(string[] strPlace)
        {
            string strPlaces = string.Empty;
            for (int i = 0; i < strPlace.Length; i++)
            {
                string strPID = strPlace[i].ToString();
                if (i != strPlace.Length - 1) { strPlaces += string.Format(@"'{0}',", strPID); }
                else { strPlaces += string.Format(@"'{0}'", strPID); }
            }

            string strQry = @"
SELECT name, checkins, location.street, location.latitude, location.longitude, page_url, website, pic
  FROM page 
 WHERE page_id IN (
      SELECT page_id
        FROM place 
       WHERE page_id IN ({0})
    ORDER BY checkin_count DESC
       LIMIT 10)
";

            return fa.GetFacebookDataByFQL(string.Format(strQry, strPlaces)).ToString();
        }

        public static string GetPlaceByPID(List<UserModule.PlaceRating> li_PlaceRating)
        {
            string strPlaces = string.Empty;
            for (int i = 0; i < li_PlaceRating.Count; i++)
            {
                string strPID = li_PlaceRating[i].pid;
                if (i != li_PlaceRating.Count - 1) { strPlaces += string.Format(@"'{0}',", strPID); }
                else { strPlaces += string.Format(@"'{0}'", strPID); }
            }

            string strQry = @"
SELECT name, checkins, location.street, location.latitude, location.longitude, page_url, website, pic
FROM   page 
WHERE  page_id IN (
      SELECT page_id
        FROM place 
       WHERE page_id IN ({0})
LIMIT  10)
";
            strQry = string.Format(strQry, strPlaces);
            return fa.GetFacebookDataStringByFQL(strQry);
        }

//        public static string GetPlaceByFriends(string strPlaces, string q_strAccessToken)
//        {
//            var accessToken = q_strAccessToken;
//            var client = new FacebookClient(accessToken);

//            // 取得有去過的朋友
//            string strQry = @"
//SELECT uid2 
//  FROM friend 
// WHERE uid1 = me() 
//   AND uid2 IN (
//    SELECT author_uid, tagged_uids
//      FROM location_post 
//     WHERE page_id IN ({0})))
//";
//            strQry = string.Format(strQry, strPlaces);
//            dynamic results = client.Get("fql", new { q = strQry }).ToString();

//            // 解析 friend
//            bool hasPaging = true;
//            results = results.data;
//            while (hasPaging)
//            {

//                foreach (dynamic place in results)
//                {
//                    // Get category_list
//                    if (place.category_list != null)
//                    {
//                        dynamic category_list = place.category_list;
//                        foreach (dynamic category in category_list)
//                        {
//                            string strCID = category.id;
//                            string strPID = place.id;
//                            if (strPlaceTopic.Contains(strCID))
//                            {
//                                // Get ID
//                                liPlace.Add(strPID);
//                                break;
//                            }
//                        }
//                    }
//                }

//                intOffset += results.Count;
//                results = client.Get(string.Format(strQry, q_fLatitude, q_fLongitude, intOffset)).ToString();
//                results = JsonConvert.DeserializeObject(results);
//                results = results.data;
//                if (results.ToString() == @"[]") { hasPaging = false; }
//            }

//            return results;
//        }
    }
}