using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;
using System.Threading.Tasks;

namespace ProcessadorTarefas.Servicos
{
    public class ProcessadorTarefasClasse : IProcessadorTarefas<Tarefa,Subtarefa>
    {

        private IRepository<Tarefa> _repositorio;

        public ProcessadorTarefasClasse(IRepository<Tarefa> repositorio)
        {
            _repositorio = repositorio;
        }

        public Task CancelarTarefa(int idTarefa)
        {
            throw new NotImplementedException();
        }
        public Task Encerrar()
        {
            throw new NotImplementedException();
        }

        public async Task ProcessarTarefasAsync(int tarefasEmParalelo) //recebe do arquivo config
        {
            //Preciso adicionar a lógica para agendar tarefas
            //preciso adicionar a lógica para colocar as emPausa antes do resto
            
            
            
            
            Queue<Tarefa> filaTarefa = new Queue<Tarefa>(_repositorio.GetAll().Where(tarefa => tarefa.Estado == EstadoTarefa.Agendada));

            var tasksEmExecucao = new List<Task>();

            //Quando inicializo o programa, entra aqui
            while (tasksEmExecucao.Count < tarefasEmParalelo)
            {
                Tarefa tarefa = filaTarefa.Dequeue();
                tasksEmExecucao.Add(IniciarTarefaAsync(tarefa));  // 5 Tasks <= Tarefas
            }

            //Quando o programa já está executando e uma tarefa termina, entra aqui
            while (tasksEmExecucao.Count > 0)
            {
                var tarefaConcluida = await Task.WhenAny(tasksEmExecucao);
                tasksEmExecucao.Remove(tarefaConcluida);
                if (filaTarefa.Count > 0)
                {
                    Tarefa tarefa = filaTarefa.Dequeue();
                    tasksEmExecucao.Add(IniciarTarefaAsync(tarefa));
                }
                await Task.WhenAll(tasksEmExecucao); //Se não for aqui, colocar logo abaixo da } a seguir
            }

        }

        public async Task IniciarTarefaAsync(Tarefa tarefa)
        {
            tarefa.Estado = EstadoTarefa.EmExecucao;
            var subtarefasPendentes = tarefa.SubtarefasPendentes;

            foreach (var subtarefa in subtarefasPendentes)
            {
                await IniciarSubtarefasAsync(subtarefa);
                tarefa.SubtarefasExecutadas.Add(subtarefa);
                tarefa.SubtarefasPendentes.Remove(subtarefa);
            }

            tarefa.Estado = EstadoTarefa.Concluida;
            tarefa.EncerradaEm = DateTime.Now;
        }

        public async Task IniciarSubtarefasAsync(Subtarefa subtarefa)
        {
            await Task.Delay(subtarefa.Duracao);
        }
    }
}