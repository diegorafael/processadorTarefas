using Microsoft.Extensions.Configuration;
using ProcessadorTarefas.Entidades;
using System.Diagnostics;

namespace ProcessadorTarefas.Servicos
{
    public interface IProcessadorTarefas
    {
        Task Iniciar();
        Task CancelarTarefa(int idTarefa);
        Task Encerrar();
    }

    public class Processador : IProcessadorTarefas
    {
        private bool _running;
        private readonly IGerenciadorTarefas _gerenciadorTarefas;
        private readonly IConfiguration _configs;
        private Dictionary<int, Tarefa> TarefasDisponiveis = new Dictionary<int, Tarefa>();
        private Dictionary<int, Tarefa> TarefasEmprocessamento = new Dictionary<int, Tarefa>();
        private Queue<Tarefa> FilaTarefasAgendadas = new Queue<Tarefa>();

        private Dictionary<int, CancellationTokenSource> cancelationTokens = new Dictionary<int, CancellationTokenSource>();
        
        public Processador(IGerenciadorTarefas gerenciadorTarefas, IConfiguration configs)
        {
            _gerenciadorTarefas = gerenciadorTarefas;
            _configs = configs;
        }

        public Task Iniciar()
        {
            var tarefas = _gerenciadorTarefas.ListarAtivas().GetAwaiter().GetResult();

            _running = true;
            foreach (var tarefa in tarefas.Where(t => t.Estado == EstadoTarefa.EmPausa))
            {
                var cancelationSource = new CancellationTokenSource();
                TarefasEmprocessamento.Add(tarefa.Id, tarefa);
                cancelationTokens.Add(tarefa.Id, cancelationSource);
                Task.Run(async () =>
                    {
                        await ProcessarTarefa(tarefa);
                        TarefasEmprocessamento.Remove(tarefa.Id);
                    },
                    cancelationSource.Token);
            }

            foreach (var tarefa in tarefas.Where(t => t.Estado == EstadoTarefa.Agendada))
            {
                FilaTarefasAgendadas.Enqueue(tarefa);
            }

            GerenciarProcessamento();

            return Task.CompletedTask;
        }

        public async Task CancelarTarefa(int idTarefa)
        {
            if (cancelationTokens.ContainsKey(idTarefa))
            {
                cancelationTokens[idTarefa].Cancel();
                cancelationTokens.Remove(idTarefa);
            }

            TarefasEmprocessamento.Remove(idTarefa);
            TarefasDisponiveis.Remove(idTarefa);
            var tarefa = await _gerenciadorTarefas.Consultar(idTarefa);

            if (tarefa != null)
                tarefa.Cancelar();
            else
                Debug.Fail($"Tarefa {idTarefa} não encontrada!");
        }

        public async Task Encerrar()
        {
            _running = false;
            foreach (var pairTarefaEmProcessamento in TarefasEmprocessamento)
            {
                cancelationTokens[pairTarefaEmProcessamento.Key].Cancel();
                cancelationTokens.Remove(pairTarefaEmProcessamento.Key);
                pairTarefaEmProcessamento.Value.Pausar();

                TarefasEmprocessamento.Remove(pairTarefaEmProcessamento.Key);
            }
        }

        private async Task GerenciarProcessamento()
        {
            while (_running)
            {
                try
                {
                    var tarefasPersistidas = await _gerenciadorTarefas.ListarAtivas();
                    foreach (var tarefa in tarefasPersistidas)
                    {
                        if (!TarefasEmprocessamento.ContainsKey(tarefa.Id) && 
                            !FilaTarefasAgendadas.Contains(tarefa) &&
                            !TarefasDisponiveis.ContainsKey(tarefa.Id))
                        {
                            TarefasDisponiveis.Add(tarefa.Id, tarefa);
                        }
                    }

                    AlimentarFilaAgendamentos();
                    ConsumirFilaAgendamento();
                }
                catch (Exception ex)
                {
                    Debug.Fail($"Error: {ex.Message}");
                }

                await Task.Delay(250);
            }
        }

        private void AlimentarFilaAgendamentos()
        {
            var limitTasksScheduled = int.Parse(_configs[Consts.MAX_TASKS_SCHEDULED] ?? "1");
            if ( FilaTarefasAgendadas.Count < limitTasksScheduled)
                for (int i = 0; i < (limitTasksScheduled - FilaTarefasAgendadas.Count); i++)
                    if(TarefasDisponiveis.Count > 0)
                    {
                        var tarefaDisponivelSelecionada = TarefasDisponiveis.OrderBy(x => x.Key).Take(1).Single().Value;
                        tarefaDisponivelSelecionada.Agendar();
                        FilaTarefasAgendadas.Enqueue(tarefaDisponivelSelecionada);
                        TarefasDisponiveis.Remove(tarefaDisponivelSelecionada.Id);
                    }
        }

        private async Task ProcessarTarefa(Tarefa tarefa)
        {
            tarefa.Iniciar();
            var subtarefas = tarefa.SubtarefasPendentes;

            foreach (var subtarefa in subtarefas)
            {
                await Task.Delay(subtarefa.Duracao);
                tarefa.ConcluirSubtarefa(subtarefa);
            }
            tarefa.Concluir();
        }

        private void ConsumirFilaAgendamento()
        {
            if (FilaTarefasAgendadas.Count > 0)
                if (TarefasEmprocessamento.Count < int.Parse(_configs[Consts.MAX_TASKS_IN_PARALLEL] ?? "1"))
                {
                    var tarefa = FilaTarefasAgendadas.Dequeue();
                    if (tarefa.PodeSerExecutada())
                    {
                        var cancelationSource = new CancellationTokenSource();
                        TarefasEmprocessamento.Add(tarefa.Id, tarefa);

                        cancelationTokens.Add(tarefa.Id, cancelationSource);
                        Task.Run(async () => 
                            {
                                await ProcessarTarefa(tarefa);
                                TarefasEmprocessamento.Remove(tarefa.Id);
                            }, 
                            cancelationSource.Token); 
                    }
                }
        }
    }
}
