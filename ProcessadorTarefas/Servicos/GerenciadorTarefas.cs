using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ProcessadorTarefas.Servicos
{
    public class GerenciadorTarefas : IGerenciadorTarefas
    {
        private readonly IRepository<Tarefa> _repositorio;
        private readonly IProcessadorTarefas<Tarefa, Subtarefa> _processadorTarefas;

        //Construtor recebendo as DIs
        public GerenciadorTarefas(IRepository<Tarefa> repositorio, IProcessadorTarefas<Tarefa, Subtarefa> processadorTarefas)
        {
            _repositorio = repositorio;
            _processadorTarefas = processadorTarefas;
        }


        public async Task<Tarefa> Criar()
        {
            var novaTarefa = Tarefa.GerarTarefa();
            _repositorio.Add(novaTarefa);

            return await Task.FromResult(novaTarefa);
        }

        public async Task<Tarefa?> Consultar(int idTarefa) //Pra que eu uso isso???
        {
            var tarefaConsultada = _repositorio.GetById(idTarefa);
            if (tarefaConsultada == null)
                throw new Exception("Não existe tarefa com este Id");

            return await Task.FromResult(tarefaConsultada);
        }

        public async Task Cancelar(int idTarefa)
        {
            var tarefaCancelada = _repositorio.GetById(idTarefa);

            if (tarefaCancelada == null)
            {
                throw new Exception("Tarefa não encontrada.");
            }

            if (tarefaCancelada.Estado == EstadoTarefa.EmExecucao)
            {
                // Chama o método de cancelamento no processador de tarefas
                await _processadorTarefas.CancelarTarefa(idTarefa);  //Como eu faço isso???
            }
            else
            {
                tarefaCancelada.Estado = EstadoTarefa.Cancelada;
                _repositorio.Update(tarefaCancelada);
            }
        }

        public async Task<IEnumerable<Tarefa>> ListarAtivas()
        {
            //Tarefas Ativas recebe todas as tarefas listadas como Criada, Agendada ou Concluída
            var tarefasAtivas = _repositorio.GetAll().Where(tarefa =>
            (tarefa.Estado == EstadoTarefa.Criada || tarefa.Estado == EstadoTarefa.Agendada ||
            tarefa.Estado == EstadoTarefa.EmExecucao || tarefa.Estado == EstadoTarefa.EmPausa));

            return await Task.FromResult(tarefasAtivas);
        }

        public async Task<IEnumerable<Tarefa>> ListarInativas()
        {
            //Tarefas Ativas recebe todas as tarefas listadas como Criada, Agendada ou Concluída
            var tarefasInativas = _repositorio.GetAll().Where(tarefa =>
            (tarefa.Estado == EstadoTarefa.Cancelada || tarefa.Estado == EstadoTarefa.Concluida));

            return await Task.FromResult(tarefasInativas);
        }

    }
}