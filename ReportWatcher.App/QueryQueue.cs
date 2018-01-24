namespace ReportWatcher.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public sealed class QueryQueue
    {
        /// <summary>
        /// The maximum threads.
        /// </summary>
        private readonly int maxThreads;

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The tasks.
        /// </summary>
        private readonly List<Task> tasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryQueue" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="maxThreads">The maximum threads.</param>
        internal QueryQueue(string name, int maxThreads)
        {
            this.name = name;
            this.maxThreads = maxThreads < 1 ? 1 : maxThreads;
            this.tasks = new List<Task>();
        }

        /// <summary>
        /// Starts the specified action.
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="action">The action.</param>
        /// <param name="parameters">The parameters.</param>
        public void Start<TParam>(Action<TParam, int> action, IReadOnlyList<TParam> parameters)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var index = i;

                lock (this.tasks)
                {
                    if (this.tasks.Count == this.maxThreads)
                    {
                        Task.WaitAny(this.tasks.ToArray());
                    }

                    this.tasks.RemoveAll(t => t.IsCompleted);
                    var newTask = Task.Run(() =>
                    {
                        try
                        {
                            Trace.WriteLine($"Spawning thread {this.name}_{this.tasks.Count}/{this.maxThreads}");
                            action(parameter, index);
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError(e.ToString());
                        }
                    });

                    this.tasks.Add(newTask);
                    Debug.Assert(this.tasks.Count <= this.maxThreads, "Thread spawn violation");
                }
            }

            lock (this.tasks)
            {
                Task.WaitAll(this.tasks.ToArray());
                this.tasks.RemoveAll(t => t.IsCompleted);
            }
        }
    }
}