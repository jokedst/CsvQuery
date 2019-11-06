namespace CsvQuery.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Benchmarking timer that allows several checkpoints to be set in the code
    /// </summary>
    public class DiagnosticTimer
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly List<Tuple<string, long>> _checkpoints = new List<Tuple<string, long>>();

        /// <summary>
        /// Adds a named checkpoint to the running timer
        /// </summary>
        /// <param name="name">Checkpoint name</param>
        public void Checkpoint(string name)
        {
            this._checkpoints.Add(new Tuple<string, long>(name, this._stopwatch.ElapsedMilliseconds));
        }

        /// <summary>
        /// Adds the last named checkpoint and returns a string describing time taken between each checkpoint
        /// </summary>
        /// <param name="name">Checkpoint name</param>
        /// <returns>Summary text of all checkpoints and total time</returns>
        public string LastCheckpoint(string name)
        {
            this._stopwatch.Stop();
            this._checkpoints.Add(new Tuple<string, long>(name, this._stopwatch.ElapsedMilliseconds));

            long lastCheckpoint = 0;
            var sb = new StringBuilder();
            foreach (var checkpoint in this._checkpoints)
            {
                sb.AppendFormat("{0}: {1}ms\n", checkpoint.Item1, checkpoint.Item2 - lastCheckpoint);
                lastCheckpoint = checkpoint.Item2;
            }
            sb.AppendFormat("Total time: {0}ms", this._stopwatch.ElapsedMilliseconds);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Benchmarking timer that allows several checkpoints to be set in the code, and each checkpoint might be hit several times
    /// </summary>
    public class LoopDiagnosticTimer
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly Dictionary<string, List<long>> _checkpoints = new Dictionary<string, List<long>>();
        private readonly List<string> _namesInsertOrder = new List<string>();

        /// <summary>
        /// Adds a measure to named checkpoint
        /// </summary>
        /// <param name="name">Checkpoint name</param>
        public void Checkpoint(string name)
        {
            if (!this._checkpoints.TryGetValue(name, out var times))
            {
                times = new List<long>();
                this._checkpoints[name] = times;
                this._namesInsertOrder.Add(name);
            }

            times.Add(this._stopwatch.ElapsedMilliseconds);
            this._stopwatch.Restart();
        }

        /// <summary>
        /// Adds the last checkpoint time and returns a string describing all times
        /// </summary>
        public string LastCheckpoint(string name)
        {
            this.Checkpoint(name);
            this._stopwatch.Stop();

            var sb = new StringBuilder();
            long total = 0;
            foreach (var checkpointName in this._namesInsertOrder)
            {
                var checkpoint = this._checkpoints[checkpointName];
                if (checkpoint.Count == 1)
                    sb.AppendFormat("{0}: {1}ms\n", checkpointName, checkpoint[0]);
                else if (checkpoint.Count == 2)
                    sb.AppendFormat("{0} (2 hits): avg {1}ms\n", checkpointName, checkpoint.Average());
                else
                    sb.AppendFormat("{0} ({1} hits): avg {2}ms ({3}ms)\n", checkpointName, checkpoint.Count, checkpoint.Average(), checkpoint.OrderBy(x => x).Skip(1).Take(checkpoint.Count - 2).Average());
                total += checkpoint.Sum();
            }

            sb.AppendFormat("Total time: {0}ms", total);
            return sb.ToString();
        }
    }
}
