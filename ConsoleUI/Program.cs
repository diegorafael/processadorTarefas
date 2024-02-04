using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;
using ProcessadorTarefas.Servicos;
using Repositorio;
using System.Text;

namespace ConsoleUI
{
    internal class Program
    {
        private enum Visualizacao
        {
            NaoDefinida= 0,
            TarefasAtivas = 1,
            TarefasInativas = 2
        }


        static async Task Main(string[] args)
        {
            var visaoAtual = Visualizacao.NaoDefinida;

            var serviceProvider = ConfigureServiceProvider();

            var processador = serviceProvider.GetService<IProcessadorTarefas>();
            var gerenciador = serviceProvider.GetService<IGerenciadorTarefas>();


            await processador.Iniciar();

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var option = Console.ReadKey(intercept: true).KeyChar;
                    Console.WriteLine();
                    switch (option)
                    {
                        case '1':
                            visaoAtual = Visualizacao.TarefasAtivas;
                            break;
                        case '2':
                            var tarefasInativas = await gerenciador.ListarInativas();
                            ImprimirTarefas(tarefasInativas);
                            visaoAtual = Visualizacao.TarefasInativas;
                            break;
                        case '3':
                            await gerenciador.Criar();
                            Console.WriteLine("Nova tarefa criada");
                            await Task.Delay(1000);
                            break;
                        case '4':
                            Console.WriteLine("Digite o número da tarefa:");
                            if (int.TryParse(Console.ReadLine(), out int idTarefa))
                            {
                                var tarefa = gerenciador.Consultar(idTarefa);
                                if (tarefa != null)
                                    await processador.CancelarTarefa(idTarefa);
                            }

                            break;
                        case '5':
                            Console.WriteLine("Encerrando processamento...");
                            await processador.Encerrar();
                            Console.WriteLine("Processamento encerrado.");
                            await Task.Delay(1000);
                            break;
                        case '6':
                            Console.WriteLine("Reiniciando processamento...");
                            await processador.Iniciar();
                            Console.WriteLine("Processamento reiniciado.");
                            await Task.Delay(1000);
                            break;
                        default:
                            Console.WriteLine("Opção inválida!!!");
                            visaoAtual = 0;
                            await Task.Delay(1000);
                            break;
                    }
                }

                Console.Clear();
                ImprimirMenu();

                switch (visaoAtual)
                {
                    case Visualizacao.TarefasAtivas:
                        var tarefasAtivas = await gerenciador.ListarAtivas();
                        ImprimirTarefas(tarefasAtivas);
                        break;
                    case Visualizacao.TarefasInativas:
                        var tarefasInativas = await gerenciador.ListarInativas();
                        ImprimirTarefas(tarefasInativas);
                        break;
                    default:
                        break;
                }

                Console.WriteLine();
                Console.WriteLine();

                ImprimirProgressoTarefas(gerenciador);

