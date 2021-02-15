using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Essentials;

namespace PlayByPlay
{
	[Activity(Label = "@string/app_name", Theme = "@style/apptheme", MainLauncher = true)]
	public class MainActivity : AndroidX.AppCompat.App.AppCompatActivity
	{
		// A request code's purpose is to match the result of a "startActivityForResult" with
		// the type of the original request.  Choose any value.
		private const int Code_SelectFile = 1337;
		private const int Code_ShowSettings = 1338;
		private const int Code_Download = 1339;

		private int _currentPlay;
		private List<string> _lines;
		private TextView _txtPlay;
		private TextView _txtVisitorTeam;
		private TextView _txtVisitorScore;
		private TextView _txtHomeTeam;
		private TextView _txtHomeScore;
		private TextView _txtQuarter;
		private TextView _txtDownAndToGo;
		private TextView _txtPosition;
		private TextView _txtTime;
		private TextView _txtCurrentPlay;
		private SeekBar _sbCurrentPlay;
		private Android.Net.Uri _recentFile;

		internal AppSettings Settings
		{
			get;
		} = new AppSettings();

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			Platform.Init(this, savedInstanceState);
			SetContentView(Resource.Layout.activity_main);

			// Get all the output widgets and store them as members for easier access
			_txtPlay = FindViewById<TextView>(Resource.Id.txtPlay);
			_txtVisitorTeam = FindViewById<TextView>(Resource.Id.txtVisitorTeam);
			_txtVisitorScore = FindViewById<TextView>(Resource.Id.txtVisitorScore);
			_txtHomeTeam = FindViewById<TextView>(Resource.Id.txtHomeTeam);
			_txtHomeScore = FindViewById<TextView>(Resource.Id.txtHomeScore);
			_txtDownAndToGo = FindViewById<TextView>(Resource.Id.txtDownAndToGo);
			_txtPosition = FindViewById<TextView>(Resource.Id.txtPosition);
			_txtQuarter = FindViewById<TextView>(Resource.Id.txtQuarter);
			_txtTime = FindViewById<TextView>(Resource.Id.txtTime);
			_txtCurrentPlay = FindViewById<TextView>(Resource.Id.txtCurrentPlay);
			_sbCurrentPlay = FindViewById<SeekBar>(Resource.Id.sbCurrentPlay);

			// Update to scroll position
			_sbCurrentPlay.ProgressChanged += (sender, e) =>
			{
				GotoPlay(sender, e.Progress);
			};


			_txtPlay.MovementMethod = new ScrollingMovementMethod();

			// Attach navigation calls to prev and next play buttons
			FindViewById<Button>(Resource.Id.btnPrevious).Click += (sender, e) => GotoPlay(sender, _currentPlay - 1);
			FindViewById<Button>(Resource.Id.btnNext).Click += (sender, e) => GotoPlay(sender, _currentPlay + 1);
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			outState.PutParcelable(nameof(_recentFile), _recentFile);
			outState.PutInt(nameof(_currentPlay), _currentPlay);

			base.OnSaveInstanceState(outState);
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState(savedInstanceState);

			var recentFile = (Android.Net.Uri)savedInstanceState.GetParcelable(nameof(_recentFile));
			var currentPlay = savedInstanceState.GetInt(nameof(_currentPlay));

			ResumeStatus(recentFile, currentPlay);
		}

		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
		{
			switch (requestCode)
			{
			case Code_Download:
				if (resultCode == Result.Ok)
				{
					var fileName = data.GetStringExtra(DownloadActivity.ResultName);
					var reddit = data.GetStringArrayExtra(DownloadActivity.ResultData);

					if (!string.IsNullOrEmpty(fileName) && reddit?.Any() == true)
					{
						ResumeStatus(reddit);
						_recentFile = SaveFile(fileName, reddit);
					}
					else
					{
						AlertHelper.ShowMessage(this, "No play data available.", "Download");
					}
				}
				break;
			case Code_SelectFile:
				if (resultCode == Result.Ok)
				{
					// The document selected by the user won't be returned in the intent.
					// Instead, a URI to that document will be contained in the return intent
					// provided to this method as a parameter.  Pull that uri using "resultData.getData()"
					ResumeStatus(data.Data);
				}
				break;
			case Code_ShowSettings:

				break;
			}
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.menu_main, menu);
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
			case Resource.Id.action_selectfile:
				SelectFile();
				return true;

			case Resource.Id.action_download:
				Download();
				return true;

