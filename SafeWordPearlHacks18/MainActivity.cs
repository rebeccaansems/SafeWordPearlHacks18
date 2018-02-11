using Android.App;
using Android.Widget;
using Android.OS;
using Android.Speech;
using Android.Content;
using System;
using Android.Util;
using Google.Apis.Speech.v1.Data;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Media;
using System.IO;

namespace SafeWordPearlHacks18
{
    [Activity(Label = "SafeWordPearlHacks18", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private const int SpeechResult = 5;

        MediaRecorder _recorder;
        MediaPlayer _player;
        Button buttonMonitor;

        private int prevFileProcessed;
        private int counter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            prevFileProcessed = -1;
            isInCall = false;

            counter = 0;

            buttonMonitor = FindViewById<Button>(Resource.Id.bMainMonitor);

            _recorder = new MediaRecorder();
            _player = new MediaPlayer();

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(10);

            buttonMonitor.Click += delegate
            {
                buttonMonitor.Enabled = false;
                var timer = new System.Threading.Timer((e) =>
                {
                    StartRecording(counter);
                    if (counter > 0)
                    {
                        StopRecording(counter);
                    }
                    counter++;
                }, null, startTimeSpan, periodTimeSpan);
            };
        }

        private void StartRecording(int count)
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            path = Path.Combine(path, "myfile" + count + ".amr");

            if (count > 0)
            {
                _recorder.Stop();
                _recorder.Reset();
            }

            _recorder.SetOutputFile(path);

            _recorder.SetAudioSource(AudioSource.Mic);
            _recorder.SetOutputFormat(OutputFormat.AmrWb);
            _recorder.SetAudioEncoder(AudioEncoder.AmrWb);
            _recorder.SetAudioEncodingBitRate(16000);
            _recorder.SetAudioChannels(1);

            _recorder.Prepare();
            _recorder.Start();
        }

        private void StopRecording(int count)
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            path = Path.Combine(path, "myfile" + (count - 1) + ".amr");

            _player.SetDataSource(path);
            _player.Prepare();
            _player.Start();
            _player.Reset();

            var byteOri = File.ReadAllBytes(path);
            string bytes = Convert.ToBase64String(byteOri);

            SendSpeech(bytes);
        }

        private void TranslateRecording(int count)
        {
            //if (count != prevFileProcessed)
            {
                prevFileProcessed = count;
                string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                path = Path.Combine(path, "myfile" + count + ".amr");

                var byteOri = File.ReadAllBytes(path);
                string bytes = Convert.ToBase64String(byteOri);

                SendSpeech(bytes);
            }
        }

        private void SendSpeech(string contents)
        {
            AudioTranslate audioTranslate = new AudioTranslate
            {
                audio = new Audio
                {
                    content = contents
                },
                config = new Config
                {
                    encoding = "AMR_WB",
                    languageCode = "en-US",
                    sampleRateHertz = 16000
                }
            };

            Uri sUrl = new Uri("https://speech.googleapis.com/v1/speech:recognize?key=" + "AIzaSyA4Kp8siN_j7nn4qQ8PbkCAOgnADweKdHk");
            string sContentType = "txt/json"; // or application/xml

            string json = JsonConvert.SerializeObject(audioTranslate);

            HttpClient oHttpClient = new HttpClient();
            var oTaskPostAsync = oHttpClient.PostAsync(sUrl, new StringContent(json, System.Text.Encoding.UTF8, sContentType));

            ToastResponse(oTaskPostAsync, ApplicationContext);
        }
        private bool isInCall;

        private async void ToastResponse(Task<HttpResponseMessage> oTaskPostAsync, Context context)
        {
            if (oTaskPostAsync.Result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                dynamic json = JValue.Parse(await oTaskPostAsync.Result.Content.ReadAsStringAsync());
                if (json.results != null)
                {
                    string statement = json.results[0].alternatives[0].transcript;
                    if (statement.Contains("test") && isInCall == false)
                    {
                        isInCall = true;
                        var uri = Android.Net.Uri.Parse("tel:9026709064");
                        var intent = new Intent(Intent.ActionCall, uri);
                        StartActivity(intent);
                    }
                }
            }
        }
    }

}

public class AudioTranslate
{
    public AudioTranslate() { }

    public Audio audio;
    public Config config;
}

public class Audio
{
    public Audio() { }
    public string content;
}

public class Config
{
    public Config() { }
    public string encoding;
    public string languageCode;
    public int sampleRateHertz;
}
