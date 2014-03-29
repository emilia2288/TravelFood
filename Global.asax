<%@ Application Language="C#" %>
<%@ Import Namespace="TravelFood" %>
<%@ Import Namespace="System.Web.Routing" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        // 應用程式啟動時執行的程式碼
        AuthConfig.RegisterOpenAuth();
        RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
    
    void Application_End(object sender, EventArgs e)
    {
        //  應用程式啟動時執行的程式碼

    }

    void Application_Error(object sender, EventArgs e)
    {
        // 應用程式啟動時執行的程式碼

    }

</script>
