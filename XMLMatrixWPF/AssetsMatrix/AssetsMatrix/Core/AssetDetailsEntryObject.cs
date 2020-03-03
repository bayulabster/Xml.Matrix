using System;
using System.ComponentModel;

public class AssetDetailsEntryObject : IEditableCollectionView
{
    public string key_name { get;  set; }    
    public string content_name { get;  set; }
    public bool Is_text { get;  set; }
    public Uri ImageFilePath { get; set; }

    public AssetDetailsEntryObject(string imageURL, string keyname, string contentname, bool contentIstext=true)
    {
        if(!string.IsNullOrEmpty(imageURL) && contentIstext == false)
        {
            ImageFilePath = new Uri(imageURL);
        }
        else
        {
            content_name = contentname;
        }

        key_name = keyname;
        
       Is_text = contentIstext;
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
