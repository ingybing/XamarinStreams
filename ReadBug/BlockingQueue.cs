using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ReadBug
{
	public class BlockingQueue<T> : IDisposable
	{
		#region Fields

		/// <summary>
		///     The kill thread flag.
		/// </summary>
		private readonly AutoResetEvent killThreadFlag = new AutoResetEvent(false);

		/// <summary>
		///     The queue.
		/// </summary>
		private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

		/// <summary>
		///     The wait handles.
		/// </summary>
		private readonly WaitHandle[] waitHandles;

		/// <summary>
		///     The semaphore.
		/// </summary>
		private Semaphore semaphore = new Semaphore(0, int.MaxValue);

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initialises a new instance of the <see cref="BlockingQueue{T}"/> class. 
		/// </summary>
		public BlockingQueue()
		{
			this.waitHandles = new WaitHandle[] { this.semaphore, this.killThreadFlag };
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		///     The de-queue.
		/// </summary>
		/// <returns>
		///     The <see cref="T" />.
		/// </returns>
		public T Dequeue()
		{
			WaitHandle.WaitAny(this.waitHandles);
			lock (this.queue)
			{
				if (this.queue.Count > 0)
				{
					T item;
					var success = this.queue.TryDequeue(out item);
					if (success)
					{
						return item;
					}
				}
			}

			return default(T);
		}

		/// <summary>
		///     The dispose.
		/// </summary>
		public void Dispose()
		{
			if (this.semaphore != null)
			{
				this.semaphore.Close();
				this.semaphore = null;
			}
		}

		/// <summary>
		/// The enqueue.
		/// </summary>
		/// <param name="data">
		/// The data.
		/// </param>
		public void Enqueue(T data)
		{
			lock (this.queue)
			{
				this.queue.Enqueue(data);
			}

			this.semaphore.Release();
		}

		/// <summary>
		///     The release reader.
		/// </summary>
		public void ReleaseReader()
		{
			this.killThreadFlag.Set();
		}

		#endregion
	}
}
