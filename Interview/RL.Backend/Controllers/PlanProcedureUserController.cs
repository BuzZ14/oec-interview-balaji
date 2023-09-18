using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Commands;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PlanProcedureUserController : ControllerBase
    {
        private readonly ILogger<PlanController> _logger;
        private readonly RLContext _context;
        private readonly IMediator _mediator;

        public PlanProcedureUserController(ILogger<PlanController> logger, RLContext context, IMediator mediator)
        {
            _logger = logger;
            _context = context;
            _mediator = mediator;
        }

        [HttpGet("GetAssignedUsersForPlanAndProcedure")]
        public async Task<IActionResult> GetAssignedUsersForPlan([FromQuery] int planId, [FromQuery] int procedureId)
        {
            var result = await _context.PlanProcedureUsers
                .Where(ppu => ppu.PlanId == planId && ppu.ProcedureId == procedureId).ToListAsync();

            return Ok(result);
        }

        [HttpPost("AddUserToPlanProcedure")]
        public async Task<IActionResult> AddProcedureToPlan(AddUserToPlanProcedureCommand command, CancellationToken token)
        {
            var response = await _mediator.Send(command, token);

            return response.ToActionResult();
        }
    }
}
