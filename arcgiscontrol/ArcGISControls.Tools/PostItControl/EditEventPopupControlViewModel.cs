
namespace ArcGISControls.Tools.PostItControl
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Windows.Input;
    using ArcGISControl.Helper;

    public class EditEventPopupControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        #region Fields

        #region Events

        public event EventHandler<UpdateEventArgs> eUpdateClick;

        public event EventHandler<EventArgs> eCloseEditEventPopup;

        #endregion

        private bool isEditEventPopupVisible;

        private EventStatus selectedStatus;

        private EventSeverity selectedUrgency;

        private string selectedOwner;

        private string comment;

        private readonly ObservableCollection<string> owners = new ObservableCollection<string>();

        #endregion

        #region Properties

        public bool IsEditEventPopupVisible
        {
            get { return this.isEditEventPopupVisible; }
            set
            {
                this.isEditEventPopupVisible = value;
                this.OnPropertyChanged("IsEditEventPopupVisible");
            }
        }

        public EventStatus SelectedStatus
        {
            get { return this.selectedStatus; }
            set
            {
                this.selectedStatus = value;
                this.OnPropertyChanged("SelectedStatus");
            }
        }

        public EventSeverity SelectedUrgency
        {
            get { return this.selectedUrgency; }
            set
            {
                this.selectedUrgency = value;
                this.OnPropertyChanged("SelectedUrgency");
            }
        }

        public ObservableCollection<string> Owners
        {
            get { return this.owners; }
        }

        public string SelectedOwner
        {
            get { return this.selectedOwner; }
            set
            {
                this.selectedOwner = value;
                this.OnPropertyChanged("SelectedOwner");
            }
        }

        public string Comment
        {
            get { return this.comment; }
            set
            {
                this.comment = value;
                this.OnPropertyChanged("Comment");
            }
        }

        #endregion

        #region Methods

        public void SetPropertie(DataTable resultTable)
        {
            if (resultTable.Rows.Count == 0)
                return;

            // Table Schema
            //resultTable.Columns.Add("status", typeof(string));
            //resultTable.Columns.Add("severity", typeof(string));
            //resultTable.Columns.Add("owner", typeof(string));
            //resultTable.Columns.Add("comment", typeof(string));

            // Mockup columnName 미정.
            var dataRow = resultTable.Rows[0];

            this.SelectedStatus = (EventStatus)Enum.Parse(typeof(EventStatus), dataRow["status"].ToString(), true);
            this.SelectedUrgency = (EventSeverity)Enum.Parse(typeof(EventSeverity), dataRow["severity"].ToString(), true);
            this.SelectedOwner = dataRow["owner"].ToString();
            this.Comment = dataRow["comment"].ToString();
        }

        #endregion

        #region Commands

        #region UpdateCommand

        private RelayCommand updateCommand;

        public ICommand UpdateCommand
        {
            get
            {
                return this.updateCommand ??
                       (this.updateCommand = new RelayCommand(param => this.ExecuteUpdateCommand()));
            }
        }

        private void ExecuteUpdateCommand()
        {
            var update = this.eUpdateClick;
            if (update != null)
            {
                update(this, new UpdateEventArgs(this.SelectedStatus, this.SelectedUrgency, this.SelectedOwner, this.Comment));
            }

            this.HideControl();
        }

        #endregion

        #region CloseCommand

        private RelayCommand closeCommand;

        public ICommand CloseCommand
        {
            get { return this.closeCommand ?? (this.closeCommand = new RelayCommand(param => this.ExecuteCloseCommand())); }
        }

        private void ExecuteCloseCommand()
        {
            this.HideControl();
        }

        private void Clear()
        {
            this.SelectedStatus = EventStatus.New;
            this.SelectedUrgency = EventSeverity.Low;
            this.SelectedOwner = null;
            this.Comment = null;
        }

        private void HideControl()
        {
            this.IsEditEventPopupVisible = false;
            this.Clear();

            var close = this.eCloseEditEventPopup;
            if (close != null)
            {
                close(this, new EventArgs());
            }
        }

        #endregion

        #endregion

        #region

        public void Dispose()
        {
            this.Owners.Clear();

            if (this.eUpdateClick != null)
            {
                foreach (EventHandler<UpdateEventArgs> handler in this.eUpdateClick.GetInvocationList())
                {
                    this.eUpdateClick -= handler;
                }
            }
        }

        #endregion
    }
}
