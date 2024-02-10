using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcessadorTarefas.Entidades;
using ProcessadorTarefas.Repositorios;
using ProcessadorTarefas.Servicos;

namespace ConsoleUI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            //Receiving Services for DI
            var serviceProvider = ServicesConfiguration();


            //Instanciando os serviços pela interface
            var processadorTarefas = serviceProvider.GetService<IProcessadorTarefas<Tarefa, Subtarefa>>();
            var gerenciadorTarefas = serviceProvider.GetService<IGerenciadorTarefas>();
            var configuracao = serviceProvider.GetService<IConfiguration>();

            // Aqui você acessa o valor da configuração "quantidadeTarefasEmParalelo"
            var quantidadeTaskParalelo = int.Parse(configuracao["quantidadeTarefasEmParalelo"]);

            if (quantidadeTaskParalelo > 0)
                await processadorTarefas.ProcessarTarefasAsync(quantidadeTaskParalelo);
        }

        public static IServiceProvider ServicesConfiguration()
        {
            string basePath = Path.GetFullPath("appsettings.json").Replace("\\bin\\Debug\\net8.0", "");

            //2-Build a configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(basePath, optional: false, reloadOnChange: true)
                .Build();

            //1&3-Create a Service Collection for DI
            //& Add the configuration to the service Collection (Dependendy Injection Container)
            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddScoped<IRepository<Tarefa>, MemoryRepository>()
                //.AddScoped<IRepository<Tarefa>>(_ => new SqliteRepository<Tarefa>(connectionString)) //pra usar no futuro
                .AddSingleton<IProcessadorTarefas<Tarefa, Subtarefa>, ProcessadorTarefasClasse>()
                .AddScoped<IGerenciadorTarefas>(provider =>
                {
                    var repository = provider.GetService<IRepository<Tarefa>>();
                    var serviceProvider = provider.GetService<IProcessadorTarefas<Tarefa, Subtarefa>>();
                    return new GerenciadorTarefas(repository, serviceProvider);
                })
                .BuildServiceProvider();

            ////Do prof
            //IServiceCollection service = new ServiceCollection();
            //service.AddScoped(_ => configuration);
            //service.AddScoped<IRepository<Tarefa>, MemoryRepository>();
            ////service.AddSingleton<IProcessadorTarefas, ProcessadorTarefasClasse>(), 
            //service.AddScoped<IGerenciadorTarefas, GerenciadorTarefas>(serviceProvider =>
            //{
            //    var repository = serviceProvider.GetService<IRepository<Tarefa>>();
            //    return new GerenciadorTarefas(serviceProvider, repository, configuration);
            //});

            return services;
        }







        //var memoryRepository = new MemoryRepository();

        //    var tarefas = memoryRepository.GetAll();
        //    Console.WriteLine("Só inicializada");
        //    foreach (var tarefa in tarefas)
        //    {
        //        Console.WriteLine($"Sou a tarefa {tarefa.Id} e tenho {tarefa.SubtarefasPendentes.Count} subtarefas.");
        //    }
        //    Console.WriteLine("____________________________________________________");




        //    memoryRepository.Add();
        //    memoryRepository.Add();
        //    tarefas = memoryRepository.GetAll();
        //    Console.WriteLine("Com duas adições");
        //    foreach (var tarefa in tarefas)
        //    {
        //        Console.WriteLine($"Sou a tarefa {tarefa.Id} e tenho {tarefa.SubtarefasPendentes.Count} subtarefas.");
        //    }
    }
}