using Android.App;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace PlayByPlay
{
	[Application]
	class PlayByPlayApp : Application
	{
		public IServiceProvider Services
		{
			get;
			private set;
		}


		protected PlayByPlayApp(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void OnCreate()
		{
			var services = new ServiceCollection();

			services.AddHttpClient<IPlayByPlayClient, PlayByPlayClient>(client =>
			{
				client.BaseAddress = new Uri("https://oskar.fritz.box:44360");
			});

			Services = services.BuildServiceProvider(validateScopes: true);

			base.OnCreate();
		}
	}
}