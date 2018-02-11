using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SafeWordPearlHacks18
{
    [Activity(Label = "Safe Word", MainLauncher = false)]
    public class Options : Activity
    {
        Button buttonMainScreen;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Options);

            buttonMainScreen = FindViewById<Button>(Resource.Id.bMainScreen);

            buttonMainScreen.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            };
        }

    }
}