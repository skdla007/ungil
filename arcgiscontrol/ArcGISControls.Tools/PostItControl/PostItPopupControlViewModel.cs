using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGISControl.Helper;

namespace ArcGISControls.Tools.PostItControl
{
    public class PostItPopupControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        #region Fields

        #region Events

        public event EventHandler<EventArgs> eGoLinkedMapClick;

        public event EventHandler<EventArgs> eEditEventClick;

        #endregion

        private bool isPostItPopupVisible;

        private string title;

        private string body;

        #endregion

        #region Properties

        public bool IsPostItPopupVisible
        {
            get { return this.isPostItPopupVisible; }
            set
            {
                this.isPostItPopupVisible = value;
                this.OnPropertyChanged("IsPostItPopupVisible");
            }
        }

        public string Title
        {
            get { return this.title; }
            set
            {
                this.title = value;
                this.OnPropertyChanged("Title");
            }
        }

        public string Body
        {
            get { return this.body; }
            set
            {
                this.body = value;
                this.OnPropertyChanged("Body");
            }
        }

        /// <summary>
        /// EditEvent 버튼 클릭 후 Status 값 받아오는 부분(비동기로 실행됨) 완료 여부.
        /// Status 값 수신이 완료 되기 전까지 EditEvent 버튼은 비활성화 되어야 함.
        /// </summary>
        public bool IsReady { get; set; }

        #endregion

        #region GoCommand

        private RelayCommand goCommand;

        public ICommand GoCommand
        {
            get { return this.goCommand ?? (this.goCommand = new RelayCommand(param => this.ExecuteGoCommand())); }
        }

        private void ExecuteGoCommand()
        {
            var goEvent = this.eGoLinkedMapClick;
            if (goEvent != null)
            {
                goEvent(this, new EventArgs());
            }

            // 직접 닫기 전까지 닫지 않는다.
            //this.HideControl();
        }

        #endregion

        #region EditEventCommand

        private RelayCommand editEventCommand;

        public ICommand EditEventCommand
        {
            get { return this.editEventCommand ?? (this.editEventCommand = new RelayCommand(param => this.ExecuteEditEventCommand(), param => this.CanExecuteEditEventCommand())); }
        }

        private bool CanExecuteEditEventCommand()
        {
            return this.IsReady;
        }

        private void ExecuteEditEventCommand()
        {
            var editEvent = this.eEditEventClick;
            if (editEvent != null)
            {
                editEvent(this, new EventArgs());
            }
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

        private void HideControl()
        {
            this.IsPostItPopupVisible = false;
            this.Title = null;
            this.Body = null;
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            if (this.eEditEventClick != null)
            {
                foreach (EventHandler<EventArgs> handler in this.eEditEventClick.GetInvocationList())
                {
                    this.eEditEventClick -= handler;
                }
            }

            if (this.eGoLinkedMapClick != null)
            {
                foreach (EventHandler<EventArgs> handler in this.eGoLinkedMapClick.GetInvocationList())
                {
                    this.eGoLinkedMapClick -= handler;
                }
            }
        }

        #endregion

    }
}
