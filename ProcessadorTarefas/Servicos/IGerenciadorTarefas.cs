using ProcessadorTarefas.Entidades;

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
}
