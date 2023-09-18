using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers.Plans
{
    public class AddUserToPlanProcedureCommandHandler : IRequestHandler<AddUserToPlanProcedureCommand, ApiResponse<Unit>>
    {
        private readonly RLContext _context;

        public AddUserToPlanProcedureCommandHandler(RLContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Unit>> Handle(AddUserToPlanProcedureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Validate request
                if (request.PlanId < 1)
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanId"));
                if (request.ProcedureId < 1)
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid ProcedureId"));

                foreach (var userId in request.UserIds)
                {
                    if (userId < 1)
                        return ApiResponse<Unit>.Fail(new BadRequestException("Invalid UserId"));

                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserId == userId);

                    if (user is null)
                        return ApiResponse<Unit>.Fail(new NotFoundException($"UserId: {userId} not found"));
                }

                var plan = await _context.Plans
                    .FirstOrDefaultAsync(p => p.PlanId == request.PlanId);

                var procedure = await _context.Procedures
                    .FirstOrDefaultAsync(p => p.ProcedureId == request.ProcedureId);

                if (plan is null)
                    return ApiResponse<Unit>.Fail(new NotFoundException($"PlanId: {request.PlanId} not found"));
                if (procedure is null)
                    return ApiResponse<Unit>.Fail(new NotFoundException($"ProcedureId: {request.ProcedureId} not found"));

                // Delete all existing users for the given plan id & procedure id
                var existingAssignedUsers = await _context.PlanProcedureUsers
                    .Where(ppu => ppu.PlanId == request.PlanId && ppu.ProcedureId == request.ProcedureId)
                    .ToArrayAsync();

                _context.PlanProcedureUsers.RemoveRange(existingAssignedUsers);
                await _context.SaveChangesAsync();

                foreach (var userId in request.UserIds)
                {
                    if (userId < 1)
                        return ApiResponse<Unit>.Fail(new BadRequestException("Invalid UserId"));

                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserId == userId);

                    if (_context.PlanProcedureUsers.Any(ppu => ppu.UserId == user.UserId && ppu.ProcedureId == procedure.ProcedureId))
                        continue;

                    _context.PlanProcedureUsers.Add(new PlanProcedureUser
                    {
                        PlanId = plan.PlanId,
                        ProcedureId = procedure.ProcedureId,
                        UserId = user.UserId,
                    });
                }

                await _context.SaveChangesAsync();

                return ApiResponse<Unit>.Succeed(new Unit());
            }
            catch (Exception e)
            {
                return ApiResponse<Unit>.Fail(e);
            }
        }
    }
}
