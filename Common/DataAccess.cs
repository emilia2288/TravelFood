using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Configuration;
using System.Data.SqlClient;

namespace TravelFood
{
    public class DataAccess
    {
        private static string strConnString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public DataTable GetDataTableFromQuery(string strQry)
        {
            SqlCommand cmd = new SqlCommand(strQry);
            using (SqlConnection con = new SqlConnection(strConnString))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    sda.SelectCommand = cmd;
                    using (DataTable dt = new DataTable())
                    {
                        sda.Fill(dt);
                        con.Close();
                        return dt;
                    }
                }
            }
        }

        public void InsertDataFromDataTable(DataTable dt, string strTableName)
        {
            using (SqlConnection con = new SqlConnection(strConnString))
            {
                con.Open();
                SqlBulkCopy sbc = new SqlBulkCopy(con);
                sbc.DestinationTableName = strTableName;
                sbc.WriteToServer(dt);
                sbc.Close();
            }
        }

        public void UpdateDataTableFromQuery(string strQry)
        {
            SqlCommand cmd = new SqlCommand(strQry);
            using (SqlConnection con = new SqlConnection(strConnString))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }
    }
}