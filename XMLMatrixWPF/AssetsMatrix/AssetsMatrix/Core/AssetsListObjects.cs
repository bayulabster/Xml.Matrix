using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;

namespace AssetsMatrix.Core
{
    public class AssetsListObjects : IEditableCollectionView
    {
        public Uri ImageFilepath { get; set; }
        public string GameObjectId { get; set; }
        public string Unity3DPackName { get; set; }
        public string SourceId { get; set; }
        public string ToolTip { get; set; }
        public string ExtendedTooltip { get; set; }
        public string ImportPath { get; set; }

        public AssetsListObjects(string imageURL, string gameObjectId, string unity3DPackName, string sourceId, string tooltip, string extendedTooltip, string importPath) : base()
        {

            if (!string.IsNullOrEmpty(imageURL))
            {
                ImageFilepath = new Uri(imageURL);
            }
            GameObjectId = gameObjectId;
            Unity3DPackName = unity3DPackName;
            SourceId = sourceId;
            ToolTip = tooltip;
            ExtendedTooltip = extendedTooltip;
            ImportPath = importPath;
        }

        bool IEditableCollectionView.CanAddNew
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

        bool IEditableCollectionView.CanRemove
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

        object IEditableCollectionView.CurrentEditItem
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

        bool IEditableCollectionView.IsEditingItem
        {
            get
            {
                throw new NotImplementedException();
            }
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

        object IEditableCollectionView.AddNew()
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.CancelEdit()
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.CancelNew()
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.CommitEdit()
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.CommitNew()
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.EditItem(object item)
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.Remove(object item)
        {
            throw new NotImplementedException();
        }

        void IEditableCollectionView.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}
