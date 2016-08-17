using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ArcGISControl.Helper
{
    /// <code>
    /// void Foo()
    /// {
    ///     using(var sl = new StopwatchLogger("Foo"))
    ///     {
    ///         // blah
    ///         sl.Stamp();
    ///         // blah
    ///         sl.Stamp();
    ///         // blah
    ///     }
    /// }
    /// </code>
    public class StopwatchLogger : IDisposable
    {
        private Stopwatch stopwatch;

        private string tag;

        private int count;

        public StopwatchLogger(string tag)
        {
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();

            this.tag = tag;

            Debug.WriteLine(
                "Begin {0} {1}",
                this.stopwatch.GetHashCode(),
                this.tag
            );
        }

        public void Stamp()
        {
            Debug.WriteLine(
                "Stamp {0} {1} {2} {3}ms",
                this.stopwatch.GetHashCode(),
                this.tag,
                ++count,
                this.stopwatch.ElapsedMilliseconds
            );
        }

        public void Dispose()
        {
            this.stopwatch.Stop();

            Debug.WriteLine(
                "End {0} {1} {2}ms",
                this.stopwatch.GetHashCode(),
                this.tag,
                this.stopwatch.ElapsedMilliseconds
            );
        }
    }
}
