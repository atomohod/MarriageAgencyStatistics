using System;
using System.Threading.Tasks;

namespace MarriageAgencyStatistics.Common
{
    public interface IClient
    {
        Task<bool> Reconnect();
        Task<T> Get<T>(string url, Func<string, T> parser);
        Task<T> Post<T, TPayload>(string url, TPayload content, Func<string, T> parser);
    }
}
