using System.Collections.Generic;

namespace Generator.Core
{
    public class SQLMetaData
    {
        private string _dal_headerNote;
        private List<string> _dal_using;
        private string _dal_namespace;
        private string _dal_classNamePrefix;
        private string _dal_classNameSuffix;
        private string _dal_baseClass;
        private List<string> _dal_methods;

        private string _model_headerNote;
        private List<string> _model_using;
        private string _model_namespace;
        private string _model_classNamePrefix;
        private string _model_classNameSuffix;
        private string _model_baseClass;

        private string _partial_check_dal_path;
        private List<TableMetaData> _tables;
        private List<string> _exceptTables;

        public SQLMetaData()
        {
            _dal_headerNote = string.Empty;
            _dal_using = new List<string>();
            _dal_namespace = string.Empty;
            _dal_classNamePrefix = string.Empty;
            _dal_classNameSuffix = string.Empty;
            _dal_baseClass = string.Empty;
            _dal_methods = new List<string>();

            _model_headerNote = string.Empty;
            _model_using = new List<string>();
            _model_namespace = string.Empty;
            _model_classNamePrefix = string.Empty;
            _model_classNameSuffix = string.Empty;
            _model_baseClass = string.Empty;

            _tables = new List<TableMetaData>();
            _exceptTables = new List<string>();
        }

        public TableMetaData this[string TableName]
        {
            get
            {
                return this._tables.Find(p => p.Name.ToLower() == TableName.ToLower());
            }
        }

        #region DAL
        /// <summary>
        /// 
        /// </summary>
        public string DAL_HeaderNote
        {
            set { this._dal_headerNote = value; }
            get { return this._dal_headerNote; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> DAL_Using
        {
            set { this._dal_using = value; }
            get { return this._dal_using; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DAL_Namespace
        {
            set { this._dal_namespace = value; }
            get { return this._dal_namespace; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DAL_ClassNamePrefix
        {
            set { this._dal_classNamePrefix = value; }
            get { return this._dal_classNamePrefix; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DAL_ClassNameSuffix
        {
            set { this._dal_classNameSuffix = value; }
            get { return this._dal_classNameSuffix; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DAL_BaseClass
        {
            set { this._dal_baseClass = value; }
            get { return this._dal_baseClass; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> DAL_Methods
        {
            set { this._dal_methods = value; }
            get { return this._dal_methods; }
        }
        #endregion

        #region Model
        /// <summary>
        /// 
        /// </summary>
        public string Model_HeaderNote
        {
            set { this._model_headerNote = value; }
            get { return this._model_headerNote; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Model_Using
        {
            set { this._model_using = value; }
            get { return this._model_using; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Model_Namespace
        {
            set { this._model_namespace = value; }
            get { return this._model_namespace; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Model_ClassNamePrefix
        {
            set { this._model_classNamePrefix = value; }
            get { return this._model_classNamePrefix; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Model_ClassNameSuffix
        {
            set { this._model_classNameSuffix = value; }
            get { return this._model_classNameSuffix; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Model_BaseClass
        {
            set { this._model_baseClass = value; }
            get { return this._model_baseClass; }
        }
        #endregion

        public List<TableMetaData> Tables
        {
            set { this._tables = value; }
            get { return this._tables; }
        }

        public List<string> ExceptTables
        {
            set { this._exceptTables = value; }
            get { return this._exceptTables; }
        }

        public string PartialCheck_DAL_Path
        {
            set { this._partial_check_dal_path = value; }
            get { return this._partial_check_dal_path; }
        }
    }

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
        private string _name;
        private string _dbType;
        private string _comment;
        private bool _isPrimaryKey;
        private bool _isIdentity;
        private bool _nullable;
        private bool _hasDefaultValue;

        public ColumnMetaData()
        {
            _name = string.Empty;
            _dbType = string.Empty;
            _comment = string.Empty;
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
