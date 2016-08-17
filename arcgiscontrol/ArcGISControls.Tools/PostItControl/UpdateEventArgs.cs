using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControls.Tools.PostItControl
{
    public class UpdateEventArgs : EventArgs
    {
        public EventStatus Status { get; private set; }

        public EventSeverity Severity { get; private set; }

        public string Owner { get; private set; }

        public string Comment { get; private set; }

        public UpdateEventArgs(EventStatus status, EventSeverity severity, string owner, string comment)
        {
            this.Status = status;
            this.Severity = severity;
            this.Owner = owner;
            this.Comment = comment;
        }
    }
}
