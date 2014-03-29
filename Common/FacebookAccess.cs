using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using Facebook;
using Newtonsoft.Json;

namespace TravelFood
{
    public class FacebookAccess
    {
        public static string strAccessToken = string.Empty;

        public FacebookAccess()
        {
            strAccessToken = HttpContext.Current.Request.Cookies["AccessToken"].Value;
        }

        public dynamic GetFacebookDataByAPI(string strQry)
        {
            var accessToken = strAccessToken;
            var client = new FacebookClient(accessToken);

            dynamic results = client.Get(strQry).ToString();
            return JsonConvert.DeserializeObject(results);
        }

        public dynamic GetFacebookDataByFQL(string strQry)
        {
            var accessToken = strAccessToken;
            var client = new FacebookClient(accessToken);

            dynamic results = client.Get("fql", new { q = strQry }).ToString();
            return JsonConvert.DeserializeObject(results);
        }

        public string GetFacebookDataStringByFQL(string strQry)
        {
            var accessToken = strAccessToken;
            var client = new FacebookClient(accessToken);

            return client.Get("fql", new { q = strQry }).ToString();
        }

        public dynamic GetFacebookDataByPagingURL(string strQry)
        {
            WebClient wc = new WebClient();
            dynamic results = wc.DownloadString(strQry);

            return JsonConvert.DeserializeObject(results);
        }
    }
}