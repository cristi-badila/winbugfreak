﻿using System;
using System.IO;
using System.Net;

namespace BugFreak.Components
{
    public class ReportRequestBuilder : IReportRequestBuilder
    {
        public const string ApiKey = "Api-Key";
        public const string TokenKey = "Token";
        public const string HttpMethod = "POST";

        private readonly IWebRequestCreate _webRequestFactory;

        public event EventHandler<ReportRequestBuildCompletedEventArgs> BuildCompleted;

        public ReportRequestBuilder()
        {
            _webRequestFactory = GlobalConfig.ServiceLocator.GetService<IWebRequestCreate>();
        }

        public void BuildAsync(ErrorReport report)
        {
            var request = CreateRequest();
            SetMethod(request);
            Sign(request);
            WriteAsync(report, request);
        }

        private WebRequest CreateRequest()
        {
            return _webRequestFactory.Create(new Uri(GlobalConfig.ServiceEndPoint));
        }

        private void SetMethod(WebRequest request)
        {
            request.Method = HttpMethod;
        }

        private void Sign(WebRequest request)
        {
            request.Headers[ApiKey] = GlobalConfig.ApiKey;
            request.Headers[TokenKey] = GlobalConfig.Token;
        }

        private void WriteAsync(ErrorReport report, WebRequest request)
        {
            var serializer = GlobalConfig.ServiceLocator.GetService<IErrorReportSerializer>();

            request.ContentType = serializer.GetContentType();
            var serializedObj = serializer.Serialize(report);

            request.BeginGetRequestStream(asyncResult => HandleWrite(asyncResult, serializedObj), request);
        }

        private void HandleWrite(IAsyncResult result, string serializedError)
        {
            ReportRequestBuildCompletedEventArgs eventArgs;
            var request = (WebRequest)result.AsyncState;
            try
            {
                var stream = request.EndGetRequestStream(result);
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(serializedError);
                    writer.Flush();
                }

                eventArgs = new ReportRequestBuildCompletedEventArgs { Result = request };
            }
            catch (Exception exception)
            {
                eventArgs = new ReportRequestBuildCompletedEventArgs { Error = exception };
            }

            OnBuildCompleted(eventArgs);
        }

        protected virtual void OnBuildCompleted(ReportRequestBuildCompletedEventArgs e)
        {
            var handler = BuildCompleted;
            if (handler != null) handler(this, e);
        }
    }
}