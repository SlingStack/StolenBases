using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StolenBasesLib.Connections
{
    public enum ConnectionType
    {
        OnBase,
        Sql,
        Supabase
    }

    public class Connection
    {
        public string? Name { get; set; }
        public bool Default { get; set; }
        public dynamic Value { get; set; }
        public ConnectionType ConnectionType { get; set; }

        public Connection(string name, dynamic value, ConnectionType type, bool isDefault = false)
        {
            Name = name;
            Value = value;
            ConnectionType = type;
            Default = isDefault;
        }

        //public dynamic GetConnectionAsType()
        //{
        //    switch (ConnectionType)
        //    {
        //        case ConnectionType.OnBase:
        //            return (OnBaseDB)Value;
        //        case ConnectionType.Sql:
        //            return (SqlServer)Value;
        //        case ConnectionType.Supabase:
        //            return (SupabaseConnection)Value;
        //        default:
        //            throw new Exception("Invalid connection type. Cannot return connection as type.");
        //    }
        //}
    }
}
