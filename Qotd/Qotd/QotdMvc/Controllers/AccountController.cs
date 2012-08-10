using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId;
using System.Web.Security;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using Qotd.Entities;
using System.Configuration;
using DotNetOpenAuth.OAuth2;
using QotdMvc.OAuth;
using System.Net;
using System.Diagnostics;

namespace QotdMvc.Controllers
{
    public class AccountController : BaseController
    {
        private static readonly string FB_PIC_URL = @"http://graph.facebook.com/{0}/picture";

        private static readonly FacebookClient client = new FacebookClient
        {
            ClientIdentifier = ConfigurationManager.AppSettings["facebookAppID"],
            ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["facebookAppSecret"]),
        };

        //
        // GET: /Account/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Signout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult SigninAs(Guid userId)
        {
            User user = DataProvider.GetUserById(userId).User;
            FormsAuthentication.RedirectFromLoginPage(user.Username, true);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult FacebookSignin()
        {
            IAuthorizationState authorization = client.ProcessUserAuthorization();
            if (authorization == null)
            {
                // Kick off authorization request
                client.RequestUserAuthorization();
            }
            else
            {
                var request = WebRequest.Create("https://graph.facebook.com/me?access_token=" + Uri.EscapeDataString(authorization.AccessToken));
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        var graph = FacebookGraph.Deserialize(responseStream);

                        User user = DataProvider.GetUserByFacebookId(graph.Id.ToString());

                        if (user == null)
                        {
                            user = new User()
                            {
                                DisplayName = graph.Name,
                                FacebookId = graph.Id.ToString(),
                                Username = "_FB" + graph.Id,
                                ProfileImageUrl = String.Format(FB_PIC_URL, graph.Id),
                                JoinedOn = Qotd.Utils.Config.Now
                            };
                            QotdService.SaveNewUser(user);
                        }

                        FormsAuthentication.RedirectFromLoginPage(user.Username, true);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult GoogleSignin()
        {
            string url = @"https://www.google.com/accounts/o8/id";

            var openid = new OpenIdRelyingParty();
            var response = openid.GetResponse();

            if (response == null)
            {
                Identifier id;
                if (Identifier.TryParse(url, out id))
                {
                    var req = openid.CreateRequest(id);
                    var fr = new FetchRequest();
                    fr.Attributes.Add(new AttributeRequest()
                    {
                        TypeUri = @"http://axschema.org/namePerson/first",
                        IsRequired = true
                    });
                    fr.Attributes.Add(new AttributeRequest()
                    {
                        TypeUri = @"http://axschema.org/namePerson/last",
                        IsRequired = true
                    });
                    req.AddExtension(fr);
                    req.AddExtension(new ClaimsRequest()
                        {
                            Email = DemandLevel.Require,
                            PolicyUrl = new Uri(@"http://openid.net/srv/ax/1.0")
                        });
                    req.RedirectToProvider();
                }
                else
                {
                    ViewBag.Message = "Invalid Identifier";
                    return View();
                }
            }
            else
            {
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        var claims = response.GetUntrustedExtension<ClaimsResponse>();
                        var att = response.GetUntrustedExtension<FetchResponse>();
                        var fname = att.Attributes[@"http://axschema.org/namePerson/first"].Values[0];
                        var lname = att.Attributes[@"http://axschema.org/namePerson/last"].Values[0];

                        // check in DB to see if 
                        var user = DataProvider.GetUserByEmail(claims.Email);   // TODO - validate EMAIL
                        if (user == null)
                        {
                            user = new User()
                            {
                                DisplayName = fname + " " + lname,
                                Email = claims.Email,
                                Username = response.ClaimedIdentifier
                            };
                            QotdService.SaveNewUser(user);
                        }
                        else
                        {
                            user.Username = response.ClaimedIdentifier;
                            DataProvider.MarkAddedOrUpdated(user);
                            DataProvider.SaveChanges();
                        }

                        FormsAuthentication.RedirectFromLoginPage(response.ClaimedIdentifier, true);
                        break;
                    case AuthenticationStatus.Canceled:
                        ViewBag.Message = "Canceled at provider";
                        return View();
                    case AuthenticationStatus.Failed:
                        ViewBag.Message = response.Exception.Message;
                        return View();
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
