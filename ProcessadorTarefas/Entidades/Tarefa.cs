using static ProcessadorTarefas.Entidades.Tarefa;

namespace ProcessadorTarefas.Entidades
{
    public interface ITarefa
    {
        int Id { get; }
        EstadoTarefa Estado { get; }
        DateTime IniciadaEm { get; }
        DateTime EncerradaEm { get; }
        ICollection<Subtarefa> SubtarefasPendentes { get; }
        ICollection<Subtarefa> SubtarefasExecutadas { get; }
    }

    public class Tarefa : ITarefa
    {
        public int Id { get; set; }
        public EstadoTarefa Estado { get; set; }
        public DateTime IniciadaEm { get; set; }
        public DateTime EncerradaEm { get; set; }
        public ICollection<Subtarefa>? SubtarefasPendentes { get; set; }
        public ICollection<Subtarefa>? SubtarefasExecutadas { get; set; }

        private static int nextId = 0;

        public Tarefa()   //Construtor em memória
        {
            //Colocar o gerador de Id
            Id = ++nextId;
            Estado = EstadoTarefa.Criada;
            SubtarefasPendentes = new List<Subtarefa>();
            SubtarefasExecutadas = new List<Subtarefa>();
        }


        //public Tarefa(int id)   //Construtor DB (?)
        //{
        //    Id = id;
        //    Estado = EstadoTarefa.Criada;
        //    SubtarefasPendentes = new List<Subtarefa>();
        //    SubtarefasExecutadas = new List<Subtarefa>();
        //}

        public static Tarefa GerarTarefa()
        {
            var tarefa = new Tarefa();
            Random random = new Random();

            //CRIA UM NUMERO RANDON DE SUBTAREFAS
            for (int contadorSubtarefas = 0; contadorSubtarefas < random.Next(2, 5); contadorSubtarefas++) //trocar o random para 10,100
            {
                var subtarefa = new Subtarefa
                {
                    Duracao = TimeSpan.FromSeconds(random.Next(3, 10)) //trocar pra 3, 60
                };

                tarefa.SubtarefasPendentes.Add(subtarefa);
            }

            return tarefa;
        }

    }
}