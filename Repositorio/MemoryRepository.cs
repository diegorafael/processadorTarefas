using Microsoft.Extensions.Configuration;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;

namespace Repositorio
{
    public class MemoryRepository : IRepository<Tarefa>
    {
        private List<Tarefa> InicializarDatabase()
        {
            const int NumeroTarefas = 10;

            var result = new List<Tarefa>();

            for (int i = 0; i < NumeroTarefas; i++)
                result.Add(Tarefa.Criar(i + 1, _configs));

            return result;
        }

        private static List<Tarefa>? _staticDb;
        private readonly IConfiguration _configs;

        private List<Tarefa> _db 
        { 
            get 
            { 
                if(_staticDb == null)
                    _staticDb = InicializarDatabase();

                return _staticDb;
            }

            set 
            { 
                _staticDb = value;
            }
        }
        public MemoryRepository(IConfiguration configuration)
        {
            _configs = configuration;
        }

        public void Add(Tarefa entity)
        {
            _db.Add(entity);
        }

        public void Delete(int id)
        {
            _db.Remove(GetById(id)!);
        }

        public IEnumerable<Tarefa> GetAll()
        {
            return _db;
        }

        public Tarefa? GetById(int id)
        {
            return _db.FirstOrDefault(t => t.Id == id);
        }

        public void Update(Tarefa entity)
        {
            // Nada a fazer. A referência de memória para o objeto é mantida 
        }
    }
}
