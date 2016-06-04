using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MongoDB.Bson;
using Oracle.DataAccess.Client;



namespace PayID.API
{
    public class DynamicOracle
    {
        public string ConnectionString = String.Empty;
        private static IDbConnection connection = null;

        private IDbConnection _getConnection()
        {
            if (connection == null || connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                connection = new OracleConnection(ConnectionString);
                connection.Open();
            }
            return connection;
        }

        private System.Data.IDbCommand _gen_insert_dynamic(string name, PayID.DataHelper.DynamicObj dynamic)
        {
            //Chuoi command text
            string _str_command = String.Empty;
            string _str_fields = String.Empty;
            string _str_values = String.Empty;

            string[] _keys = dynamic.dictionary.Keys.ToArray<string>();

            foreach (string key in _keys)
            {
                _str_fields += ((key == "_id") ? "id" : key) + ",";
                _str_values += _value_string(dynamic.dictionary[key]) + ",";
            }
            _str_fields += "month_key, date_key";
            _str_fields = "(" + _str_fields + ")";

            _str_values += DateTime.Now.ToString("yyyyMM") + "," + DateTime.Now.ToString("yyyyMMdd");
            _str_values = "(" + _str_values + ")";

            _str_command = "insert into " + name + " " + _str_fields + " values " + _str_values;

            return new OracleCommand(_str_command);
        }
        private System.Data.IDbCommand _gen_update_dynamic(string name, PayID.DataHelper.DynamicObj dynamic)
        {
            //Chuoi command text
            string _str_command = String.Empty;
            List<string> _set_values = new List<string>();
            string[] _keys = dynamic.dictionary.Keys.ToArray<string>();

            foreach (string key in _keys)
                if (key != "_id")
                {
                    _set_values.Add(key + " = " + _value_string(dynamic.dictionary[key]));
                }

            _str_command = String.Join(", ", _set_values.ToArray());
            _str_command = "update " + name + " set " + _str_command + " where id =" + _value_string(dynamic.dictionary["_id"]);
            return new OracleCommand(_str_command);
        }

        public string Insert(string objectName, dynamic dynamicObject)
        {
            IDbCommand _command = _gen_insert_dynamic(objectName, dynamicObject);
            _command.Connection = _getConnection();
            _command.ExecuteNonQuery();
            return dynamicObject._id.ToString();
        }

        public string Update(string objectName, dynamic dynamicObject)
        {
            IDbCommand _command = _gen_update_dynamic(objectName, dynamicObject);
            _command.Connection = _getConnection();
            _command.ExecuteNonQuery();
            return dynamicObject._id.ToString();
        }

        public dynamic Get(string objectName, string whereClause)
        {
            if (!String.IsNullOrEmpty(whereClause)) whereClause = "and (" + whereClause + ")";
            IDbCommand _command = new OracleCommand("select * from " + objectName + " where 1 = 1 " + whereClause, (OracleConnection)_getConnection());
            IDataReader _reader = _command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(_reader);
            if (dt.Rows.Count > 0)
                return _gen_dynamic_from_row(dt.Rows[0]);
            return null;
        }

        public dynamic Get(string objectName, string[] fields, string whereClause)
        {
            string _fields = string.Empty;
            _fields = String.Join(", ", fields);
            if (!String.IsNullOrEmpty(whereClause)) whereClause = "and (" + whereClause + ")";
            IDbCommand _command = new OracleCommand("select " + _fields + " from " + objectName + " where 1 = 1 " + whereClause, (OracleConnection)_getConnection());
            IDataReader _reader = _command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(_reader);
            if (dt.Rows.Count > 0)
                return _gen_dynamic_from_row(dt.Rows[0]);
            return null;
        }

        public dynamic[] List(string objectName, string[] fields, string whereClause)
        {
            List<dynamic> list = new List<dynamic>();
            string _fields = string.Empty;
            _fields = String.Join(", ", fields);
            if (!String.IsNullOrEmpty(whereClause)) whereClause = "and (" + whereClause + ")";
            IDbCommand _command = new OracleCommand("select " + _fields + " from " + objectName + " where 1 = 1 " + whereClause, (OracleConnection)_getConnection());
            IDataReader _reader = _command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(_reader);
            foreach (DataRow row in dt.Rows)
                list.Add(_gen_dynamic_from_row(row));
            return list.ToArray();
        }
        public dynamic[] List(string objectName, string whereClause)
        {
            List<dynamic> list = new List<dynamic>();
            if (!String.IsNullOrEmpty(whereClause)) whereClause = "and (" + whereClause + ")";
            IDbCommand _command = new OracleCommand("select * from " + objectName + " where 1 = 1 " + whereClause, (OracleConnection)_getConnection());
            IDataReader _reader = _command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(_reader);
            foreach (DataRow row in dt.Rows)
                list.Add(_gen_dynamic_from_row(row));

            return list.ToArray();
        }

        private dynamic _gen_dynamic_from_row(DataRow row)
        {
            PayID.DataHelper.DynamicObj dynamic = new PayID.DataHelper.DynamicObj();
            List<string> keys = new List<string>();
            foreach (DataColumn col in row.Table.Columns)
            {
                dynamic.dictionary.Add(col.ColumnName.ToLower(), row[col.ColumnName]);
            }
            return dynamic;
        }
        string _value_string(object p)
        {
            try
            {
                if(String.IsNullOrEmpty(p.ToString())) return "null";
                if (p is String)
                    return @"'" + p.ToString().Trim() + "'";
                if (p is DateTime)
                    return "TO_DATE('" + ((DateTime)p).ToString("yyyy/MM/dd HH:mm:ss") + "', 'yyyy/mm/dd hh24:mi:ss')";
                return p.ToString().Replace(',','.');
            }
            catch
            {
                return "null";
            }
        }
    }
}