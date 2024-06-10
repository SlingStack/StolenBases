using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StolenBasesLib.Connections;

namespace StolenBasesLib
{
	public static class ConnectionManager
	{
		public static List<Connection> ConfiguredConnections { get; private set; } = new List<Connection>();

		public static void AddConnection(Connection connection)
		{
			ConfiguredConnections.Add(connection);
		}

		public static bool TryGetConnection(string name, out Connection connection)
		{
			connection = ConfiguredConnections.FirstOrDefault(a => a.Name == name)!;
			if (connection == null) { return false; }
			return true;
		}

		public static bool TryGetDefaultConnection<T>(out Connection connection)
		{
			connection = ConfiguredConnections.FirstOrDefault(a => a.GetType() == typeof(T) && a.Default)!;
			if(connection == null) { return false; }
			return true;
		}
	}
}
