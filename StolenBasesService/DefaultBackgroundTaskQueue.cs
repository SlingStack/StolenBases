using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace StolenBasesService
{
	public readonly record struct QueueCommand(Func<CancellationToken, string, ValueTask> QueueFunc, string Command);
	
	public sealed class DefaultBackgroundTaskQueue : IBackgroundTaskQueue
	{
		private readonly Channel<QueueCommand> _queue;

		public DefaultBackgroundTaskQueue(int capacity)
		{
			BoundedChannelOptions options = new(capacity)
			{
				FullMode = BoundedChannelFullMode.Wait
			};

			_queue = Channel.CreateBounded<QueueCommand>(options);
		}

		public async ValueTask QueueBackgroundWorkItemAsync(QueueCommand qc)
		{
			ArgumentNullException.ThrowIfNull(qc);

			await _queue.Writer.WriteAsync(qc);
		}

		public async ValueTask<IAsyncEnumerable<QueueCommand>> DequeueAsync(CancellationToken cancellationToken)
		{
			IAsyncEnumerable<QueueCommand>? qc = _queue.Reader.ReadAllAsync(cancellationToken);

			return qc;
		}
	}
}
