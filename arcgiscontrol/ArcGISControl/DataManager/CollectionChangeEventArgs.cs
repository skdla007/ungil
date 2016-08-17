namespace ArcGISControl.DataManager
{
    using System;
    using System.ComponentModel;

    public class CollectionChangeEventArgs<T> : EventArgs
    {
        private CollectionChangeAction action;
        private T element;

        public virtual CollectionChangeAction Action
        {
            get
            {
                return this.action;
            }
        }

        public virtual T Element
        {
            get
            {
                return this.element;
            }
        }

        public CollectionChangeEventArgs(CollectionChangeAction action, T element)
        {
            this.action = action;
            this.element = element;
        }
    }
}
