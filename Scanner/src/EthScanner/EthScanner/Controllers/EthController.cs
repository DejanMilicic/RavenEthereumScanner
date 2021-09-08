using System.Threading.Tasks;
using EthScanner.Features;
using EthScanner.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EthScanner.Controllers
{
    [ApiController]
    public class EthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("test")]
        public async Task<TransactionInfo> Get() => await _mediator.Send(new GetRandomTransaction.Query());
    }
}
