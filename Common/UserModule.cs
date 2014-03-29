using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;

namespace TravelFood
{
    public class UserModule
    {
        #region
        public struct FriendSim
        {
            public string fid;
            public double sim;
        }

        public struct PlaceRating
        {
            public string pid;
            public double rating;
        }

        public class PlaceRatingCompare : IComparer<PlaceRating>
        {
            public int Compare(PlaceRating x, PlaceRating y)
            {
                return (y.rating.CompareTo(x.rating));
            }
        }
        #endregion

        private static string strUID = HttpContext.Current.Request.Cookies["FacebookID"].Value;
        private static DataAccess da = new DataAccess();

        public static List<PlaceRating> GetUserSimilarity(string[] strPlace)
        {
            string strPlaces = string.Empty;
            for (int i = 0; i < strPlace.Length; i++)
            {
                string strPID = strPlace[i].ToString();
                if (i != strPlace.Length - 1) { strPlaces += string.Format(@"'{0}',", strPID); }
                else { strPlaces += string.Format(@"'{0}'", strPID); }
            }

            // 取得去過這些地點的使用者以及使用者評分(去過次數)
            List<CheckinModule.FriendPlace> liFriendPlace = CheckinModule.GetPlaceRating(strPlaces);

            // 計算頻率
            List<string> li_Place = new List<string>();
            List<FriendSim> liSim = new List<FriendSim>();
            FriendSim sd = new FriendSim();
            foreach (CheckinModule.FriendPlace fp in liFriendPlace)
            {
                // Get Place
                string strPlaceTemp = li_Place.Find(delegate(string s) { return s == fp.pid; });
                if (strPlaceTemp == null || strPlaceTemp == string.Empty) { li_Place.Add(fp.pid); }

                // Get User Similarity
                List<FriendSim> fs = liSim.FindAll(delegate(FriendSim f) { return f.fid == fp.fid; });
                if (fs.Count == 0)
                {
                    string strFID = fp.fid;
                    bool isUpdate = CheckinModule.GetCheckinData(strFID);
                    if (isUpdate) { CheckinModule.GetFoodTypeFrequencyBySeason(strFID); }

                    // 計算相似度
                    string strQry = @"
SELECT * 
  FROM [tf_FoodFrequency] 
 WHERE [UID] IN ('{0}', '{1}') 
   AND [SEASON] = CASE WHEN DATEPART(mm, getdate()) IN (1, 2, 3) THEN 1
	                   WHEN DATEPART(mm, getdate()) IN (4, 5, 6) THEN 2
	                   WHEN DATEPART(mm, getdate()) IN (7, 8, 9) THEN 3
	              ELSE 4 END;
";
                    DataTable dtFreq = da.GetDataTableFromQuery(string.Format(strQry, strUID, strFID));
                    if (dtFreq.Rows.Count == 2)
                    {
                        double d_uid1 = 0;
                        double d_uid2 = 0;
                        double d_numerator = 0;
                        for (int i = 2; i < dtFreq.Columns.Count - 4; i++)
                        {
                            DataColumn dc = dtFreq.Columns[i];
                            d_numerator += double.Parse(dtFreq.Rows[0][dc.ColumnName].ToString()) * double.Parse(dtFreq.Rows[1][dc.ColumnName].ToString());
                            d_uid1 += Math.Pow(double.Parse(dtFreq.Rows[0][dc.ColumnName].ToString()), 2);
                            d_uid2 += Math.Pow(double.Parse(dtFreq.Rows[1][dc.ColumnName].ToString()), 2);
                        }
                        sd.fid = strFID;
                        sd.sim = d_numerator / (Math.Sqrt(d_uid1) * Math.Sqrt(d_uid2));
                        liSim.Add(sd);
                    }
                }
            }

            // 計算地點推薦度
            List<PlaceRating> li_PlaceRating = new List<PlaceRating>();
            PlaceRating pr = new PlaceRating();
            foreach (string place in li_Place)
            {
                double d_numerator = 0;   // 分子
                double d_denominator = 0; // 分母
                List<CheckinModule.FriendPlace> liData = liFriendPlace.FindAll(delegate(CheckinModule.FriendPlace fd) { return fd.pid == place; });
                foreach (CheckinModule.FriendPlace data in liData)
                {
                    FriendSim s_fid = liSim.Find(delegate(FriendSim s) { return s.fid == data.fid; });
                    double i_Close = 1;
                    if (!data.isclose) { i_Close = 0.75; }
                    d_numerator += s_fid.sim * data.count * i_Close;
                    d_denominator += s_fid.sim;
                }
                pr.pid = place;
                pr.rating = d_numerator / d_denominator;
                li_PlaceRating.Add(pr);
            }
            li_PlaceRating.Sort(new PlaceRatingCompare());

            return li_PlaceRating;
        }

        
    }
}