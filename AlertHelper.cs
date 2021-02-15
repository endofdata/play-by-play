using Android.App;
using System;
using System.Reflection;
using System.Text;

namespace PlayByPlay
{
	internal static class AlertHelper
	{
		public static void ShowMessage(Activity context, string message, string title)
		{
			context.RunOnUiThread(() =>
			{
				var alert = new AlertDialog.Builder(context)
					.SetMessage(message)
					.SetTitle(title)
					.SetPositiveButton("OK", (sender, e) =>
					{
					}).Show();
			});
		}

		public static void ShowException(Activity context, Exception exception, string title = null)
		{
			var strb = new StringBuilder();

			BuildMessage(strb, exception);

			ShowMessage(context, strb.ToString(), title ?? "Error");
		}

		private static void BuildMessage(StringBuilder strb, Exception exception)
		{
			while (exception != null)
			{
				strb.AppendLine(exception.Message);

				if (exception is AggregateException arx)
				{
					foreach (var innerEx in arx.InnerExceptions)
					{
						BuildMessage(strb, innerEx);
					}
					exception = null;
				}
				else if (exception is ReflectionTypeLoadException rtlx)
				{
					foreach (var innerEx in rtlx.LoaderExceptions)
					{
						BuildMessage(strb, innerEx);
					}
					exception = null;
				}
				else
				{
					exception = exception.InnerException;
				}
			}
		}
	}
}