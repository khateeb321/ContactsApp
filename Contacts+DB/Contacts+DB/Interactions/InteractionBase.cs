using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using AP.ViewModel;
using LinqToVisualTree;
using System.Linq;
using System.Collections.ObjectModel;

namespace AP.Interactions
{
    /// <summary>
    /// A base class for interactions.
    /// </summary>
    public abstract class InteractionBase : IInteraction
    {
        private bool _isActive = false;

        protected ItemsControl _todoList;
        protected ObservableCollection<ToDoItemViewModel> _todoItems;
        protected ScrollViewer _scrollViewer;

        public virtual void Initialise(ItemsControl todoList, ObservableCollection<ToDoItemViewModel> todoItems)
        {
            _todoList = todoList;
            _todoItems = todoItems;

            // when the ItemsControl has been rendered, we can locate the ScrollViewer
            // that is within its template.
            

            IsEnabled = true;
        }

        protected virtual void ScrollViewerLocated(ScrollViewer scrollViewer)
        {
        }

        public virtual void AddElement(FrameworkElement rootElement)
        {
        }

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;

                if (_isActive == true)
                {
                    if (Activated != null)
                    {
                        Activated(this, EventArgs.Empty);
                    }
                }
                else
                {
                    if (DeActivated != null)
                    {
                        DeActivated(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool IsEnabled { get; set; }

        public event EventHandler Activated;

        public event EventHandler DeActivated;
    }
}
