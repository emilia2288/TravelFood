using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using Facebook;
using Newtonsoft.Json;

using System.Threading;

namespace TravelFood
{
    public class CheckinModule
    {
        #region
        public struct FriendPlace
        {
            public string fid;
            public string pid;
            public int count;
            public bool isclose;
        }

        public struct Checkin
        {
            public string pid;
            public int create_date;
        }

        public struct CategoryData
        {
            public string cid;
            public int count;
        }
        #endregion

        private static string strUID = HttpContext.Current.Request.Cookies["FacebookID"].Value;
        private static int i_LastTime = 0;
        private static int i_Time = 0;
        private static DataTable dt_Freq = new DataTable();
        private static DataAccess da = new DataAccess();
        private static FacebookAccess fa = new FacebookAccess();

        public static List<FriendPlace> GetPlaceRating(string strPlaces)
        {
            string strQry = @"
SELECT D.*, F.[IsClose] 
  FROM [tf_Friends] F, (
	SELECT [UID], [PID], COUNT([PID]) AS [COUNT]
	  FROM [tf_Checkins] 
     WHERE [PID] IN ({0})
       AND [UID] IN ( SELECT [FID] FROM [tf_Friends] WHERE [UID] = '{1}' )
	 GROUP BY [UID], [PID])D
 WHERE D.[UID] = F.[FID]
 ORDER BY [UID], [PID];
";
            DataTable dt = da.GetDataTableFromQuery(string.Format(strQry, strPlaces, strUID));
            List<FriendPlace> liFriendPlace = new List<FriendPlace>();
            FriendPlace fp = new FriendPlace();
            foreach (DataRow dr in dt.Rows)
            {
                fp.fid = dr["UID"].ToString();
                fp.pid = dr["PID"].ToString();
                fp.count = int.Parse(dr["COUNT"].ToString());
                fp.isclose = (dr["IsClose"].ToString() == @"True") ? true : false;
                liFriendPlace.Add(fp);
            }

            return liFriendPlace;
        }

        public static bool GetCheckinData(string strFID)
        {
            string strQry = @"
SELECT TOP 1 [Update_Date]
  FROM [tf_FoodFrequency] 
 WHERE [UID] = '{0}';
";
            DataTable dt = da.GetDataTableFromQuery(string.Format(strQry, strFID));
            if (dt.Rows.Count > 0)
            {
                i_LastTime = int.Parse(da.GetDataTableFromQuery(string.Format(strQry, strFID)).Rows[0]["Update_Date"].ToString());
            }

            strQry = @"
SELECT page_id, timestamp
  FROM location_post
 WHERE author_uid = '{0}'
    OR '{0}' IN tagged_uids
 LIMIT 1000
";
            dynamic results = fa.GetFacebookDataByFQL(string.Format(strQry, strFID));
            
            // 解析 checkins
            results = results.data;
            List<string> liCheckin = new List<string>();
            List<Checkin> liPlace = new List<Checkin>();
            Checkin pd = new Checkin();
            foreach (dynamic checkins in results)
            {
                if (i_LastTime > 0) 
                {
                    if (checkins.timestamp <= i_LastTime) { break; }
                }

                pd.create_date = checkins.timestamp;
                if (checkins.timestamp > i_Time) { i_Time = checkins.timestamp; }                

                if (checkins.page_id != null)
                {
                    string strPID = checkins.page_id;
                    pd.pid = strPID;
                    liPlace.Add(pd);
                }
            }

            if (liPlace.Count > 0) 
            { 
                WriteCheckinsToDB(strFID, liPlace);
                return true;
            }
            else { return false; }
        }

        private static void WriteCheckinsToDB(string strFID, List<Checkin> liCheckin)
        {
            string strTableName = @"tf_Checkins";
            string strQry = @"
SELECT * FROM {0} WHERE 1 = 2;";
            DataTable dt = da.GetDataTableFromQuery(string.Format(strQry, strTableName));

            foreach (Checkin place in liCheckin)
            {
                DataRow dr = dt.NewRow();
                dr["UID"] = strFID;
                dr["PID"] = place.pid;
                dr["Create_Date"] = place.create_date;
                dt.Rows.Add(dr);
            }

            // 資料轉入DB
            da.InsertDataFromDataTable(dt, strTableName);
        }

