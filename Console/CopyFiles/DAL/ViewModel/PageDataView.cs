using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPA.DataLayer
{
    public class PageDataView<T>
    {
        private int _totalRecords;
        public PageDataView()
        {
            this._Items = new List<T>();
        }

        public int TotalRecords
        {
            get { return _totalRecords; }
            set { _totalRecords = value; }
        }

        private IList<T> _Items;
        public IList<T> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
