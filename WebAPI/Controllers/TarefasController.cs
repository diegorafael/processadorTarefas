using Microsoft.AspNetCore.Mvc;
using ProcessadorTarefas.Entidades;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TarefasController : ControllerBase
    {
        private readonly ILogger<TarefasController> _logger;

        public TarefasController(ILogger<TarefasController> logger)
        {
            _logger = logger;
        }

        //[HttpGet("active", Name = "Tarefas Ativas")]
        //public IActionResult<IEnumerable<TarefaDto>> Get()
        //{
            
        //}
    }
}
