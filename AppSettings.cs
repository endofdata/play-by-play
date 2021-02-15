using Android.OS;
using Android.Runtime;
using Java.Interop;

namespace PlayByPlay
{
	class AppSettings : Java.Lang.Object, IParcelable
	{
		public bool IsDarkMode
		{
			get;
			set;
		}

		//public IntPtr Handle => base.PeerReference.Handle;

		public int DescribeContents() => 0;

		public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
		{
			dest.WriteByte((sbyte)(IsDarkMode ? 1 : 0));
		}

		private static readonly GenericParcelableCreator<AppSettings> _creator = new GenericParcelableCreator<AppSettings>(
			parcel => new AppSettings
			{
				IsDarkMode = parcel.ReadByte() == 1
			});

		[ExportField("CREATOR")]
		public static GenericParcelableCreator<AppSettings> GetCreator()
		{
			return _creator;
		}
	}
}