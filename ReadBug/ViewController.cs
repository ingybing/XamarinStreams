using System;
using System.IO;
using System.Net;
using System.Text;
using UIKit;

namespace ReadBug
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.AddressBox.Text = "http://hsw10473-7vm.cse-servelec.com/rio/storeandforward/assessmentsresources.txt";
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        partial void FetchDocumentTouched(Foundation.NSObject sender)
        {
            this.DisplayOutput("Downloading ...");
            this.DownloadAndDisplayFile(this.AddressBox.Text);
        }

        private void DisplayOutput(string output)
        {
            this.InvokeOnMainThread(() => { this.OutputArea.Text = output; });
        }

        private void DownloadAndDisplayFile(string urlToDownload)
        {
			var url = new Uri(urlToDownload);
			var request = (HttpWebRequest)WebRequest.Create(url);
			var response = request.GetResponse();
			var stringBuilder = new StringBuilder();

			using (var stream = response.GetResponseStream())
			{
				if (stream != null)
				{
					const int BufferSize = 256;
					var encoding = Encoding.ASCII;
					using (var streamReader = new StreamReader(stream, encoding))
					{
						var buffer = new char[BufferSize];
						var count = streamReader.Read(buffer, 0, BufferSize);
						while (count > 0)
						{
							stringBuilder.Append(new string(buffer, 0, count));
							count = streamReader.Read(buffer, 0, BufferSize);
						}

						streamReader.Close();
						stream.Close();
						response.Close();
					}
				}
			}

            this.DisplayOutput(stringBuilder.ToString());
        }
    }
}
