using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetMatrixConsoleApp
{
    class ListViewLabNameSorter : IComparer
    {
        private int _ColumnToSort;
        private SortOrder _OrderOfSort;
        private CaseInsensitiveComparer _ObjectCompare;

        public ListViewLabNameSorter()
        {
            _ColumnToSort = 0;

            _OrderOfSort = SortOrder.None;

            _ObjectCompare = new CaseInsensitiveComparer();
        }

        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listViewX, listViewY;

            listViewX = (ListViewItem)x;
            listViewY = (ListViewItem)y;

            compareResult = _ObjectCompare.Compare(listViewX.SubItems[_ColumnToSort].Text, listViewY.SubItems[_ColumnToSort].Text);

            if(_OrderOfSort == SortOrder.Ascending)
            {
                return compareResult;
            }
            else if(_OrderOfSort == SortOrder.Descending)
            {
                return (-compareResult);
            }
            else
            {
                return 0;
            }
        }

        public int SortColumn
        {
            set
            {
                _ColumnToSort = value;
            }

            get
            {
                return _ColumnToSort;
            }
        }

        public SortOrder Order
        {
            set
            {
                _OrderOfSort = value;
            }

            get
            {
                return _OrderOfSort;
            }
        }
    }
}
