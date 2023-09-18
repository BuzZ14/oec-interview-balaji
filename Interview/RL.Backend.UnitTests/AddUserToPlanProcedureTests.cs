using Moq;
using RL.Backend.Commands.Handlers.Plans;
using RL.Backend.Commands;
using RL.Backend.Exceptions;
using RL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace RL.Backend.UnitTests
{
    [TestClass]
    public class AddUserToPlanProcedureTests
    {
        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(int.MinValue)]
        public async Task AddUserToPlanProcedureTests_InvalidPlanId_ReturnsBadRequest(int planId)
        {
            //Given
            var context = new Mock<RLContext>();
            var sut = new AddUserToPlanProcedureCommandHandler(context.Object);
            var request = new AddUserToPlanProcedureCommand()
            {
                PlanId = planId,
                ProcedureId = 1,
                UserIds = new List<int> { 1 },
            };
            //When
            var result = await sut.Handle(request, new CancellationToken());

            //Then
            result.Exception.Should().BeOfType(typeof(BadRequestException));
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(int.MinValue)]
        public async Task AddUserToPlanProcedureTests_InvalidProcedureId_ReturnsBadRequest(int procedureId)
        {
            //Given
            var context = new Mock<RLContext>();
            var sut = new AddUserToPlanProcedureCommandHandler(context.Object);
            var request = new AddUserToPlanProcedureCommand()
            {
                PlanId = 1,
                ProcedureId = procedureId,
                UserIds = new List<int> { 1 },
            };
            //When
            var result = await sut.Handle(request, new CancellationToken());

            //Then
            result.Exception.Should().BeOfType(typeof(BadRequestException));
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(int.MinValue)]
        public async Task AddUserToPlanProcedureTests_InvalidUserId_ReturnsBadRequest(int userId)
        {
            //Given
            var context = new Mock<RLContext>();
            var sut = new AddUserToPlanProcedureCommandHandler(context.Object);
            var request = new AddUserToPlanProcedureCommand()
            {
                PlanId = 1,
                ProcedureId = 1,
                UserIds = new List<int> { userId },
            };
            //When
            var result = await sut.Handle(request, new CancellationToken());

            //Then
            result.Exception.Should().BeOfType(typeof(BadRequestException));
            result.Succeeded.Should().BeFalse();
        }

        public async Task AddUserToPlanProcedureTests_PlanIdNotFound_ReturnsNotFound(int planId)
        {
            //Given
            var context = DbContextHelper.CreateContext();
            var sut = new AddProcedureToPlanCommandHandler(context);
            var request = new AddProcedureToPlanCommand()
            {
                PlanId = planId,
                ProcedureId = 1
            };

            context.Plans.Add(new Data.DataModels.Plan
            {
                PlanId = planId + 1
            });
            await context.SaveChangesAsync();

            //When
            var result = await sut.Handle(request, new CancellationToken());

            //Then
            result.Exception.Should().BeOfType(typeof(NotFoundException));
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(19)]
        [DataRow(35)]
        public async Task AddUserToPlanProcedureTests_ProcedureIdNotFound_ReturnsNotFound(int procedureId)
        {
            //Given
            var context = DbContextHelper.CreateContext();
            var sut = new AddProcedureToPlanCommandHandler(context);
            var request = new AddProcedureToPlanCommand()
            {
                PlanId = 1,
                ProcedureId = procedureId
            };

            context.Plans.Add(new Data.DataModels.Plan
            {
                PlanId = procedureId + 1
            });
            context.Procedures.Add(new Data.DataModels.Procedure
            {
                ProcedureId = procedureId + 1,
                ProcedureTitle = "Test Procedure"
            });
            await context.SaveChangesAsync();

            //When
            var result = await sut.Handle(request, new CancellationToken());

            //Then
            result.Exception.Should().BeOfType(typeof(NotFoundException));
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(19)]
        [DataRow(35)]
        public async Task AddUserToPlanProcedureTests_UserIdNotFound_ReturnsNotFound(int userId)
        {
            //Given
            var context = DbContextHelper.CreateContext();
            var sut = new AddProcedureToPlanCommandHandler(context);
            var request = new AddProcedureToPlanCommand()
            {
                PlanId = 1,
                ProcedureId = 1
            };

            context.Plans.Add(new Data.DataModels.Plan
            {
                PlanId = userId + 1
            });
            context.Procedures.Add(new Data.DataModels.Procedure
            {
                ProcedureId = userId + 1,
                ProcedureTitle = "Test Procedure"
            });
            context.Users.Add(new Data.DataModels.User
            {
                UserId = userId + 1,
                Name = "Test User"
            });
            await context.SaveChangesAsync();

            //When
            var result = await sut.Handle(request, new CancellationToken());

            //Then
            result.Exception.Should().BeOfType(typeof(NotFoundException));
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(1, 1, 1)]
        public async Task AddUserToPlanProcedureTests_ReturnsSuccess(int planId, int procedureId, int userId)
        {
            //Given
            var context = DbContextHelper.CreateContext();
            var sut = new AddProcedureToPlanCommandHandler(context);
            var request = new AddProcedureToPlanCommand()
            {
                PlanId = planId,
                ProcedureId = procedureId
            };

            context.Plans.Add(new Data.DataModels.Plan
            {
                PlanId = planId
            });
            context.Procedures.Add(new Data.DataModels.Procedure
            {
                ProcedureId = procedureId,
                ProcedureTitle = "Test Procedure"
            });
            context.Users.Add(new Data.DataModels.User
            {
                UserId = userId,
                Name = "Test User"
            });
            await context.SaveChangesAsync();

            context.PlanProcedureUsers.Add(new Data.DataModels.PlanProcedureUser 
            {
                PlanId = planId,
                ProcedureId = procedureId,
                UserId = userId,
            });
            await context.SaveChangesAsync();

            //When
            var result = await sut.Handle(request, new CancellationToken());

            //Then
            var dbPlanProcedureUser = await context.PlanProcedureUsers.FirstOrDefaultAsync(ppu => ppu.PlanId == planId && ppu.ProcedureId == procedureId && ppu.UserId == userId);

            dbPlanProcedureUser.Should().NotBeNull();

            result.Value.Should().BeOfType(typeof(Unit));
            result.Succeeded.Should().BeTrue();
        }
    }
}
