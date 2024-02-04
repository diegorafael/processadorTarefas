using ProcessadorTarefas.Entidades;
using System.Text;

namespace ConsoleUI
{
    public static class Extensions
    {
        public const string CodigoResetCor = "\u001b[0m";
        public static Dictionary<EstadoTarefa, string> CodigoCor = new Dictionary<EstadoTarefa, string>{
            { EstadoTarefa.Criada, "\u001b[36m" },
            { EstadoTarefa.Cancelada, "\u001b[31m" },
            { EstadoTarefa.Concluida, "\u001b[32m" },
            { EstadoTarefa.Agendada, "\u001b[93m" },
            { EstadoTarefa.EmExecucao, "\u001b[33m" },
            { EstadoTarefa.EmPausa, "\u001b[97m" },
        };

        public static string Repeat(this string self, int times)
        {
            if (times <= 0)
                return string.Empty;

            var sb = new StringBuilder();
            for (int i = 0; i < times; i++)
                sb.Append(self);

            return sb.ToString();
        }
    }
}
