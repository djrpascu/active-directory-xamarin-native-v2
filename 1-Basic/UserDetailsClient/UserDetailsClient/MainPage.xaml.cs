using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace UserDetailsClient
{
    public partial class MainPage : ContentPage
    {
        public string Token { get; set; }

        public MainPage()
        {
            InitializeComponent();
        }

        async void OnSignInSignOut(object sender, EventArgs e)
        {
            AuthenticationResult authResult = null;
            IEnumerable<IAccount> accounts = await App.PCA.GetAccountsAsync().ConfigureAwait(false);
            try
            {
                if (btnSignInSignOut.Text == "Sign in")
                {
                    try
                    {
                        IAccount firstAccount = accounts.FirstOrDefault();
                        authResult = await App.PCA.AcquireTokenSilent(App.Scopes, firstAccount)
                                              .ExecuteAsync()
                                              .ConfigureAwait(false);
                    }
                    catch (MsalUiRequiredException)
                    {
                        try
                        {
                            var builder = App.PCA.AcquireTokenInteractive(App.Scopes)
                                .WithAuthority($"https://login.microsoftonline.com/{App.TenantID}")
                                .WithParentActivityOrWindow(App.ParentWindow);

                            if (Device.RuntimePlatform != "UWP")
                            {
                                // on Android and iOS, prefer to use the system browser, which does not exist on UWP
                                SystemWebViewOptions systemWebViewOptions = new SystemWebViewOptions()
                                {                            
                                    iOSHidePrivacyPrompt = true,
                                };

                                builder.WithSystemWebViewOptions(systemWebViewOptions);
                                builder.WithUseEmbeddedWebView(false);
                            }

                            authResult = await builder.ExecuteAsync().ConfigureAwait(false);
                        }
                        catch (Exception ex2)
                        {
                            await DisplayAlert("Acquire token interactive failed. See exception message for details: ", ex2.Message, "Dismiss");
                        }
                    }

                    if (authResult != null)
                    {
                        Token = authResult.AccessToken;

                        //var content = await GetHttpContentWithTokenAsync(authResult.AccessToken);
                        //UpdateUserContent(content);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            slUser.IsVisible = true;
                            btnSignInSignOut.Text = "Sign out";
                        });
                    }
                }
                else
                {
                    while (accounts.Any())
                    {
                        await App.PCA.RemoveAsync(accounts.FirstOrDefault()).ConfigureAwait(false);
                        accounts = await App.PCA.GetAccountsAsync().ConfigureAwait(false);
                    }

                    
                    Device.BeginInvokeOnMainThread(() => 
                    {
                        slUser.IsVisible = false;
                        btnSignInSignOut.Text = "Sign in"; 
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Authentication failed. See exception message for details: ", ex.Message, "Dismiss");
            }
        }

        private void UpdateUserContent(string content)
        {
            if(!string.IsNullOrEmpty(content))
            {
                JObject user = JObject.Parse(content);

                Device.BeginInvokeOnMainThread(() =>
                {
                    slUser.IsVisible = true;

                    lblDisplayName.Text = user["displayName"].ToString();
                    lblGivenName.Text = user["givenName"].ToString();
                    lblId.Text = user["id"].ToString();
                    lblSurname.Text = user["surname"].ToString();
                    lblUserPrincipalName.Text = user["userPrincipalName"].ToString();

                    btnSignInSignOut.Text = "Sign out";
                });
            }
        }

        public async Task<string> GetHttpContentWithTokenAsync(string token)
        {
            try
            {
                //get data from API
                HttpClient client = new HttpClient();
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(message).ConfigureAwait(false);
                string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return responseString;
            }
            catch(Exception ex)
            {
                await DisplayAlert("API call to graph failed: ", ex.Message, "Dismiss").ConfigureAwait(false);
                return ex.ToString();
            }
        }

        private async void CallApi_Clicked(object sender, EventArgs e)
        {
            try
            {
                //get data from API
                HttpClient client = new HttpClient();
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, $"{App.MeterRequestEndpoint}/values");
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = await client.SendAsync(message).ConfigureAwait(false);
                string responseString1 = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                HttpRequestMessage message2 = new HttpRequestMessage(HttpMethod.Get, $"{App.MeterRequestEndpoint}/MeterMasterRequests");
                message2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response2 = await client.SendAsync(message2).ConfigureAwait(false);
                string responseString2 = await response2.Content.ReadAsStringAsync().ConfigureAwait(false);

                Device.BeginInvokeOnMainThread(() =>
                {
                    Endpoint.Text = $"{App.MeterRequestEndpoint}/values";
                    Endpoint2.Text = $"{App.MeterRequestEndpoint}/MeterMasterRequests";
                    ApiResults.Text = responseString1;
                    ApiResults2.Text = responseString2;
                });
            }
            catch (Exception ex)
            {
                //await DisplayAlert("API call failed: ", ex.Message, "Dismiss").ConfigureAwait(false);
            }
        }
    }
}