        public static void GetFoodTypeFrequencyBySeason(string strFID)
        {
            // Get Checkin Data
            string strQry = @"
SELECT [PID], [Create_Date], CASE WHEN DATEPART(mm, [Create_Date]) IN (1, 2, 3) THEN 1
                                  WHEN DATEPART(mm, [Create_Date]) IN (4, 5, 6) THEN 2
                                  WHEN DATEPART(mm, [Create_Date]) IN (7, 8, 9) THEN 3
                                  ELSE 4 END AS [SEASON]
  FROM (
    SELECT * FROM (
        SELECT [PID], DATEADD(hh, 8, DATEADD(s, [Create_Date], '19700101')) AS [Create_Date] 
          FROM [tf_Checkins] 
         WHERE [UID] = '{0}')D
    GROUP BY [PID], [Create_Date])D
ORDER BY [SEASON], [PID];
";
            DataTable dtResults = da.GetDataTableFromQuery(string.Format(strQry, strFID));
            List<string> liPlace_1 = new List<string>();
            List<string> liPlace_2 = new List<string>();
            List<string> liPlace_3 = new List<string>();
            List<string> liPlace_4 = new List<string>();
            foreach (DataRow dr in dtResults.Rows)
            {
                if (dr["SEASON"].ToString() == @"1") { liPlace_1.Add(dr["PID"].ToString()); }
                if (dr["SEASON"].ToString() == @"2") { liPlace_2.Add(dr["PID"].ToString()); }
                if (dr["SEASON"].ToString() == @"3") { liPlace_3.Add(dr["PID"].ToString()); }
                if (dr["SEASON"].ToString() == @"4") { liPlace_4.Add(dr["PID"].ToString()); }
            }

            try
            {
                // 取得 Table欄位
                strQry = @"
SELECT * FROM [tf_FoodFrequency] WHERE 1=2;
";
                dt_Freq = da.GetDataTableFromQuery(strQry);

                Thread thr1 = new Thread(delegate() { GetFrequency(liPlace_1, 1, dt_Freq, strFID); });
                Thread thr2 = new Thread(delegate() { GetFrequency(liPlace_2, 2, dt_Freq, strFID); });
                Thread thr3 = new Thread(delegate() { GetFrequency(liPlace_3, 3, dt_Freq, strFID); });
                Thread thr4 = new Thread(delegate() { GetFrequency(liPlace_4, 4, dt_Freq, strFID); });

                thr1.Start();
                thr2.Start();
                thr3.Start();
                thr4.Start();

                thr1.Join();
                thr2.Join();
                thr3.Join();
                thr4.Join();

                if (dt_Freq.Rows.Count > 0) { WriteInterestToDB(dt_Freq, strFID); }
            }
            catch { }
        }

        private static DataTable GetFrequency(List<string> liPlace, int i_Season, DataTable dt, string strFID)
        {
            int i_FoodCount = 0;
            List<string> liCategories = new List<string>();
            for (int i = 0; i < liPlace.Count; )
            {
                string strPlace = liPlace[i];
                List<string> liTemp = liPlace.FindAll(delegate(string d) { return d == liPlace[i]; });
                i += liTemp.Count;

                string strQry = @"
SELECT page_id, categories
  FROM page
 WHERE page_id = '{0}'
";
                dynamic results = fa.GetFacebookDataByFQL(string.Format(strQry, strPlace));
                
                // 解析 page
                results = results.data;
                bool is_Food = false;
                foreach (dynamic page in results)
                {
                    if (page.categories != null)
                    {
                        dynamic categories = page.categories;
                        foreach (dynamic data in categories)
                        {
                            string id = data.id;
                            liCategories.Add(id);

                            if (!is_Food)
                            {
                                strQry = @"
SELECT * FROM [tf_PlaceTopic] WHERE [CID] = '{0}'
";
                                DataTable dtTemp = da.GetDataTableFromQuery(string.Format(strQry, id));
                                if (dtTemp.Rows.Count > 0)
                                {
                                    i_FoodCount++;
                                    is_Food = true;
                                }
                            }
                        }
                        is_Food = false;
                    }
                }
            }

            // 計算各類別的頻率
            liCategories.Sort();
            List<string> tempData = new List<string>();
            List<CategoryData> li_Category = new List<CategoryData>();
            CategoryData temp = new CategoryData();
            for (int i = 0; i < liCategories.Count; )
            {
                tempData = liCategories.FindAll(delegate(string d) { return d == liCategories[i]; });
                temp.cid = liCategories[i];
                temp.count = tempData.Count;

                li_Category.Add(temp);
                i += temp.count;
            }

            DataRow dr = dt.NewRow();
            foreach (CategoryData cd in li_Category)
            {
                try
                {
                    dr[cd.cid] = (float)cd.count / i_FoodCount;
                }
                catch { }
            }
            dr["UID"] = strFID;
            dr["SEASON"] = i_Season;
            dr["Creator"] = strUID;
            dr["Update_Date"] = i_Time;
            dt.Rows.Add(dr);

            return dt;
        }        

        private static void WriteInterestToDB(DataTable dt, string strFID)
        {
            string strTableName = @"tf_FoodFrequency";
            
            // 清理現有資料
            string strQry = @"
DELETE {0} WHERE [UID] = '{1}';
";
            da.UpdateDataTableFromQuery(string.Format(strQry, strTableName, strFID));
            // 資料轉入DB
            da.InsertDataFromDataTable(dt, strTableName);
        }
    }
}