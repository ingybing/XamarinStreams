using System;
using System.Threading;

namespace ReadBug
{
	public class NonUiThreadSynchronisationContext : SynchronizationContext
	{
		#region Fields

		/// <summary>
		///     The blocking queue.
		/// </summary>
		private readonly BlockingQueue<SendOrPostCallbackItem> blockingQueue;

		/// <summary>
		///     The stop flag.
		/// </summary>
		private readonly AutoResetEvent stopFlag = new AutoResetEvent(false);

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initialises a new instance of the <see cref="NonUiThreadSynchronisationContext"/> class. 
		///     Initializes a new instance of the <see cref="NonUiThreadSynchronisationContext"/> class.
		/// </summary>
		public NonUiThreadSynchronisationContext()
		{
			this.blockingQueue = new BlockingQueue<SendOrPostCallbackItem>();
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// The set non ui synchronisation context if required.
		/// </summary>
		public static void SetNonUISynchronisationContextIfRequired()
		{
			// ReSharper disable RedundantNameQualifier
			if (SynchronizationContext.Current == null)
			{
				SynchronizationContext.SetSynchronizationContext(new NonUiThreadSynchronisationContext());
			}

			// ReSharper restore RedundantNameQualifier
		}

		/// <summary>
		/// The start non ui synchronisation context.
		/// </summary>
		public static void StartNonUISynchronisationContext()
		{
			var nonUiSynchronisationContext = Current as NonUiThreadSynchronisationContext;
			if (nonUiSynchronisationContext != null)
			{
				nonUiSynchronisationContext.StartMonitoringQueue();
			}
		}

		/// <summary>
		/// The stop non ui synchronisation context.
		/// </summary>
		public static void StopNonUISynchronisationContext()
		{
			var nonUiSynchronisationContext = Current as NonUiThreadSynchronisationContext;
			if (nonUiSynchronisationContext != null)
			{
				nonUiSynchronisationContext.StopMonitoringQueue();
			}
		}

		/// <summary>
		/// The create copy.
		/// </summary>
		/// <returns>
		/// The <see cref="SynchronizationContext"/>.
		/// </returns>
		public override SynchronizationContext CreateCopy()
		{
			return this;
		}

		/// <summary>
		/// The post method.
		/// </summary>
		/// <param name="d">
		/// The d.
		/// </param>
		/// <param name="state">
		/// The state.
		/// </param>
		public override void Post(SendOrPostCallback d, object state)
		{
			var item = new SendOrPostCallbackItem(d, state, true);
			this.blockingQueue.Enqueue(item);
		}

		/// <summary>
		/// The send method.
		/// </summary>
		/// <param name="d">
		/// The d.
		/// </param>
		/// <param name="state">
		/// The state.
		/// </param>
		public override void Send(SendOrPostCallback d, object state)
		{
			var item = new SendOrPostCallbackItem(d, state, false);
			this.blockingQueue.Enqueue(item);
			item.ExecutionCompletedFlag.WaitOne();
			if (item.Exception != null)
			{
				throw item.Exception;
			}
		}

		/// <summary>
		///     Start monitoring the queue for work from other threads.
		/// </summary>
		public void StartMonitoringQueue()
		{
			while (true)
			{
				var stop = this.stopFlag.WaitOne(0);
				if (stop)
				{
					break;
				}

				var item = this.blockingQueue.Dequeue();
				if (item != null)
				{
					item.Execute();
				}
			}
		}

		/// <summary>
		///     Stop monitoring the queue and releases any blocked readers.
		/// </summary>
		public void StopMonitoringQueue()
		{
			this.stopFlag.Set();
			this.blockingQueue.ReleaseReader();
		}

		#endregion
	}
}
