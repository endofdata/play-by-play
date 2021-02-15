using PlayByPlay.Api;

namespace PlayByPlay
{
	class DownloadModel : BoxScoreRequest
	{
		public string SelectedTeam
		{
			get; set;
		}

		public DownloadModel()
		{
		}

		public DownloadModel(int year) : base(year)
		{
		}
	}
}