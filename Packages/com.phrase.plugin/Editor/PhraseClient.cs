using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Phrase
{
    public class RetryHandler : DelegatingHandler
    {
        private const int MaxRetries = 1;

        private readonly PhraseProvider Provider;

        public RetryHandler(HttpMessageHandler innerHandler, PhraseProvider provider) : base(innerHandler)
        {
            Provider = provider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            request.Headers.Add("Authorization", "Bearer " + Provider.Token);
            for (int i = 0; i < MaxRetries; i++)
            {
                Provider.Log("Sending request to " + request.RequestUri);
                response = base.SendAsync(request, cancellationToken).Result;
                if (response.IsSuccessStatusCode) {
                    return response;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Provider.Log("Unauthorized, refreshing token");
                    if (await Provider.RefreshToken()) {
                        request.Headers.Remove("Authorization");
                        request.Headers.Add("Authorization", "Bearer " + Provider.Token);
                    } else {
                        Provider.Log("Request unauthorized, failed to refresh token");
                        return response;
                    }
                }
                else
                {
                    Provider.Log("Request failed with status code " + response.StatusCode);
                    return response;
                }
            }

            return response;
        }
    }

    public class PhraseClient
    {
        private readonly string Version = "1.2.2";

        private readonly PhraseProvider Provider;

        private string ApiUrl => Provider.m_ApiUrl;

        private HttpClient Client;

        [Serializable]
        public class Locale
        {
            public string id;
            public string name;
            public string code;

            public override string ToString()
            {
                return name == code ? name : $"{name} ({code})";
            }
        }

        [Serializable]
        public struct Account
        {
            public string id;
            public string name;
        }

        [Serializable]
        public struct Project
        {
            public string id;
            public string name;
            public Account account;
        }

        [Serializable]
        public class Screenshot
        {
            public string id;
            public string screenshot_url;
        }

        public class ScreenshotMarker
        {
            public string id;
        }

        [Serializable]
        public class Key
        {
            public string id;
            public string name;
        }

        public PhraseClient(PhraseProvider provider)
        {
            this.Provider = provider;
            this.Client = new HttpClient(new RetryHandler(new HttpClientHandler(), provider));
            Client.DefaultRequestHeaders.Add("User-Agent", $"Unity Plugin/{Version}");
            Client.BaseAddress = new Uri(ApiUrl);
        }

        public async Task<string> DownloadLocale(string projectID, string localeID, string tag, string keyPrefix)
        {
            string url = string.Format("projects/{0}/locales/{1}/download?file_format=csv&format_options%5Bexport_max_characters_allowed%5D=true&format_options%5Bexport_key_id%5D=true&include_empty_translations=true", projectID, localeID);
            if (tag != null)
            {
                url += "&tags=" + WebUtility.UrlEncode(tag);
            }
            if (keyPrefix != null)
            {
                url += "&filter_by_prefix=true&translation_key_prefix=" + WebUtility.UrlEncode(keyPrefix);
            }
            HttpResponseMessage response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }


        public async void UpdateLocalesList(string projectID, List<Locale> locales)
        {
            string url = string.Format("projects/{0}/locales", projectID);
            HttpResponseMessage response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            locales.Clear();
            locales.AddRange(JsonConvert.DeserializeObject<List<Locale>>(jsonResponse));
        }

        public async void CreateLocale(string projectID, string localeCode, string localeName)
        {
            string url = string.Format("projects/{0}/locales", projectID);
            var content = new StringContent(JsonConvert.SerializeObject(new { code = localeCode, name = localeName }), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        public async void UpdateProjectsList(List<Project> projects)
        {
            projects.Clear();
            int page = 1;
            List<Project> currentBatch;

            do
            {
                HttpResponseMessage response = await Client.GetAsync($"projects?per_page=100&page={page}");
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();
                currentBatch = JsonConvert.DeserializeObject<List<Project>>(jsonResponse);
                projects.AddRange(currentBatch);

                page++;
            } while (currentBatch.Count == 100);  // Continue if a full batch is returned
        }

        public async void UploadFile(string path, string projectID, string localeID, string localeName, bool autoTranslate, string tag)
        {
            string url = string.Format("projects/{0}/uploads", projectID);
            NameValueCollection nvc = new NameValueCollection
            {
                { "locale_id", localeID },
                { "file_format", "csv" },
                { "update_descriptions", "true" },
                { "update_translations", "true" },
                { $"locale_mapping[{localeName}]", "4" },
                { "format_options[key_index]", "1" },
                { "format_options[comment_index]", "2" },
                { "format_options[max_characters_allowed_column]", "3" },
                { "format_options[header_content_row]", "true" }
            };
            if (autoTranslate)
            {
                nvc.Add("autotranslate", "true");
            }
            if (tag != null)
            {
                nvc.Add("tags", tag);
            }

            await HttpUploadFile(url, path, "file", "text/plain", nvc);
        }

        public async Task<Screenshot> UploadScreenshot(string projectID, string name, string path)
        {
            string url = string.Format("projects/{0}/screenshots", projectID);
            NameValueCollection nvc = new NameValueCollection
            {
                { "name", name },
                { "description", "Uploaded from Unity" }
            };
            string responseString = await HttpUploadFile(url, path, "filename", "image/png", nvc);
            return JsonConvert.DeserializeObject<Screenshot>(responseString);
        }

        public async Task<ScreenshotMarker> CreateScreenshotMarker(string projectID, string screenshotID, string keyID)
        {
            string url = string.Format("projects/{0}/screenshots/{1}/markers", projectID, screenshotID);
            var content = new StringContent(JsonConvert.SerializeObject(new { key_id = keyID }), Encoding.UTF8, "application/json");
            var response = Client.PostAsync(url, content).Result;
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<ScreenshotMarker>(await response.Content.ReadAsStringAsync());
        }

        public void DeleteScreenshotMarker(string projectID, string screenshotID, string markerID)
        {
            string url = string.Format("projects/{0}/screenshots/{1}/markers/{2}", projectID, screenshotID, markerID);
            Client.DeleteAsync(url);
        }

        public async Task<List<Key>> GetKeysByName(string projectID, List<string> keyNames)
        {
            var allKeys = new List<Key>();
            if (keyNames == null || keyNames.Count == 0) return allKeys;
            var escapedNames = keyNames.Select(n => Regex.Replace(n, @"[\\\"" ,]", m => "\\" + m.Value)).ToList();
            string q = $"name:{string.Join(",", escapedNames)}";

            int page = 0;
            const int perPage = 100; // Phrase API max per_page is 100

            while (true)
            {
                string url = $"projects/{projectID}/keys/search?page={page}&per_page={perPage}";
                var payload = new { q = q };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await Client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var batch = JsonConvert.DeserializeObject<List<Key>>(jsonResponse);

                if (batch == null || batch.Count == 0) break;

                allKeys.AddRange(batch);

                if (batch.Count < perPage) break; // no more pages
                page++;
            }

            return allKeys;
        }

        private async Task<string> HttpUploadFile(string url, string path, string paramName, string contentType, NameValueCollection nvc)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();
            var fileStream = new FileStream(path, FileMode.Open);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.Add("Content-Type", contentType);
            var fileName = Path.GetFileName(path);
            form.Add(streamContent, paramName, fileName);

            foreach (string key in nvc.Keys)
            {
                form.Add(new StringContent(nvc[key]), key);
            }

            var response = await Client.PostAsync(url, form);
            var responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            fileStream.Close();
            return Task.FromResult(responseContent).Result;
        }
    }
}