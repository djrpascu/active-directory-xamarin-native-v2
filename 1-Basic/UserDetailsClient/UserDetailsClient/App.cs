using Microsoft.Identity.Client;
using System;
using Xamarin.Forms;

namespace UserDetailsClient
{
    public class App : Application
    {
        public static IPublicClientApplication PCA = null;

        /// <summary>
        /// The ClientID is the Application ID found in the portal (https://go.microsoft.com/fwlink/?linkid=2083908). 
        /// You can use the below id however if you create an app of your own you should replace the value here.
        /// </summary>
        public static string ClientID = "a0e1b51f-2ef9-4cff-b0c1-22d7e8c99209"; //msidentity-samples-testing tenant
        public static string TenantID = "da610562-359f-4ebe-9eb2-5b307090e8d2";
        public static string MeterRequestEndpoint = "https://testmeterrequest-hawaiianelectric.msappproxy.net/MeterRequestAPI/api";

        //public static string[] Scopes = { "https://testmeterrequest-hawaiianelectric.msappproxy.net/MeterRequestAPI/api//user_impersonation" };
        public static string[] Scopes = { "User.Read", "https://testmeterrequest-hawaiianelectric.msappproxy.net/MeterRequestAPI/api//user_impersonation" };
        public static string Username = string.Empty;

        public static object ParentWindow { get; set; }

        public App(string specialRedirectUri = null)
        {
            PCA = PublicClientApplicationBuilder.Create(ClientID)
                .WithRedirectUri(specialRedirectUri?? $"msal{ClientID}://auth")
                .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                .Build();

            MainPage = new NavigationPage(new UserDetailsClient.MainPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
