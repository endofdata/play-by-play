using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using PlayByPlay.Api;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace PlayByPlay
{
	[Activity(Label = "DownloadActivity")]
	public class DownloadActivity : Activity
	{
		public const string ResultData = "reddit";
		public const string ResultName = "suggested_name";

		private DownloadModel _model;
		private Spinner _spnTeam;
		private ListView _lstBoxScores;
		private IPlayByPlayClient _client;
		private Team[] _teams;
		private BoxScoreUrl[] _boxScoreUrls;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_download);

			_client = ((PlayByPlayApp)Application).Services.GetRequiredService<IPlayByPlayClient>();
			_model = new DownloadModel(DateTimeOffset.Now.Year);
			_spnTeam = FindViewById<Spinner>(Resource.Id.spn_team);
			_lstBoxScores = FindViewById<ListView>(Resource.Id.lst_boxscores);

			var edSeason = FindViewById<EditText>(Resource.Id.ed_season);

			if (Connectivity.NetworkAccess == NetworkAccess.Internet)
			{
				_spnTeam.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(TeamSpinner_ItemSelected);
				_lstBoxScores.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(BoxScoreList_ItemClick);

				edSeason.FocusChange += async (sender, e) =>
				{
					if (int.TryParse(edSeason.Text, out int season))
					{
						_model.Year = await LoadTeamsAsync(season);
					}
				};
			}
			edSeason.Text = _model.Year.ToString(CultureInfo.CurrentCulture);

			var btnFind = FindViewById<Button>(Resource.Id.btn_find);

			btnFind.Click += async (sender, e) => await LoadBoxScoresAsync();

			// Create your application here
		}

		private void TeamSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
		{
			if (e.Position >= 0 && e.Position < _teams?.Length)
			{
				_model.SelectedTeam = _teams[e.Position].NFLCode;
			}
		}

		private async void BoxScoreList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			if (e.Position >= 0 && e.Position < _boxScoreUrls?.Length)
			{
				var boxScore = _boxScoreUrls[e.Position];

				try
				{
					var reddit = await _client.GetRedditAsync(new PlayRequest
					{
						Season = _model.Year,
						Week = _model.Filter.Week,
						AwayCode = boxScore.AwayCode,
						HomeCode = boxScore.HomeCode,
						GameId = boxScore.GetGameId()
					}).ConfigureAwait(true);

					var returnIntent = new Intent();
					returnIntent.PutExtra(ResultData, reddit);

					if (reddit?.Length > 0)
					{
						returnIntent.PutExtra(ResultName, FileNameBuilder.GetFileName(reddit[0], _model.Year, _model.Filter.Week));
					}
					SetResult(Result.Ok, returnIntent);

					Finish();
				}
				catch (Exception ex) when
				(
					ex is HttpRequestException ||
					ex is System.OperationCanceledException
				)
				{
					AlertHelper.ShowException(this, ex);
				}
			}
		}

		private async Task LoadBoxScoresAsync()
		{
			try
			{
				var filter = _model.Filter;

				if (!int.TryParse(FindViewById<EditText>(Resource.Id.ed_week).Text, out var week))
				{
					week = 0;
				}

				filter.Week = week;
				filter.Away = filter.Home = filter.Team = null;

				switch (FindViewById<RadioGroup>(Resource.Id.rg_team_slot).CheckedRadioButtonId)
				{
				case Resource.Id.rb_away:
					filter.Away = _model.SelectedTeam;
					break;
				case Resource.Id.rb_home:
					filter.Home = _model.SelectedTeam;
					break;
				case Resource.Id.rb_both:
					filter.Team = _model.SelectedTeam;
					break;
				default:
					throw new InvalidOperationException("Unexpected team slot.");
				}

				_boxScoreUrls = await _client.GetBoxScoreUrlsAsync(_model);
				var displayUrls = _boxScoreUrls.Select(b => $"{b.Date:MM/dd}: {b.AwayCode} @ {b.HomeCode}").ToArray();
				var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSelectableListItem, displayUrls);
				adapter.SetDropDownViewResource(Android.Resource.Layout.ListContent);
				_lstBoxScores.Adapter = adapter;
			}
			catch (Exception ex) when
			(
				ex is HttpRequestException ||
				ex is System.OperationCanceledException
			)
			{
				AlertHelper.ShowException(this, ex);
			}
		}

		private async Task<int> LoadTeamsAsync(int season)
		{
			try
			{
				if (season > 1900 && season <= DateTime.UtcNow.Year)
				{
					if (season != _model.Year || _spnTeam.Adapter == null)
					{
						_teams = await _client.GetTeamsAsync(_model.Year).ConfigureAwait(true);
						var teamNames = _teams.Select(t => t.Nickname ?? "???").ToArray();
						var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, teamNames);
						adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
						_spnTeam.Adapter = adapter;

						return season;
					}
				}
			}
			catch (Exception ex) when
			(
				ex is HttpRequestException ||
				ex is System.OperationCanceledException
			)
			{
				AlertHelper.ShowException(this, ex);
			}

			return _model.Year;
		}
	}
}