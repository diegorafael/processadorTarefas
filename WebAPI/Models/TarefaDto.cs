using ProcessadorTarefas.Entidades;

namespace WebAPI
{
    public class TarefaDto
    {
        public int Id { get; set; }
        public EstadoTarefa Estado { get; set; }
        public DateTime? IniciadaEm { get; set; }
        public DateTime? EncerradaEm { get; set; }
        public decimal PercentualConcluido { get; set; }
    }
}
