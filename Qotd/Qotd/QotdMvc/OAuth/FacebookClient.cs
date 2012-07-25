using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OAuth2;

namespace QotdMvc.OAuth
{
    public class FacebookClient : WebServerClient
    {
        private static readonly AuthorizationServerDescription FacebookDescription = new AuthorizationServerDescription()
        {
            TokenEndpoint = new Uri("https://graph.facebook.com/oauth/access_token"),
            AuthorizationEndpoint = new Uri("https://graph.facebook.com/oauth/authorize")
        };

        public FacebookClient()
            : base(FacebookDescription)
        {
        }
    }
}