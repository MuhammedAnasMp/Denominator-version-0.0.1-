using System;
using System.Threading.Tasks;

namespace Deno.Services
{
    internal class AsyncRelayCommand<T> : IAsyncRelayCommand
    {
        private Func<object, Task> postToApiAsync;

        public AsyncRelayCommand(Func<object, Task> postToApiAsync)
        {
            this.postToApiAsync = postToApiAsync;
        }
    }
}