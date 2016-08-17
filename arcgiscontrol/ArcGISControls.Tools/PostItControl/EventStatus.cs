using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ArcGISControls.Tools.PostItControl
{
    public enum EventStatus
    {
        [Description("NEW")]
        New,

        [Description("IN PROGRESS")]
        InProgress,

        [Description("PENDING")]
        Pending,

        [Description("RESOLVED")]
        Resolved,

        [Description("CLOSED")]
        Closed
    }

    public enum EventSeverity
    {
        [Description("LOW")]
        Low,

        [Description("MEDIUM")]
        Medium,

        [Description("INFORMATION")]
        Information,

        [Description("HIGH")]
        High,

        [Description("CRITICAL")]
        Critical
    }
}
