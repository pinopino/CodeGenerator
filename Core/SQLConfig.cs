using System.Collections.Generic;

namespace Generator.Core
{
    public class TableMetaData
    {
        private string _name;
        private string _comment;
        private bool _isRefed;
        private List<ColumnMetaData> _primaryKey;
        private List<ForeignKeyMetaData> _foreignKey;
        private ColumnMetaData _ideneity;
        private List<ColumnMetaData> _columns;
        private List<ColumnMetaData> _existPredicate;
        private List<ColumnMetaData> _wherePredicate;
        private List<RefTableMetaData> _referenceTable;

        /// <summary>
        /// 
        /// </summary>
        public TableMetaData()
        {
            _name = string.Empty;
            _comment = string.Empty;
            _isRefed = false;
            _primaryKey = new List<ColumnMetaData>();
            _foreignKey = new List<ForeignKeyMetaData>();
            _columns = new List<ColumnMetaData>();
            _existPredicate = new List<ColumnMetaData>();
            _wherePredicate = new List<ColumnMetaData>();
            _referenceTable = new List<RefTableMetaData>();
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
        protected List<ForeignKeyMetaData> ForeignKey
        {
            set { this._foreignKey = value; }
            get { return this._foreignKey; }
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

        /// <summary>
        /// 
        /// </summary>
        public List<ColumnMetaData> ExistPredicate
        {
            set { this._existPredicate = value; }
            get { return this._existPredicate; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<ColumnMetaData> WherePredicate
        {
            set { this._wherePredicate = value; }
            get { return this._wherePredicate; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected List<RefTableMetaData> ReferenceTable
        {
            set { this._referenceTable = value; }
            get { return this._referenceTable; }
        }
    }

    public class RefTableMetaData
    {
        private string _name;
        private List<ForeignKeyMetaData> _foreignKey;

        public RefTableMetaData()
        {
            _name = string.Empty;
            _foreignKey = new List<ForeignKeyMetaData>();
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
        public List<ForeignKeyMetaData> ForeignKey
        {
            set { this._foreignKey = value; }
            get { return this._foreignKey; }
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

    public class ForeignKeyMetaData
    {
        private string _fromname;
        private string _toname;
        private string _dbType;

        public ForeignKeyMetaData()
        {
            _fromname = string.Empty;
            _toname = string.Empty;
            _dbType = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public string FromName
        {
            set { this._fromname = value; }
            get { return this._fromname; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ToName
        {
            set { this._toname = value; }
            get { return this._toname; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DbType
        {
            set { this._dbType = value; }
            get { return this._dbType; }
        }
    }
}
