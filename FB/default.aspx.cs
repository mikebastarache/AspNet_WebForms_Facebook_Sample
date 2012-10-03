using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facebook;
using System.Text;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO; // JSON.NET project 

namespace FB
{
    public partial class _default : System.Web.UI.Page
    {
        private const string AppId = "374918325879397";
        private const string Appsecret = "54c997ce7eb78b85f906db62e758e368";
        private const string ExtendedPermissions = "user_about_me,read_stream";
        private const string redirect_uri = "http://fb.local/";
        private string _accessToken;

        protected void Page_Load(object sender, EventArgs e)
        {
            
            var client = new FacebookClient();
          
            dynamic parameters = new ExpandoObject();
            parameters.fields = "id,name";

            dynamic result = client.Get("mikebastarache");
            //dynamic result = client.Get("mikebastarache", parameters);

            Response.Write(Convert.ToString(result) + "<HR>");

            var id = result.id;
            var name = result.name;
            var firstName = result.first_name;
            var lastName = result.last_name;
            var link = result.link;
            var username = result.username;
            var gender = result.gender;
            var male = result.locale;

            Literal1.Text = "FB ID = " + result.id + "<BR>";
            Literal1.Text += "Name = " + result.name + "<BR>";
           
            string profilePictureUrl = string.Format("https://graph.facebook.com/{0}/picture", id);
            Image1.ImageUrl = profilePictureUrl ;
            
            if (Request.Params["signed_request"] != null)
            {
                string payload = Request.Params["signed_request"].Split('.')[1];
                var encoding = new UTF8Encoding();
                var decodedJson = payload.Replace("=", string.Empty).Replace('-', '+').Replace('_', '/');
                var base64JsonArray = Convert.FromBase64String(decodedJson.PadRight(decodedJson.Length + (4 - decodedJson.Length % 4) % 4, '='));
                var json = encoding.GetString(base64JsonArray);
                var o = JObject.Parse(json);
                
                var lPid = Convert.ToString(o.SelectToken("page.id")).Replace("\"", "");

                var oauth_token = Convert.ToString(o.SelectToken("oauth_token")).Replace("\"", "");
                var algorithm = Convert.ToString(o.SelectToken("algorithm")).Replace("\"", "");
               

                var lLiked = Convert.ToString(o.SelectToken("page.liked")).Replace("\"", "");
                var lUserId = Convert.ToString(o.SelectToken("user_id")).Replace("\"", "");
                Response.Write("PAGE ID = " + lPid + "<BR>");
                Response.Write("USER LIKES PAGE = " + lLiked + "<BR>");
                Response.Write("USER ID = " + lUserId + "<HR>");


                try
                {
                    var fb1 = new FacebookClient(oauth_token);
                    
                    dynamic parameters1 = new ExpandoObject();
                    parameters1.q = "SELECT name, uid FROM user WHERE uid = me()";
                    dynamic result11 = fb1.Get("fql", parameters1);
                    Response.Write("FQL RESULTS : " + result11 + "<HR>");
                }
                catch (FacebookApiException ex)
                {
                    Response.Write("FQL RESULTS : FACEBOOK EXCEPTION<br>" + ex + "<HR>");
                }
                catch (Exception ex)
                {
                    Response.Write("FQL RESULTS : None.  Need to authorize APP to see results<br>" + ex + "<HR>");
                }

                try
                {
                    //string FBcode = Request.QueryString["code"].ToString();
                    string FBcode = Request.Params["code"];
                    string FBAccessUrl = "https://graph.facebook.com/oauth/access_token?client_id=" + AppId + "&redirect_uri=" + redirect_uri + "&client_secret=" + Appsecret + "&code=" + FBcode;

                    string accessToken = null;

                    // Send the request to exchange the code for access_token
                    var accessTokenRequest = System.Net.HttpWebRequest.Create(FBAccessUrl);

                    HttpWebResponse response = (HttpWebResponse)accessTokenRequest.GetResponse();
                    // handle response from FB 
                    // this will not be a url with params like the first request to get the 'code'
                    Encoding rEncoding = Encoding.GetEncoding(response.CharacterSet);

                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), rEncoding))
                    {
                        // parse the response to get the value of the 'access_token'
                        accessToken = HttpUtility.ParseQueryString(sr.ReadToEnd()).Get("access_token");
                        Response.Write(accessToken + "<HR>");
                    }
                }
                catch (Exception ex)
                {
                    Response.Write("graph login<br>" + ex + "<HR>");
                }

                Response.Write(Convert.ToString(o) + "<HR><br><br>");


                Response.Write(Request.Params + "<HR>");
            }


            
        }

        public class FacebookLogin : IHttpHandler
        {

            public void ProcessRequest(HttpContext context)
            {
                var accessToken = context.Request["accessToken"];
                context.Session["AccessToken"] = accessToken;

                context.Response.Redirect("/MyUrlHere");
            }

            public bool IsReusable
            {
                get { return false; }
            }
        }
    }


}