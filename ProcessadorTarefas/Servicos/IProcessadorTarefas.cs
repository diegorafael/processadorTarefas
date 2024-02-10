namespace ProcessadorTarefas.Servicos
{
    public interface IProcessadorTarefas<T,Y>
    {
        Task ProcessarTarefasAsync(int tarefasEmParalelo);
        Task IniciarTarefaAsync(T entity);
        Task IniciarSubtarefasAsync(Y entity);
        Task CancelarTarefa(int idTarefa);
        Task Encerrar();
    }
}
