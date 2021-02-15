using Android.Content;
using Android.OS;
using Android.Widget;

namespace PlayByPlay
{
	class ServiceConnection : Java.Lang.Object, IServiceConnection
	{
		private readonly AutoCompleteTextView _listView;

		public ServiceConnection(AutoCompleteTextView listView)
		{
			_listView = listView ?? throw new System.ArgumentNullException(nameof(listView));
		}

		public void OnServiceConnected(ComponentName name, IBinder service)
		{
		}

		public void OnServiceDisconnected(ComponentName name)
		{
		}
	}
}