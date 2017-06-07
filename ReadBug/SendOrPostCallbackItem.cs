using System;
using System.Threading;

namespace ReadBug
{
	public class SendOrPostCallbackItem
	{
		#region Fields

		/// <summary>
		///     The call back.
		/// </summary>
		private readonly SendOrPostCallback callback;

		/// <summary>
		///     The execute asynchronously.
		/// </summary>
		private readonly bool executeAsynchronously;

		/// <summary>
		///     The execution completed flag.
		/// </summary>
		private readonly ManualResetEvent executionCompletedFlag = new ManualResetEvent(false);

		/// <summary>
		///     The state.
		/// </summary>
		private readonly object state;

		/// <summary>
		///     The exception.
		/// </summary>
		private Exception exception;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initialises a new instance of the <see cref="SendOrPostCallbackItem"/> class.
		/// </summary>
		/// <param name="callback">
		/// The call back.
		/// </param>
		/// <param name="state">
		/// The state.
		/// </param>
		/// <param name="executeAsynchronously">
		/// The execute asynchronously.
		/// </param>
		public SendOrPostCallbackItem(SendOrPostCallback callback, object state, bool executeAsynchronously)
		{
			this.callback = callback;
			this.state = state;
			this.executeAsynchronously = executeAsynchronously;
		}

		#endregion

		#region Public Properties

		/// <summary>
		///     Gets the exception.
		/// </summary>
		public Exception Exception
		{
			get
			{
				return this.exception;
			}
		}

		/// <summary>
		///     Gets the execution completed flag.
		/// </summary>
		public ManualResetEvent ExecutionCompletedFlag
		{
			get
			{
				return this.executionCompletedFlag;
			}
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		///     The execute.
		/// </summary>
		public void Execute()
		{
			if (this.executeAsynchronously)
			{
				this.Post();
			}
			else
			{
				this.Send();
			}
		}

		#endregion

		#region Methods

		/// <summary>
		///     The post method.
		/// </summary>
		private void Post()
		{
			this.callback(this.state);
		}

		/// <summary>
		///     The send method.
		/// </summary>
		private void Send()
		{
			try
			{
				this.callback(this.state);
			}
			catch (Exception ex)
			{
				this.exception = ex;
			}
			finally
			{
				this.executionCompletedFlag.Set();
			}
		}

		#endregion
	}
}
