namespace CsvQuery.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
            _checkpoints.Add(new Tuple<string, long>(name, _stopwatch.ElapsedMilliseconds));
        }

        /// <summary>
        /// Adds the last named checkpoint and returns a string describing time taken between each checkpoint
        /// </summary>
        /// <param name="name">Checkpoint name</param>
        /// <returns>Summary text of all checkpoints and total time</returns>
        public string LastCheckpoint(string name)
        {
            _stopwatch.Stop();
            _checkpoints.Add(new Tuple<string, long>(name, _stopwatch.ElapsedMilliseconds));

            long lastCheckpoint = 0;
            var sb = new StringBuilder();
            foreach (var checkpoint in _checkpoints)
            {
                sb.AppendFormat("{0}: {1}ms\n", checkpoint.Item1, checkpoint.Item2 - lastCheckpoint);
                lastCheckpoint = checkpoint.Item2;
            }
            sb.AppendFormat("Total time: {0}ms", _stopwatch.ElapsedMilliseconds);
            return sb.ToString();
        }
    }
}
