
namespace Repositorio
{
    //internal abstract class SqlRepository<T> : IRepository<T> where T : class, new()
    //{
    //    private readonly string _connectionString;
    //    private readonly Func<IDataReader, T> _mapper;

    //    protected SqlRepository(string connectionstring, Func<IDataReader, T> mapper)
    //    {
    //        _connectionString = connectionstring;
    //        _mapper = mapper;
    //    }

    //    public void Add(T entity)
    //    {
    //        // ToDo: Refatorar - Como identificar as propriedades de cada tabela? como obter os valores que serão inseridos?
    //        ExecuteCommand(@$"
    //            INSERT INTO {typeof(T).Name} (Id, Titulo, Publicacao, Autores, Resumo) 
    //            VALUES (1, 'titulo', '2023-01-20', 'Ninguém', 'Resumo do livro' )");
    //    }

    //    public void Delete(int id)
    //    {
    //        // ToDo: Refatorar - nome do campo fixo
    //        ExecuteCommand($"DELETE FROM {nameof(T)} WHERE Id = @id", new SqlParameter("id", id));
    //    }

    //    public IEnumerable<T> GetAll()
    //    {
    //        return ExecuteReader($"SELECT * FROM {nameof(T)}");
    //    }

    //    public T? GetById(int id)
    //    {
    //        // ToDo: Refatorar - nome do campo fixo
    //        return ExecuteReader($"SELECT * FROM {nameof(T)} WHERE Id = @id", new SqlParameter("id", id)).FirstOrDefault();
    //    }

    //    public void Update(T entity)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    private IEnumerable<T> ExecuteReader(string commandText, params SqlParameter[] parameters)
    //    {
    //        using SqlConnection connection = new SqlConnection(_connectionString);
    //        using SqlCommand command = new SqlCommand(commandText, connection);

    //        if (parameters != null)
    //            command.Parameters.AddRange(parameters);

    //        connection.Open();

    //        using SqlDataReader reader = command.ExecuteReader();

    //        while (reader.Read())
    //        {
    //            yield return _mapper(reader);
    //        }
    //    }

    //    private void ExecuteCommand(string commandText, params SqlParameter[] parameters)
    //    {
    //        using SqlConnection connection = new SqlConnection(_connectionString);
    //        using SqlCommand command = new SqlCommand(commandText, connection);

    //        if (parameters != null)
    //            command.Parameters.AddRange(parameters);

    //        connection.Open();

    //        var result = command.ExecuteNonQuery();

    //        Console.WriteLine($"Registros afetados: {result}");
    //    }
    //}
}
