using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

// Orginal repository:
// https://github.com/kkokosa/PooledValueTaskSource

namespace PooledValueTaskSource
{
    public class FileReadingPooledValueTaskSource : IValueTaskSource<string>
    {
        /// Sentinel object used to indicate that the operation has completed prior to OnCompleted being called.
        private static readonly Action<object> CallbackCompleted = _ => { Debug.Assert(false, "Should not be invoked"); };

        private Action<object> continuation;
        private string result;
        private Exception exception;

        /// <summary>Current token value given to a ValueTask and then verified against the value it passes back to us.</summary>
        /// <remarks>
        /// This is not meant to be a completely reliable mechanism, doesn't require additional synchronization, etc.
        /// It's purely a best effort attempt to catch misuse, including awaiting for a value task twice and after
        /// it's already being reused by someone else.
        /// </remarks>
        private short token;
        private object state;
        private ObjectPool<FileReadingPooledValueTaskSource> pool;
        private ExecutionContext executionContext;
        private object scheduler;

        public string GetResult(short token)
        {
            Console.WriteLine("GetResult");
            var exception = this.exception;
            var result = ResetAndReleaseOperation();
            if (exception != null)
            {
                throw exception;
            }
            return result;
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            if (token != this.token)
                ThrowMultipleContinuations();

            Console.Write("GetStatus:");
            if (result == null)
            {
                Console.WriteLine("pending");
                return ValueTaskSourceStatus.Pending;
            }
            Console.WriteLine("completed: succeeded or faulted");
            return exception != null ? ValueTaskSourceStatus.Succeeded : ValueTaskSourceStatus.Faulted;
        }

        /// <summary>Called on awaiting so:
        /// - if operation has not yet completed - queues the provided continuation to be executed once the operation is completed
        /// - if operation has completed - 
        /// </summary>
        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            Console.WriteLine("." + token);
            if (token != this.token)
                ThrowMultipleContinuations();

            if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) != 0)
            {
                executionContext = ExecutionContext.Capture();
            }

            if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != 0)
            {
                SynchronizationContext sc = SynchronizationContext.Current;
                if (sc != null && sc.GetType() != typeof(SynchronizationContext))
                {
                    scheduler = sc;
                }
                else
                {
                    TaskScheduler ts = TaskScheduler.Current;
                    if (ts != TaskScheduler.Default)
                    {
                        scheduler = ts;
                    }
                }
            }

            // Remember current state
            this.state = state;
            // Remember continuation to be executed on completed (if not already completed, in case of which
            // continuation will be set to CallbackCompleted)
            var previousContinuation = Interlocked.CompareExchange(ref this.continuation, continuation, null);
            if (previousContinuation != null)
            {
                if (!ReferenceEquals(previousContinuation, CallbackCompleted))
                    ThrowMultipleContinuations();

                // Lost the race condition and the operation has now already completed.
                // We need to invoke the continuation, but it must be asynchronously to
                // avoid a stack dive.  However, since all of the queueing mechanisms flow
                // ExecutionContext, and since we're still in the same context where we
                // captured it, we can just ignore the one we captured.
                executionContext = null;
                this.state = null; // we have the state in "state"; no need for the one in UserToken
                InvokeContinuation(continuation, state, forceAsync: true);
            }
        }

        public ValueTask<string> RunAsync(string filename, ObjectPool<FileReadingPooledValueTaskSource> pool)
        {
            Debug.Assert(Volatile.Read(ref continuation) == null, $"Expected null continuation to indicate reserved for use");
            this.pool = pool;

            // Start async op
            var isCompleted = FireAsyncWorkWithSyncReturnPossible(filename);
            if (!isCompleted)
            {
                // Opearation not yet completed. Return ValueTask wrapping us.
                Console.WriteLine("Asynchronous path.");
                return new ValueTask<string>(this, token);
            }

            // OMG so happy, we catch up! Just return ValueTask wrapping the result.
            Console.WriteLine("Synchronous path.");
            var result = ResetAndReleaseOperation();
            return new ValueTask<string>(result);
        }

        private bool FireAsyncWorkWithSyncReturnPossible(string filename)
        {
            if (filename == @"c:\dummy.txt")
            {
                // Simulate sync path
                this.result = filename;
                return true;
            }
            // Simulate some low-level, unmanaged, asynchronous work. This normally:
            // - would call an OS-level API with callback registered
            // - after some time registered callback would be triggered (with NotifyAsyncWorkCompletion call inside)
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(1000);
                var data = File.ReadAllText(filename);
                NotifyAsyncWorkCompletion(data);
            });
            return false;
        }

        private void NotifyAsyncWorkCompletion(string data, Exception exception = null)
        {
            this.result = data;
            this.exception = exception;

            // Mark operation as completed
            var previousContinuation = Interlocked.CompareExchange(ref this.continuation, CallbackCompleted, null);
            if (previousContinuation != null)
            {
                // Async work completed, continue with... continuation
                ExecutionContext ec = executionContext;
                if (ec == null)
                {
                    InvokeContinuation(previousContinuation, this.state, forceAsync: false);
                }
                else
                {
                    // This case should be relatively rare, as the async Task/ValueTask method builders
                    // use the awaiter's UnsafeOnCompleted, so this will only happen with code that
                    // explicitly uses the awaiter's OnCompleted instead.
                    executionContext = null;
                    ExecutionContext.Run(ec, runState =>
                    {
                        var t = (Tuple<FileReadingPooledValueTaskSource, Action<object>, object>)runState;
                        t.Item1.InvokeContinuation(t.Item2, t.Item3, forceAsync: false);
                    }, Tuple.Create(this, previousContinuation, this.state));
                }
            }
        }

        private void InvokeContinuation(Action<object> continuation, object state, bool forceAsync)
        {
            if (continuation == null)
                return;

            object scheduler = this.scheduler;
            this.scheduler = null;
            if (scheduler != null)
            {
                if (scheduler is SynchronizationContext sc)
                {
                    sc.Post(s =>
                    {
                        var t = (Tuple<Action<object>, object>)s;
                        t.Item1(t.Item2);
                    }, Tuple.Create(continuation, state));
                }
                else
                {
                    Debug.Assert(scheduler is TaskScheduler, $"Expected TaskScheduler, got {scheduler}");
                    Task.Factory.StartNew(continuation, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, (TaskScheduler)scheduler);
                }
            }
            else if (forceAsync)
            {
                ThreadPool.QueueUserWorkItem(continuation, state, preferLocal: true);
            }
            else
            {
                continuation(state);
            }
        }

        public void ThrowMultipleContinuations()
        {
            throw new InvalidOperationException("Multiple awaiters are not allowed");
        }

        protected virtual string ResetAndReleaseOperation()
        {
            string result = this.result;
            this.token++;
            this.result = null;
            this.exception = null;
            this.state = null;
            this.continuation = null;
            this.pool.Return(this);
            return result;
        }
    }
}
