using CamCorder.Business.Services;
using CamCorder.Common.Enums;
using CamCorder.Data;
using RestSharp;
using System.Text.Json.Serialization;

namespace CamCorder.Infrastructure.Services
{
    public class ChaturbateHttpClient(RestClient client, CamCorderContext context) : ICamSite
    {
        private readonly RestClient _client = client;
        private readonly CamCorderContext _context = context;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private class ChaturbateHlsResponse
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("room_status")]
            public string? RoomStatus { get; set; }

            [JsonPropertyName("success")]
            public bool Success { get; set; }
        }

        public async Task<PageInfo> GetPageInfoAsync(int performerId, CancellationToken cancellationToken = default)
        {
            var performer = await _context.Performers.FindAsync([performerId, cancellationToken], cancellationToken: cancellationToken);
            if (performer is null)
                return new PageInfo { StreamUrl = null, Status = RoomStatus.Unknown };

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var request = new RestRequest("get_edge_hls_url_ajax/", Method.Post)
                    .AddHeader("X-Requested-With", "XMLHttpRequest")
                    .AddParameter("room_slug", performer.Name, ParameterType.GetOrPost);

                var response = await _client.ExecuteAsync<ChaturbateHlsResponse>(request, cancellationToken);

                if (response.IsSuccessful && response.Data is not null && response.Data.Success)
                {
                    var status = response.Data.RoomStatus switch
                    {
                        "public" => RoomStatus.Online,
                        "private" => RoomStatus.Private,
                        "offline" => RoomStatus.Offline,
                        _ => RoomStatus.Unknown
                    };

                    return new PageInfo
                    {
                        StreamUrl = response.Data.Url,
                        Status = status
                    };
                }

                return new PageInfo { StreamUrl = null, Status = RoomStatus.Offline };
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
