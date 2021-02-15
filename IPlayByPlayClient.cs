using PlayByPlay.Api;
using System.Threading.Tasks;

namespace PlayByPlay
{
	public interface IPlayByPlayClient
	{
		Task<BoxScoreUrl[]> GetBoxScoreUrlsAsync(BoxScoreRequest request);
		Task<string[]> GetRedditAsync(PlayRequest request);
		Task<Team[]> GetTeamsAsync(int year);
	}
}