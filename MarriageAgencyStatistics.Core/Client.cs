﻿using System;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using Polly;
using Polly.Retry;
using RestSharp;

namespace MarriageAgencyStatistics.Core
{
    public abstract class Client : IClient
    {
        protected readonly RestClient _client;
        private readonly RetryPolicy _retryPolicy;

        protected Client(string intialPage)
        {
            _client = new RestClient(intialPage);

            _retryPolicy = Policy
                .Handle<ReloginRequiredException>()
                .WaitAndRetryAsync(10, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, span) => Reconnect());
        }

        public abstract Task<bool> Reconnect();

        public async Task<T> Get<T>(string url, Func<string, T> parser)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var result = await _client.ExecuteGetTaskAsync(new RestRequest(url, Method.GET));

                if (IsReloginRequired(result))
                    throw new ReloginRequiredException();

                return parser(result.Content);
            });
        }

        public async Task<T> Post<T, TPayload>(string url, TPayload content, Func<string, T> parser)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var restRequest = new RestRequest(url, Method.POST);
                restRequest.AddHeader("content-type", "application/x-www-form-urlencoded");
                restRequest.AddParameter("application/x-www-form-urlencoded", content.ToQueryString(), ParameterType.RequestBody);
                var result = await _client.ExecutePostTaskAsync(restRequest);

                IsReloginRequired(result);

                return parser(result.Content);
            });
        }

        protected abstract bool IsReloginRequired(IRestResponse response);
    }
}