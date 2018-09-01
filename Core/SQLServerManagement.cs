using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;

namespace Generator.Core
{
    public class SQLServerManagement : IDisposable
    {
        private bool _disposed = false;
        private string _connectionStr;
        private ServerConnection _srvConn;
        private Server _srv;

        public SQLServerManagement(string connStr)
        {
            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new ArgumentNullException("connStr");
            }
            _connectionStr = connStr;
            _srvConn = new ServerConnection(new System.Data.SqlClient.SqlConnection(connStr));
            _srv = new Server(_srvConn);
        }

        public void Dispose()
        {
            //必须为true
            Dispose(true);
            //通知垃圾回收机制不再调用终结器（析构器）
            GC.SuppressFinalize(this);
        }

        ~SQLServerManagement()
        {
            //必须为false
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                // 清理托管资源
                _srvConn.Disconnect();
            }

            // 清理非托管资源

            //让类型知道自己已经被释放
            _disposed = true;
        }

        public DatabaseCollection Databases
        {
            get
            {
                return this._srv.Databases;
            }
        }

        public void Close()
        {
            this._srvConn.Disconnect();
        }
    }
}
