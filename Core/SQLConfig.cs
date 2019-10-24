using System.Collections.Generic;

namespace Generator.Core
{
    public class TableMetaData
    {
        private string _name;
        private string _comment;
        private bool _isRefed;
        private List<ColumnMetaData> _primaryKey;
        private ColumnMetaData _ideneity;
        private List<ColumnMetaData> _columns;

        /// <summary>
        /// 
        /// </summary>
        public TableMetaData()
        {
            _name = string.Empty;
            _comment = string.Empty;
            _isRefed = false;
            _primaryKey = new List<ColumnMetaData>();
            _columns = new List<ColumnMetaData>();
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            set { this._name = value; }
            get { return this._name; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Comment
        {
            set { this._comment = value; }
            get { return this._comment; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected bool IsRefed
        {
            set { this._isRefed = value; }
            get { return this._isRefed; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<ColumnMetaData> PrimaryKey
        {
            set { this._primaryKey = value; }
            get { return this._primaryKey; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ColumnMetaData Identity
        {
            set { this._ideneity = value; }
            get { return this._ideneity; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<ColumnMetaData> Columns
        {
            set { this._columns = value; }
            get { return this._columns; }
        }
    }

    public class ColumnMetaData
    {
        private string _tableName;
        private string _name;
        private string _dbType;
        private string _comment;
        private bool _isPrimaryKey;
        private bool _isIdentity;
        private bool _nullable;
        private bool _hasDefaultValue;

        public ColumnMetaData()
        {
            _tableName = string.Empty;
            _name = string.Empty;
            _dbType = string.Empty;
            _comment = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public string TableName
        {
            set { this._tableName = value; }
            get { return this._tableName; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            set { this._name = value; }
            get { return this._name; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DbType
        {
            set { this._dbType = value; }
            get { return this._dbType; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Comment
        {
            set { this._comment = value; }
            get { return this._comment; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPrimaryKey
        {
            set { this._isPrimaryKey = value; }
            get { return this._isPrimaryKey; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIdentity
        {
            set { this._isIdentity = value; }
            get { return this._isIdentity; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Nullable
        {
            set { this._nullable = value; }
            get { return this._nullable; }
        }

        public bool HasDefaultValue
        {
            set { this._hasDefaultValue = value; }
            get { return this._hasDefaultValue; }
        }
    }
}
