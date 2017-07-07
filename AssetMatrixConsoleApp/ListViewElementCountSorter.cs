using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetMatrixConsoleApp
{
    class ListViewElementCountSorter : IComparer
    {
        private int _ColumnToSort;
        private SortOrder _OrderOfSort;
        

        public ListViewElementCountSorter()
        {
            _ColumnToSort = 1;
            _OrderOfSort = SortOrder.None;
            
        }

        public int Compare(object x, object y)
        {
            int compareResult = 0;
            ListViewItem listViewX, listViewY;

            listViewX = (ListViewItem)x;
            listViewY = (ListViewItem)y;

            string compareX = listViewX.SubItems[_ColumnToSort].Text;
            string compareY = listViewY.SubItems[_ColumnToSort].Text;

            int xValue = 0;
            int yValue = 1;
            if(!int.TryParse(compareX, out xValue) || !int.TryParse(compareY, out yValue))
            {
                return 0;
            }

            compareResult = xValue.CompareTo(yValue);
            
            if (_OrderOfSort == SortOrder.Descending)
            {
                compareResult *= -1;
            }
            else
            {
                compareResult = 0;
            }

            return compareResult;
        }

        public int SortColumn
        {
            get
            {
                return _ColumnToSort;
            }

            set
            {
                _ColumnToSort = value;
            }
        }

        public SortOrder Order
        {
            get
            {
                return _OrderOfSort;
            }

            set
            {
                _OrderOfSort = value;
            }
        }
    }
}
