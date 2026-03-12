



using Domain.Entities;
using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Api.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class EmployeeMutation : GenericMutation<Employee, EmployeeInput>
    {
        protected override IQueryable<Employee> IncludeNavigation(IQueryable<Employee> query)
        {
            return query.Include(e => e.EmployeeProjects);
        }
        protected override Employee MapInputToEntity(EmployeeInput input, Employee? existingEntity = null)
        {

            var entity = existingEntity ?? new Employee();

            entity.Name = input.Name;
            entity.Email = input.Email;
            entity.Salary = input.Salary;
            entity.DepartmentId = input.DepartmentId;

            if (input.ProjectIds != null)
            {
                var existingProjectIds = entity.EmployeeProjects?
                    .Select(ep => ep.ProjectId)
                    .ToList();

                // add new
                var toAdd = input.ProjectIds.Except(existingProjectIds);
                foreach (var projectId in toAdd)
                {
                    entity.EmployeeProjects.Add(new EmployeeProject
                    {
                        EmployeeId = entity.Id,
                        ProjectId = projectId
                    });
                }

                // remove old
                var toRemove = entity.EmployeeProjects
                    .Where(ep => !input.ProjectIds.Contains(ep.ProjectId))
                    .ToList();

                foreach (var rel in toRemove)
                {
                    entity.EmployeeProjects.Remove(rel);
                }
            }

            return entity;
        }

        protected override void SetEntityId(Employee entity, int id)
        {
            entity.Id = id;
        }

        protected override object GetEntityId(Employee entity)
        {
            return entity.Id;
        }


        public async Task<Employee> CreateEmployee(
                EmployeeInput input,
                [Service] AppDbContext db,
                [Service] IValidator<EmployeeInput> validator,
                CancellationToken cancellationToken)
        {
            return await Create(input, db, validator, cancellationToken);
        }

        public async Task<Employee> UpdateEmployee(
            int id,
            EmployeeInput input,
            [Service] AppDbContext db,
            [Service] IValidator<EmployeeInput> validator,
            CancellationToken cancellationToken)
        {
            return await Update(id, input, db, validator, cancellationToken);
        }

        public async Task<bool> DeleteEmployee(
            int id,
            [Service] AppDbContext db,
            CancellationToken cancellationToken)
        {
            return await Delete(id, db, cancellationToken);
        }
    }
}