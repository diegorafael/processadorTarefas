using Dapper;
using Microsoft.Data.Sqlite;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;
using System.Data;
using System.Reflection;


namespace Repositorio
{
    public class SqliteRepository<T> : IRepository<T> where T : IEntidade
    {
        private readonly string _connectionString;

        public SqliteRepository(string connectionstring)
        {
            _connectionString = connectionstring;

            using IDbConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            var tableName = typeof(T).Name;

            if (!TableExists(connection, tableName))
            {
                var createTableQuery = GetCreateTableQuery(tableName);
                connection.Execute(createTableQuery);
            }
            
        }

        public void Add(T entity)
        {
            var objType = typeof(T);
            var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            ExecuteCommand(@$"
                INSERT INTO {typeof(T).Name} ({string.Join(", ",properties.Select(p => p.Name))}) 
                VALUES ({string.Join(", ", properties.Select(p => $"@{p.Name}"))})", entity);
        }

        public void Delete(int id)
        {
            ExecuteCommand($"DELETE FROM {typeof(T).Name} WHERE {nameof(IEntidade.Id)} = @{nameof(IEntidade.Id)}", new SqliteParameter(nameof(IEntidade.Id), id));
        }

        public IEnumerable<T> GetAll()
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection.Query<T>($"SELECT * FROM {typeof(T).Name}");
        }

        public T? GetById(int id)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection.QueryFirstOrDefault<T>($"SELECT * FROM {typeof(T).Name}s WHERE {nameof(IEntidade.Id)} = @{nameof(IEntidade.Id)}", new { Id = id });
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        private void ExecuteCommand(string commandText, T entity)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            
            connection.Open();
            connection.Execute(commandText, entity);
        }

        private void ExecuteCommand(string commandText, params SqliteParameter[] parameters)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            using SqliteCommand command = new SqliteCommand(commandText, connection);

            if (parameters != null)
                command.Parameters.AddRange(parameters);

            connection.Open();

            var result = command.ExecuteNonQuery();
        }


        private bool TableExists(IDbConnection dbConnection, string tableName)
        {
            var tableQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name=@TableName";
            var result = dbConnection.Query<string>(tableQuery, new { TableName = tableName });
            return result.Any();
        }

        private string GetCreateTableQuery(string tableName)
        {
            var columns = typeof(T).GetProperties()
                .Select(property =>
                {
                    var columnName = property.Name;
                    var columnType = GetColumnType(property.PropertyType);
                    return $"{columnName} {columnType}";
                });

            var createTableQuery = $"CREATE TABLE {tableName} ({nameof(IEntidade.Id)} INTEGER PRIMARY KEY, {string.Join(", ", columns)})";
            return createTableQuery;
        }

        private string GetColumnType(Type propertyType)
        {
            if (propertyType == typeof(int))
                return "INTEGER";
            if (propertyType == typeof(DateTime))
                return "DATETIME";
            if (propertyType == typeof(DateTime?))
                return "DATETIME";
            if (propertyType == typeof(string))
                return "TEXT";
            if (propertyType == typeof(EstadoTarefa))
                return "INTEGER";

            throw new NotSupportedException($"Tipo de propriedade não suportado: {propertyType.Name}");
        }

    }
}