                await Task.Delay(100);
            }
        }

        private static IServiceProvider ConfigureServiceProvider()
        {
            string connectionString = "Data Source=database.db";

            IConfiguration configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .Build();

            IServiceCollection services = new ServiceCollection();
            services.AddScoped(_ => configuration);
            //services.AddScoped<IRepository<Tarefa>, MemoryRepository>();
            services.AddScoped<IRepository<Tarefa>>(_ => new SqliteRepository<Tarefa>(connectionString) );
            services.AddSingleton<IProcessadorTarefas, Processador>();
            services.AddScoped<IGerenciadorTarefas, Gerenciador>(serviceProvider =>
            {
                var repository = serviceProvider.GetService<IRepository<Tarefa>>();
                return new Gerenciador(serviceProvider, repository, configuration);
            });

            return services.BuildServiceProvider(); ;
        }

        static void ImprimirMenu()
        {
            Console.WriteLine("ESCOLHA UMA OPÇÃO: ");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("    1. Listar tarefas ativas;");
            Console.WriteLine("    2. Listar tarefas inativas;");
            Console.WriteLine("    3. Criar nova tarefa;");
            Console.WriteLine("    4. Cancelar tarefa;");
            Console.WriteLine("    5. Parar de processar;");
            Console.WriteLine("    6. Reiniciar procesamento;");
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine();
        }
        static void ImprimirTarefas(IEnumerable<Tarefa> tarefas)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                string.Join('|',
                    "DESCRICÃO".PadRight(12, ' '),
                    "ESTADO".PadRight(15, ' '),
                    "INÍCIO".PadRight(30, ' '),
                    "TÉRMINO".PadRight(30, ' '),
                    "SUBTAREFAS".PadRight(10, ' '),
                    "TEMPO TOTAL".PadRight(10, ' ')
                    )
            );
            foreach (var tarefa in tarefas)
                if (tarefa != null)
                {
                    sb.AppendLine($"{string.Join(
                            $"{Extensions.CodigoResetCor}|{Extensions.CodigoCor[tarefa.Estado]}",
                            $"{Extensions.CodigoCor[tarefa.Estado]} Tarefa {tarefa.Id}".PadRight(17, ' '),
                            $"{tarefa.Estado}".PadRight(15, ' '),
                            $"{tarefa.IniciadaEm}".PadRight(30, ' '),
                            $"{tarefa.EncerradaEm}".PadRight(30, ' '),
                            $"{tarefa.SubtarefasExecutadas.Count() + tarefa.SubtarefasPendentes.Count()}".PadRight(10, ' '),
                            $"{tarefa.SubtarefasExecutadas.Union(tarefa.SubtarefasPendentes).Sum(x => x.Duracao.TotalSeconds)}".PadRight(10, ' ')
                        )}{Extensions.CodigoResetCor}");
                }

            Console.WriteLine(sb.ToString());
        }

        static void ImprimirProgressoTarefas(IGerenciadorTarefas gerenciadorTarefas)
        {
            const int BAR_SIZE = 50;
            var sb = new StringBuilder();
            var tarefas = gerenciadorTarefas.ListarAtivas().GetAwaiter().GetResult();

            foreach (var tarefa in tarefas.Where(t => t.Estado == EstadoTarefa.EmExecucao))
                if (tarefa != null)
                {
                    decimal competed = tarefa.SubtarefasExecutadas.Count();
                    var total = tarefa.SubtarefasPendentes.Count() + tarefa.SubtarefasExecutadas.Count();
                    var completion = competed / total;

                    sb.AppendLine(
                        string.Concat($"TAREFA {tarefa.Id}".PadRight(20, ' '),
                        $": [{"█".Repeat(Convert.ToInt32(completion * BAR_SIZE)).PadRight(BAR_SIZE, '_')}] {Convert.ToInt32(completion * 100)}%"
                        ));
                }

            Console.WriteLine(sb.ToString());
        }

        //static void Main(string[] args)
        //{
        //    string connectionString = "Data Source=database.db";
        //    InicializarDatabase(connectionString);
        //    var livroRepository = new LivroSqliteRepository(connectionString);

        //    var novoLivro = new Livro
        //    {
        //        Id = 5,
        //        Publicacao = new DateOnly(2005, 05, 23),
        //        Autores = "Eu, eu mesmo e Irene",
        //        Resumo = "Fala sobre algum assunto aleatório que não é importante para o nosso exemplo",
        //        Titulo = "Éramos dois"
        //    };

        //    livroRepository.Add(novoLivro);

        //}

        //private static void InicializarDatabase(string connectionString)
        //{
        //    using var connection = new SqliteConnection(connectionString);
        //    connection.Open();

        //    using var command = new SqliteCommand(@$"
        //        CREATE TABLE IF NOT EXISTS Livro(
        //            {nameof(Livro.Id)} INTEGER PRIMARY KEY,
        //            {nameof(Livro.Titulo)} TEXT NOT NULL,
        //            {nameof(Livro.Autores)} TEXT NOT NULL,
        //            {nameof(Livro.Resumo)} TEXT NULL,
        //            {nameof(Livro.Publicacao)} DATE NULL
        //        );
        //        ", connection);

        //    command.ExecuteNonQuery();
        //}

    }
}
