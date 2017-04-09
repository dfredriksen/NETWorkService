using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;

namespace CYINT.NETWorkServices
{
    public class NetworkService
    {
        protected HttpClient _httpClient;
        protected HttpClientHandler _handler;
        protected HttpResponseMessage _response;
        protected Dictionary<string, string> _cookies;
        protected Dictionary<string, string> _headers;
        protected bool _followRedirects;
        protected int _maxResponseContentBufferSize;
        public string _baseUrl;

        public NetworkService(string baseUrl)
        {
            _cookies = new Dictionary<string, string>();
            _headers = new Dictionary<string, string>();
            _maxResponseContentBufferSize = 256000;
            _followRedirects = true;
            SetBaseUrl(baseUrl);
        }


        public Dictionary<string, string> GetCookies()
        {
            return _cookies;
        }

        public void AddCookie(string name, string value)
        {
            _cookies.Add(name, value);
            InitializeClient();
        }

        public string GetCookie(string name)
        {
            if (_cookies.ContainsKey(name))
                return _cookies[name];

            throw new Exception("Cookie does not exist");
        }

        public bool CookieExists(string name)
        {
            if (_cookies.ContainsKey(name))
                return true;

            return false;
        }

        public bool HeaderExists(string name)
        {
            if (_headers.ContainsKey(name))
                return true;

            return false;
        }

        public void RemoveCookie(string name)
        {
            if (_cookies.ContainsKey(name))
            {
                _cookies.Remove(name);
                InitializeClient();
            }
            else
                throw new Exception("Cookie does not exist");

        }

        public Dictionary<string, string> GetHeaders()
        {
            return _headers;
        }

        public void SetHeaders(Dictionary<string, string> headers)
        {
            _headers = headers;
            InitializeClient();
        }

        public void AddHeader(string name, string value)
        {
            _headers.Add(name, value);
            InitializeClient();
        }

        public void RemoveHeader(string name)
        {
            if (_headers.ContainsKey(name))
            {
                _headers.Remove(name);
                InitializeClient();
            }
            else
                throw new Exception("Header does not exist.");
        }

        public string GetHeader(string name)
        {
            if (_headers.ContainsKey(name))
                return _headers[name];

            throw new Exception("Header does not exist.");
        }

        public void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public string GetBaseUrl()
        {
            return _baseUrl;
        }

        public async Task<string> FetchData(
             string path
             , bool isPost = false
             , string postData = null
             , Encoding encoding = null
             , string mediaType = "application/x-www-form-urlencoded"
         )
        {
            Uri uri;
            HttpResponseMessage response;
            string content = null;

            response = null;

            if (IsNetworkAvailable())
            {

                encoding = encoding ?? Encoding.UTF8;
                uri = new Uri(string.Format(GetBaseUrl() + path, string.Empty));

                if (isPost)
                {
                    StringContent payload = new StringContent(postData, encoding, mediaType);
                    response = await GetHttpClient().PostAsync(uri, payload);
                }
                else
                {
                    response = await GetHttpClient().GetAsync(uri);
                }

                SetResponse(response);

                if (response.IsSuccessStatusCode || ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399))
                    content = await response.Content.ReadAsStringAsync();
            }
            else
                throw new Exception("Network unavailable.");

            return content;
        }


        public void SetResponse(HttpResponseMessage response)
        {
            _response = response;
        }

        public HttpClient GetHttpClient()
        {
            if (_httpClient == null)
                InitializeClient();

            return _httpClient;
        }


        public bool IsNetworkAvailable()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        public void InitializeClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            CookieContainer cookieJar = new CookieContainer();

            if (GetCookies().Count > 0)
            {
                foreach (KeyValuePair<string, string> cookie in GetCookies())
                {
                    cookieJar.Add(new Uri(GetBaseUrl()), new Cookie(cookie.Key, cookie.Value));
                }
            }

            handler.CookieContainer = cookieJar;
            _handler = handler;
            _httpClient = new HttpClient(_handler);

            _httpClient.MaxResponseContentBufferSize = _maxResponseContentBufferSize;

            if (GetHeaders().Count > 0)
            {
                foreach (KeyValuePair<string, string> header in GetHeaders())
                {
                    if (!_httpClient.DefaultRequestHeaders.Contains(header.Key))
                        _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }
    }
}
