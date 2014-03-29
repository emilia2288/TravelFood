using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Services;

namespace TravelFood.Food
{
    public partial class FoodByDistance : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        [WebMethod]
        public static string GetPlace(string v_Latitude, string v_Longitude)
        {
            string results = string.Empty;
            if (HttpContext.Current.Request.Cookies["AccessToken"].Value != null)
            {
                float flatitude = float.Parse(v_Latitude);
                float flongitude = float.Parse(v_Longitude);
                string[] strPlace = PlaceModule.GetPlaceID(flatitude, flongitude);

                results = PlaceModule.GetPlaceByDistance(strPlace, flatitude, flongitude);
            }

            return results;
        }
    }
}