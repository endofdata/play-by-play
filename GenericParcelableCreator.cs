using Android.OS;
using System;

namespace PlayByPlay
{
	public sealed class GenericParcelableCreator<T> : Java.Lang.Object, IParcelableCreator
        where T : Java.Lang.Object, new()
    {
        private readonly Func<Parcel, T> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelableDemo.GenericParcelableCreator`1"/> class.
        /// </summary>
        /// <param name='createFromParcelFunc'>
        /// Func that creates an instance of T, populated with the values from the parcel parameter
        /// </param>
        public GenericParcelableCreator(Func<Parcel, T> createFromParcelFunc)
        {
            _factory = createFromParcelFunc;
        }

		public Java.Lang.Object CreateFromParcel(Parcel source) => _factory(source);

		public Java.Lang.Object[] NewArray(int size) => new T[size];
	}
}