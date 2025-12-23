using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;

namespace Phrase
{
    [Serializable]
    struct PhraseAuthTokenResponse
    {
        public string access_token;
        public string token_type;
        public string expires_in;
        public string scope;
        public string refresh_token;
    }

    [Serializable]
    struct PhraseOrganization {
        public string uid;
    }

    [Serializable]
    struct PhraseUserResponse
    {
        public string uid;
        public string email;
        public string username;
        public PhraseOrganization lastOrganization;
    }

    [Serializable]
    struct PhraseAppTokenResponse
    {
        public string accessToken;
    }

    class PhraseOauthAuthenticator
    {
        private static PhraseProvider provider;

        private static readonly HttpListener listener = new HttpListener();

        private static readonly string listenUrl = "http://localhost:8000/oauth/";

        private static readonly int OauthTimeout = 5*60*1000; // 5 minutes
        private static readonly string redirectUrl = $"{listenUrl}/callback";
        private static readonly string pageSuccess =
@"<html>
  <head>
    <title>Authorization successful</title>
  </head>
  <body>
    <p>You may close this page now</p>
  </body>
</html>";

        private static readonly string clientId = "strings-unity";

        private static string BaseUrl
        {
            get {
                switch (provider.m_Environment)
                {
                    case "EU":
                        return "https://eu.phrase.com";
                    case "US":
                        return "https://us.phrase.com";
                    default:
                        return "https://eu.phrase-qa.com";
                };
            }
        }

        private static string codeVerifier = "";

        private static readonly string challengeMethod = "S256";

        private static void HandleAuthorizationCodeFlow()
        {
            codeVerifier = GenerateRandomString(64);
            var codeChallenge = GenerateCodeChallenge(codeVerifier);

            Application.OpenURL($"{BaseUrl}/idm/openid/authorize?response_type=code&scope=openid%20sso%20offline_access&client_id={clientId}&code_challenge_method={challengeMethod}&code_challenge={codeChallenge}&redirect_uri={redirectUrl}&login_hint=__idm__");
        }

        private static async Task<PhraseAuthTokenResponse> GetAccessToken(string code)
        {
            provider.Log($"Getting access token from code {code}");
            var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("code_verifier", codeVerifier),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", redirectUrl),
                new KeyValuePair<string, string>("code", code)
            });

            var response = await httpClient.PostAsync($"{BaseUrl}/idm/oauth/token", content);
            var jsonResponse =  await response.Content.ReadAsStringAsync();
            provider.Log($"Response: {jsonResponse}");
            return JsonUtility.FromJson<PhraseAuthTokenResponse>(jsonResponse);
        }

        private static async Task<PhraseAuthTokenResponse> RefreshAuthToken()
        {
            provider.Log($"Refreshing token from refresh token {provider.m_OauthRefreshToken}");
            var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", provider.m_OauthRefreshToken)
            });

            var response = await httpClient.PostAsync($"{BaseUrl}/idm/oauth/token", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            provider.Log($"Response: {jsonResponse}");
            return JsonUtility.FromJson<PhraseAuthTokenResponse>(jsonResponse);
        }

        private static async Task<PhraseUserResponse> GetUser(string token)
        {
            provider.Log($"Getting user from token {token}");
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await httpClient.GetAsync($"{BaseUrl}/idm/api/v1/user");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            provider.Log($"Response: {jsonResponse}");
            return JsonUtility.FromJson<PhraseUserResponse>(jsonResponse);
        }

        private static async Task<PhraseAppTokenResponse> GetAppToken(string token, string organizationId)
        {
            provider.Log($"Getting app token from token {token} and organization {organizationId}");
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var body = new StringContent("{\"applicationUid\":\"strings\"}", Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{BaseUrl}/idm/api/v1/user/organizations/{organizationId}/token/grant", body);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            provider.Log($"Response: {jsonResponse}");
            return JsonUtility.FromJson<PhraseAppTokenResponse>(jsonResponse);
        }

        private static string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Convert.ToBase64String(bytes)
                    .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
        }

        private static string GenerateRandomString(int length)
        {
            var text = new StringBuilder();
            const string possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            var random = new System.Random();
            for (var i = 0; i < length; i++)
            {
                text.Append(possible[random.Next(possible.Length)]);
            }

            return text.ToString();
        }

        private static async Task HandleIncomingConnections()
        {
            bool runServer = true;
            try {

                // While a user hasn't successfully authenticated, keep on handling requests
                while (runServer)
                {
                    provider.Log("Waiting for a request...");
                    // Will wait here until we hear from a connection
                    HttpListenerContext ctx = await listener.GetContextAsync();
                    HttpListenerRequest req = ctx.Request;

                    // Print out some info about the request
                    provider.Log($"Request: {req.HttpMethod} {req.Url.ToString()}");

                    string code = Regex.Match(req.Url.Query, "code=([^&]*)").Groups[1].Value;
                    if (code != "")
                    {
                        var authTokenResponse = await GetAccessToken(code);
                        provider.m_OauthRefreshToken = authTokenResponse.refresh_token;
                        var userResponse = await GetUser(authTokenResponse.access_token);
                        provider.m_OauthOrganizationId = userResponse.lastOrganization.uid;
                        var appTokenResponse = await GetAppToken(authTokenResponse.access_token, provider.m_OauthOrganizationId);
                        provider.SetOauthToken(appTokenResponse.accessToken);
                        EditorUtility.SetDirty(provider);
                        SendPageContent(ctx.Response, pageSuccess);
                        runServer = false;
                        provider.Log("Closing server...");
                        StopServer(null);
                    } else {
                        SendPageContent(ctx.Response, "Error: No code found in the request");
                    }
                }
            } catch (ObjectDisposedException e) {
                // This is expected when the server is stopped
                provider.Log($"Error: {e.Message}");
            }
        }

        public static async Task<bool> RefreshToken(PhraseProvider provider)
        {
            PhraseOauthAuthenticator.provider = provider;
            if (provider.m_OauthRefreshToken == null || provider.m_OauthOrganizationId == null)
            {
                provider.Log("No refresh token or organization id found");
                return false;
            }
            var authTokenResponse = await RefreshAuthToken();
            provider.m_OauthRefreshToken = authTokenResponse.refresh_token;
            var appTokenResponse = await GetAppToken(authTokenResponse.access_token, provider.m_OauthOrganizationId);
            provider.Log($"Token: {appTokenResponse.accessToken}");
            provider.SetOauthToken(appTokenResponse.accessToken);
            EditorUtility.SetDirty(provider);
            return true;
        }

        private static void SendPageContent(HttpListenerResponse response, string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        private static async void StartServer()
        {
            listener.Prefixes.Add(listenUrl);
            listener.Start();
            provider.Log($"Listening for connections on {listenUrl}");

            // Handle requests
            await HandleIncomingConnections();
        }

        private static Timer stopperTimer;

        private static void StopServer(object state)
        {
            provider.Log("Stopping server...");
            if (listener.IsListening)
            {
                listener.Stop();
            }
            if (stopperTimer != null)
            {
                stopperTimer.Dispose();
            }
            provider.m_OauthInProgress = false;
        }

        public static void Authenticate(PhraseProvider provider) {
            PhraseOauthAuthenticator.provider = provider;
            provider.m_OauthInProgress = true;
            StartServer();
            HandleAuthorizationCodeFlow();
            stopperTimer = new Timer(StopServer, null, OauthTimeout, Timeout.Infinite);
        }
    }
}
