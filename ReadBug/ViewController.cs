using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

            this.AddressBox.Text = "http://www.ingybing.com/sampletext.txt";
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

		/*******************************************
		 * 
		 * Fetch Button Touch Handlers
		 * 
         *******************************************/

		partial void FetchDocumentTouched(Foundation.NSObject sender)
        {
            this.ClearOutput();
            this.DebugOutput("FD Thread ID: " + Thread.CurrentThread.ManagedThreadId);
            var url = this.AddressBox.Text;
            this.DebugOutput("Downloading ...");
            this.DownloadAndDisplayFile(url);
        }

        partial void FetchDocumentInTaskTouched(Foundation.NSObject sender)
        {
            this.ClearOutput();
            this.DebugOutput("FDIT Thread ID: " + Thread.CurrentThread.ManagedThreadId);
            this.ClearOutput();
            var url = this.AddressBox.Text;

            this.DoWorkInNewThreadAndReturnOnOriginatingThread<bool>(
                () => 
                {
                    this.DebugOutput("FDIT - ACTION Work Thread ID: " + Thread.CurrentThread.ManagedThreadId);
                    this.DebugOutput("Downloading");
                    this.DownloadAndDisplayFile(url);
                    return true;
                }, 
                result => 
                {
                    this.DebugOutput("FDIT Result Thread ID: " + Thread.CurrentThread.ManagedThreadId);
                    this.DebugOutput("Done");
                }, 
                this.ExceptionHandler);
        }

        partial void FetchDocumentInCustomContextTouched(Foundation.NSObject sender)
        {
            this.ClearOutput();
            this.DebugOutput("FDICC Thread ID: " + Thread.CurrentThread.ManagedThreadId);
            var url = this.AddressBox.Text;

            var task = new Task(() => this.StartSecondBackgroundThread(url));
            task.Start();
        }

		/*******************************************
         * 
         * Private Methods
         * 
         *******************************************/

		private void ClearOutput()
		{
			this.InvokeOnMainThread(() => { this.DebugConsole.Text = string.Empty; this.OutputArea.Text = string.Empty; });
		}

		private void DebugOutput(string output)
		{
			this.InvokeOnMainThread(() => { this.DebugConsole.Text = this.DebugConsole.Text + "\n" + output; });
		}

		private void DisplayOutput(string output)
		{
			this.InvokeOnMainThread(() => { this.OutputArea.Text = output; });
		}

		private void DownloadAndDisplayFile(string urlToDownload)
		{
            this.DisplayOutput("Downloading: ...");

			if (SynchronizationContext.Current != null)
			{
				this.DebugOutput("SYNCHRONIZATIONCONTEXT: " + SynchronizationContext.Current.GetType().FullName);
			}
			else
			{
				this.DebugOutput("SYNCHRONIZATIONCONTEXT: NULL");
			}

			this.DebugOutput("DownloadAndDisplayFile Thread ID: " + Thread.CurrentThread.ManagedThreadId);
			var url = new Uri(urlToDownload);
			this.DebugOutput("Creating Request.");
			var request = (HttpWebRequest)WebRequest.Create(url);
			this.DebugOutput("Getting Response.");
			var response = request.GetResponse();
			var stringBuilder = new StringBuilder();

			this.DebugOutput("Processing response stream");
			using (var stream = response.GetResponseStream())
			{
				if (stream != null)
				{
					const int BufferSize = 5000;
					this.DebugOutput("Buffer size: " + BufferSize);
					var encoding = Encoding.ASCII;
					using (var streamReader = new StreamReader(stream, encoding))
					{
						var buffer = new char[BufferSize];
						var count = streamReader.Read(buffer, 0, BufferSize);
						this.DebugOutput("Count: " + count);
						while (count > 0)
						{
							stringBuilder.Append(new string(buffer, 0, count));
							this.DebugOutput("StringBuilder Length: " + stringBuilder.Length);
							this.DebugOutput("Reading Stream Again");
							count = streamReader.Read(buffer, 0, BufferSize);
							this.DebugOutput("Count: " + count);
						}

						this.DebugOutput("SUCCESS, closing stream etc");
						streamReader.Close();
						stream.Close();
						response.Close();
					}
				}
			}

			this.DisplayOutput(stringBuilder.ToString());
		}

        public void DoWorkInNewThreadAndReturnOnOriginatingThread<T>(Func<T> actionToPerform, Action<T> resultDelegate, Action<Exception> exceptionDelegate)
		{
			this.DebugOutput("DWINTAROOT Thread ID: " + Thread.CurrentThread.ManagedThreadId);
			var context = SynchronizationContext.Current;

			var originatingThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();

			var task = new Task<T>(actionToPerform);

			task.ContinueWith(
				completedTask =>
				{
					this.DebugOutput("DWINTAROOT ContinueWith Thread ID: " + Thread.CurrentThread.ManagedThreadId);
					if (SynchronizationContext.Current == null)
					{
						// .Net 4.0 bug. The execution context is altered and looses the synchronisation context so grab te original reference to it.
						// This is aparantly fixed in .Net 4.5
						SynchronizationContext.SetSynchronizationContext(context);
					}

					if (completedTask.IsFaulted)
					{
						exceptionDelegate(completedTask.Exception);
					}
					else
					{
						resultDelegate(completedTask.Result);
					}
				},
				originatingThreadScheduler);

			task.Start(TaskScheduler.Default);
		}

		private void ExceptionHandler(Exception ex)
		{
			this.DisplayOutput(ex.Message);
		}

        private void StartSecondBackgroundThread(string url)
        {
            this.DebugOutput("SSBT Thread ID: " + Thread.CurrentThread.ManagedThreadId);
            NonUiThreadSynchronisationContext.SetNonUISynchronisationContextIfRequired();

            this.DownloadAndDisplayFile(url);

			this.DoWorkInNewThreadAndReturnOnOriginatingThread<bool>(
				() =>
				{
                    this.DebugOutput("SSBT - ACTION Thread ID: " + Thread.CurrentThread.ManagedThreadId);
					this.DebugOutput("Simulating another async action and performing download after that has finished on it's original thread.");
                    this.DownloadAndDisplayFile(url);
					return true;
				},
				result =>
				{
                    // SynchronizationContext.SetSynchronizationContext(null);
                    this.DownloadAndDisplayFile(url);
                },
				this.ExceptionHandler);

            NonUiThreadSynchronisationContext.StartNonUISynchronisationContext();
        }
    }
}
