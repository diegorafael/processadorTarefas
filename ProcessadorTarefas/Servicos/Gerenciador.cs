using Microsoft.Extensions.Configuration;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;

namespace ProcessadorTarefas.Servicos
{
    public interface IGerenciadorTarefas
    {
        Task<Tarefa> Criar();
        Task<Tarefa> Consultar(int idTarefa);
        Task Cancelar(int idTarefa);
        Task<IEnumerable<Tarefa>> ListarAtivas();
        Task<IEnumerable<Tarefa>> ListarInativas();
    }

    public class Gerenciador : IGerenciadorTarefas
    {
        private const int minDuracaoSubtarefa = 3;
        private const int maxDuracaoSubtarefa = 10;
        private const int minSubtarefas = 1;
        private const int maxSubtarefas = 10;

        private readonly IRepository<Tarefa> _tarefaRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configs;
        private static Random random = new Random();
        
        private readonly EstadoTarefa[] estadosAtivos = [
                    EstadoTarefa.Criada,
                    EstadoTarefa.EmExecucao,
                    EstadoTarefa.Agendada,
                    EstadoTarefa.EmPausa
                ];
        private readonly EstadoTarefa[] estadosInativos = [ EstadoTarefa.Cancelada, EstadoTarefa.Concluida ];

        public Gerenciador(IServiceProvider serviceProvider, IRepository<Tarefa> tarefaRepository, IConfiguration configs)
        {
            _tarefaRepository = tarefaRepository;
            _serviceProvider = serviceProvider;
            _configs = configs;
        }

        public async Task Cancelar(int idTarefa)
        {
            IProcessadorTarefas? processador = _serviceProvider.GetService(typeof(IProcessadorTarefas)) as IProcessadorTarefas;

            if (processador != null)
                await processador.CancelarTarefa(idTarefa);
            else
                throw new Exception("Não foi possível acessar o processador de tarefas.");
        }

        public async Task<Tarefa> Consultar(int idTarefa)
            => await Task.FromResult(_tarefaRepository.GetById(idTarefa)!);
        
        public async Task<Tarefa> Criar()
        {
            var ultimaTarefa =  _tarefaRepository.GetAll().MaxBy(tarefa => tarefa.Id);

            var novaTarefa = Tarefa.Criar(ultimaTarefa?.Id + 1 ?? 1, _configs);

            _tarefaRepository.Add(novaTarefa);
            return await Task.FromResult(novaTarefa);
        }

        

        public async Task<IEnumerable<Tarefa>> ListarAtivas()
        {
            return await Task.FromResult(
                _tarefaRepository
                    .GetAll()
                    .Where(t => estadosAtivos.Contains(t.Estado) )
                );
        }

        public async Task<IEnumerable<Tarefa>> ListarInativas()
        {
            return await Task.FromResult(
                _tarefaRepository
                    .GetAll()
                    .Where(t => estadosInativos.Contains(t.Estado))
                );
        }
    }
}
