using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesService
{
	public interface IBackgroundTaskQueue
	{
		ValueTask QueueBackgroundWorkItemAsync(QueueCommand queueCommand);

		ValueTask<IAsyncEnumerable<QueueCommand>> DequeueAsync(CancellationToken token);

		ValueTask<IAsyncEnumerable<QueueCommand>> DequeueAllAsync();
	}
}
