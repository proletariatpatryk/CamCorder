using CamCorder.Common.Enums;
using CamCorder.Data;
using RestSharp;
using System.Text.Json.Serialization;

namespace CamCorder.Business.Services
{
    public class PageInfo
    {
        public string? StreamUrl { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Unknown;
    }

    public class ChaturbateService(CamCorderContext context) : ICamSite
    {
        private readonly RestClient _client = new(config =>
            {
                config.BaseUrl = new Uri("https://chaturbate.com");
            });
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

            [JsonPropertyName("hidden_message")]
            public string? HiddenMessage { get; set; }

            [JsonPropertyName("cmaf_edge")]
            public bool CmafEdge { get; set; }
        }

        public async Task<PageInfo> GetPageInfoAsync(int performerId, CancellationToken cancellationToken = default)
        {
            var performer = await _context.Performers.FindAsync([performerId], cancellationToken);
            if (performer is null)
            {
                return new PageInfo
                {
                    StreamUrl = null,
                    Status = RoomStatus.Unknown
                };
            }

            var hlsData = await GetEdgeHlsUrlDataAsync(performer.Name, cancellationToken);
            if (hlsData is null || !hlsData.Success)
            {
                return new PageInfo
                {
                    StreamUrl = null,
                    Status = RoomStatus.Offline
                };
            }
            var status = hlsData.RoomStatus switch
            {
                "public" => RoomStatus.Online,
                "private" => RoomStatus.Private,
                "offline" => RoomStatus.Offline,
                _ => RoomStatus.Unknown
            };
            return new PageInfo
            {
                StreamUrl = hlsData.Url,
                Status = status
            };
        }

        private async Task<ChaturbateHlsResponse?> GetEdgeHlsUrlDataAsync(string performerName, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(performerName);

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var request = new RestRequest("get_edge_hls_url_ajax/", Method.Post)
                    .AddHeader("X-Requested-With", "XMLHttpRequest")
                    .AddParameter("room_slug", performerName, ParameterType.GetOrPost);

                var response = await _client.ExecuteAsync<ChaturbateHlsResponse>(request, cancellationToken);

                if (response.IsSuccessful && response.Data is not null)
                {
                    return response.Data;
                }

                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}

