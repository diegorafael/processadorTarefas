using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;
using ProcessadorTarefas.Servicos;

namespace ProcessadorTarefas.Repositorios
{
    public class MemoryRepository : IRepository<Tarefa>
    {
        private static List<Tarefa>? _tarefas; //aqui é variável

        private static List<Tarefa> Tarefas //aqui é prop
        {
            get
            {
                if (_tarefas == null)
                {
                    _tarefas = GerarListaTarefas(); //Esse número é definido aqui mesmo.
                }
                return _tarefas;
            }
            set
            {
                _tarefas = value;
            }
        }

        private readonly IConfiguration _configuration;

        public MemoryRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void Add(Tarefa tarefa)
        {
            Tarefas.Add(tarefa);

        }

        public IEnumerable<Tarefa> GetAll()
        {
            return Tarefas;
        }

        public Tarefa? GetById(int id)
        {
            return Tarefas.FirstOrDefault(tarefa => tarefa.Id == id);
        }

        public void Update(Tarefa tarefa)
        {
            var tarefaExistente = GetById(tarefa.Id);

            if (tarefaExistente != null)
            {
                tarefaExistente.Id = tarefa.Id;
                tarefaExistente.Estado = tarefa.Estado;
                tarefaExistente.IniciadaEm = tarefa.IniciadaEm;
                tarefaExistente.EncerradaEm = tarefa.EncerradaEm;
                tarefaExistente.SubtarefasPendentes = tarefa.SubtarefasPendentes;
                tarefaExistente.SubtarefasExecutadas = tarefa.SubtarefasExecutadas;
            }
            else
            {
                throw new InvalidOperationException("Tarefa não encontrada.");
            }
        }


        private static List<Tarefa> GerarListaTarefas()
        {
            const int quantidadeTarefas = 20;  //pelo problema deve iniciar com 100
            List<Tarefa> tarefas = new List<Tarefa>();

            for (int contadorTarefas = 0; contadorTarefas < quantidadeTarefas; contadorTarefas++)
            {
                var tarefa = Tarefa.GerarTarefa();
                tarefas.Add(tarefa);
            }

            return tarefas;
        }

        //SE NÂO DER CERTO VOLTA PRA CA
        //var tarefa = new Tarefa();
        //Random randon = new Random();

        ////CRIA AS SUBTAREFAS
        //for (int contadorSubtarefas = 0; contadorSubtarefas < randon.Next(2, 10); contadorSubtarefas++) //trocar pra 10, 100
        //{
        //    var subtarefa = new Subtarefa
        //    {
        //        Duracao = TimeSpan.FromSeconds(randon.Next(3, 10)) //trocar pra 3, 60
        //    };

        //    tarefa.SubtarefasPendentes.Add(subtarefa);
        //}

        //return tarefa;
    }
}