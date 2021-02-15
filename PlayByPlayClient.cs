using Microsoft.AspNetCore.WebUtilities;
using PlayByPlay.Api;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlayByPlay
{
	internal class PlayByPlayClient : IPlayByPlayClient
	{
		private readonly HttpClient _client;
		private readonly JsonSerializerOptions _serializerOptions;

		public PlayByPlayClient(HttpClient client)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_serializerOptions = new JsonSerializerOptions
			{
				DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase				
			};
		}

		public async Task<Team[]> GetTeamsAsync(int year)
			=> await GetJsonAsync<Team[]>("/api/teams", new Dictionary<string, string>
			{
				[nameof(year)] = year.ToString()
			});

		public async Task<BoxScoreUrl[]> GetBoxScoreUrlsAsync(BoxScoreRequest request)
		{
			if (request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			return await GetJsonAsync<BoxScoreUrl[]>("/api/boxscoreurls", new Dictionary<string, string>
			{
				[nameof(BoxScoreRequest.Year)] = request.Year.ToString(),
				[$"{nameof(BoxScoreRequest.Filter)}.{nameof(BoxScoreFilter.Team)}"] = request.Filter?.Team ?? string.Empty,
				[$"{nameof(BoxScoreRequest.Filter)}.{nameof(BoxScoreFilter.Away)}"] = request.Filter?.Away ?? string.Empty,
				[$"{nameof(BoxScoreRequest.Filter)}.{nameof(BoxScoreFilter.Home)}"] = request.Filter?.Home ?? string.Empty,
				[$"{nameof(BoxScoreRequest.Filter)}.{nameof(BoxScoreFilter.Week)}"] = request.Filter.Week.ToString()
			});
		}

		public async Task<string[]> GetRedditAsync(PlayRequest request)
		{
			if (request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			return await GetJsonAsync<string[]>("/api/reddit", new Dictionary<string, string>
			{
				[nameof(PlayRequest.Season)] = request.Season.ToString(),
				[nameof(PlayRequest.Week)] = request.Week.ToString(),
				[nameof(PlayRequest.GameId)] = request.GameId,
				[nameof(PlayRequest.HomeCode)] = request.HomeCode,
				[nameof(PlayRequest.AwayCode)] = request.AwayCode
			});
		}

		private async Task<T> GetJsonAsync<T>(string path, IDictionary<string, string> parameters)
		{
			string requestUrl = QueryHelpers.AddQueryString(path, parameters);

			var response = await _client.GetAsync(requestUrl).ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			//var text = await response.Content.ReadAsStringAsync();

			var stream = await response.Content.ReadAsStreamAsync();

			return await JsonSerializer.DeserializeAsync<T>(stream, _serializerOptions).ConfigureAwait(false);
		}
	}
}