			case Resource.Id.action_settings:
				ShowSettings();
				return true;

			case Resource.Id.action_exit:
				Finish();
				return true;

			}
			return base.OnOptionsItemSelected(item);
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		public void SelectFile()
		{
			var intent = new Intent(Intent.ActionOpenDocument);

			// Filter to only show results that can be "opened", such as a file (as opposed to a list
			// of contacts or timezones)
			intent.AddCategory(Intent.CategoryOpenable);

			intent.SetType("text/plain");

			StartActivityForResult(intent, Code_SelectFile);
		}

		public void Download()
		{
			var intent = new Intent(this, typeof(DownloadActivity));

			StartActivityForResult(intent, Code_Download);
		}

		public Android.Net.Uri SaveFile(string fileName, IEnumerable<string> reddit)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentException($"Parameter '{nameof(fileName)}' cannot be null or empty.");
			}

			if (reddit?.Any() != true)
			{
				throw new ArgumentException($"Parameter '{nameof(reddit)}' cannot be null or empty array.");
			}

			Android.Net.Uri result = null;

			try
			{
				var folder = GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads);
				string path = System.IO.Path.Combine(folder.AbsolutePath, fileName);

				if (!File.Exists(path))
				{
					using (var writer = new StreamWriter(path, append: false, Encoding.UTF8))
					{
						foreach (var line in reddit)
						{
							writer.WriteLine(line);
						}
					}
				}

				result = new Android.Net.Uri.Builder()
					.Scheme("file")
					.AppendEncodedPath(path)
					.Build();
			}
			catch (IOException)
			{
				// TODO: Error handling
			}
			return result;
		}

		public void ShowSettings()
		{
			var intent = new Intent(this, typeof(SettingsActivity));

			intent.PutExtra(SettingsActivity.ModelKey, Settings);

			StartActivityForResult(intent, Code_ShowSettings);
		}

		private void ResumeStatus(Android.Net.Uri dataUri, int currentPlay = 0)
		{
			if (dataUri != null)
			{
				using (var stream = ContentResolver.OpenInputStream(dataUri))
				{
					ResumeStatus(new EnumerableLines(stream), currentPlay);
					_recentFile = dataUri;
				}
			}
		}

		private void ResumeStatus(IEnumerable<string> redditSource, int currentPlay = 0)
		{
			_lines = new List<string>();
			_recentFile = null;

			try
			{
				int skip = 0;

				foreach (var line in redditSource)
				{
					if (line.StartsWith('|'))
					{
						switch (skip++)
						{
						case 0:
							InitTeams(line);
							break;
						case 1:
							break;
						default:
							_lines.Add(line);
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				_txtPlay.Text = ex.Message;
			}

			_sbCurrentPlay.Max = _lines.Count - 1;

			GotoPlay(this, currentPlay);
		}

		private void GotoPlay(object sender, int index)
		{
			if (_lines?.Count > 0)
			{
				_currentPlay = Math.Max(0, Math.Min(index, _lines.Count - 1));

				if (sender != _sbCurrentPlay)
				{
					_sbCurrentPlay.Progress = _currentPlay;
				}
				if (sender != _txtCurrentPlay)
				{
					_txtCurrentPlay.Text = $"{_currentPlay + 1}";
				}
				var line = _lines[_currentPlay];

				// |Quarter|Time|Down|ToGo|Location|Detail|GNB|CAR|EPB|EPA|Win%|
				var tags = line.Split('|');

				if (tags.Length > 8)
				{
					_txtQuarter.Text = Order(tags[1]);
					_txtTime.Text = tags[2];
					_txtDownAndToGo.Text = string.IsNullOrEmpty(tags[3]) ? string.Empty : $"{Order(tags[3])} / {tags[4]}";
					_txtPosition.Text = tags[5];
					_txtPlay.Text = tags[6];
					_txtVisitorScore.Text = tags[7];
					_txtHomeScore.Text = tags[8];
				}
			}
		}

		private string Order(string value)
		{
			if (int.TryParse(value, out var numericVValue))
			{
				switch (numericVValue)
				{
				case 1:
					return "1st";
				case 2:
					return "2nd";
				case 3:
					return "3rd";
				case 4:
					return "4th";
				}
			}
			return value;
		}

		private void InitTeams(string line)
		{
			var tags = line.Split('|');

			if (tags.Length > 8)
			{
				_txtVisitorTeam.Text = tags[7];
				_txtHomeTeam.Text = tags[8];
			}
		}

	}
}

