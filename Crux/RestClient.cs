﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Crux
{
    /// <summary>
    /// All credits to: https://www.codeproject.com/tips/497123/how-to-make-rest-requests-with-csharp
    /// </summary>
    class RestClient
    {
        public enum HttpVerb
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        public string EndPoint { get; set; }
        public HttpVerb Method { get; set; }
        public string ContentType { get; set; }
        public string PostData { get; set; }

        public RestClient()
        {
            EndPoint = "";
            Method = HttpVerb.GET;
            ContentType = "text/xml";
            PostData = "";
        }
        public RestClient(string endpoint)
        {
            EndPoint = endpoint;
            Method = HttpVerb.GET;
            ContentType = "text/xml";
            PostData = "";
        }
        public RestClient(string endpoint, HttpVerb method)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = "text/xml";
            PostData = "";
        }

        public RestClient(string endpoint, HttpVerb method, string postData)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = "text/xml";
            PostData = postData;
        }


        public string MakeRequest()
        {
            return MakeRequest("");
        }

        public string MakeRequest(string parameters, string[] headers = null, bool retry = true, int retryRemaining = 10)
        {
            var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);

            request.Method = Method.ToString();
            request.ContentLength = 0;
            request.ContentType = ContentType;

            if (!string.IsNullOrEmpty(PostData) && Method == HttpVerb.POST)
            {
                var encoding = new UTF8Encoding();
                var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
                request.ContentLength = bytes.Length;

                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    request.Headers.Add(h);
                }
            }
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var responseValue = string.Empty;

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                        throw new ApplicationException(message);
                    }

                    // grab the response
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                            using (var reader = new StreamReader(responseStream))
                            {
                                responseValue = reader.ReadToEnd();
                            }
                    }

                    return responseValue;
                }
            }
            catch (WebException e) when (retry && retryRemaining > 0)
            {
                Log.Write("Web exception", 0);
                Log.Write($"Status code: {(e.Response as HttpWebResponse).StatusCode}", 0);
                Log.Write($"Status description: {(e.Response as HttpWebResponse).StatusDescription}", 0);
                Thread.Sleep(2000);
                return MakeRequest(parameters, headers, retry, retryRemaining - 1);
            }
            catch (Exception e) when (retry && retryRemaining > 0)
            {
                Log.Write(e.Message, 0);
                Log.Write(e.StackTrace, 0);
                Thread.Sleep(2000);
                return MakeRequest(parameters, headers, retry, retryRemaining - 1);
            }
        }
    }
}
