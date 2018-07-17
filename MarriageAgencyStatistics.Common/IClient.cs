using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageAgencyStatistics.Common
{
    public interface IClient
    {
        Task<bool> Reconnect();
        Task<T> GetAsync<T>(string url, Func<string, Task<T>> parser);
        Task<T> PostAsync<T, TPayload>(string url, TPayload content, Func<string, Task<T>> parser);

        Task<T> Get<T>(string url, Func<string, T> parser);
        Task<T> Post<T, TPayload>(string url, TPayload content, Func<string, T> parser);
    }
}
