
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;

namespace PlayByPlay
{
	[Activity(Label = "@string/action_settings")]
	[IntentFilter(actions: new[] { Intent.ActionApplicationPreferences }, Categories = new string[] { Intent.CategoryPreference })]
	public class SettingsActivity : PreferenceActivity
	{
		public const string ModelKey = "settings";

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_settings);

			FragmentManager
				.BeginTransaction()
				.Replace(Resource.Id.settings_frame, new SettingsFragment())
				.Commit();
		}

		protected override bool IsValidFragment(string fragmentName) => fragmentName == typeof(SettingsFragment).Name;
	}
}