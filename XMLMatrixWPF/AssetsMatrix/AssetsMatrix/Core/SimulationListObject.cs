using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace AssetsMatrix.Core
{
    class SimulationListObject : IEditableCollectionView
    {
        public String SimulationName { get; set; }
        public String Count { get; set; }

        public SimulationListObject(string simulationName, string count)
        {
            SimulationName = simulationName;
            Count = count;
        }

        NewItemPlaceholderPosition IEditableCollectionView.NewItemPlaceholderPosition
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        bool IEditableCollectionView.CanAddNew
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IEditableCollectionView.IsAddingNew
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        object IEditableCollectionView.CurrentAddItem
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IEditableCollectionView.CanRemove
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IEditableCollectionView.CanCancelEdit
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool IEditableCollectionView.IsEditingItem
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        object IEditableCollectionView.CurrentEditItem
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        object IEditableCollectionView.AddNew()
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.CommitNew()
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.CancelNew()
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.Remove(object item)
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.EditItem(object item)
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.CommitEdit()
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.CancelEdit()
        {
            throw new NotImplementedException();
        }
    }
}
