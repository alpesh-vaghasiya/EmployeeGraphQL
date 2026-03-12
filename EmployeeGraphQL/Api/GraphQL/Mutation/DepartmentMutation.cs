



using Api.GraphQL.Inputs;
using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Api.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class DepartmentMutation : GenericMutation<Department, DepartmentInput>
    {
        protected override IQueryable<Department> IncludeNavigation(IQueryable<Department> query)
        {
            return query.Include(d => d.Employees);
        }
        protected override Department MapInputToEntity(DepartmentInput input, Department? existingEntity = null)
        {
            var entity = existingEntity ?? new Department();
            entity.Name = input.Name.Trim();

            return entity;
        }

        protected override void SetEntityId(Department entity, int id)
        {
            entity.Id = id;
        }

        protected override object GetEntityId(Department entity)
        {
            return entity.Id;
        }

        [AllowAnonymous]
        public async Task<Department> CreateDepartment(
                DepartmentInput input,
                [Service] AppDbContext db,
                [Service] IValidator<DepartmentInput> validator,
                CancellationToken cancellationToken)
        {
            var name = input.Name.Trim();
            if (await db.Departments.AnyAsync(x => EF.Functions.ILike(x.Name, name), cancellationToken))
            {
                throw new GraphQLException(ErrorBuilder.New().SetMessage("Department name already exists.").SetCode("VALIDATION_ERROR").Build());
            }
            return await Create(input, db, validator, cancellationToken);
        }

        public async Task<Department> UpdateDepartment(
            int id,
            DepartmentInput input,
            [Service] AppDbContext db,
            [Service] IValidator<DepartmentInput> validator,
            CancellationToken cancellationToken)
        {
            var name = input.Name.Trim();

            if (await db.Departments.AnyAsync(x => EF.Functions.ILike(x.Name, name) && x.Id != id, cancellationToken))
            {
                throw new GraphQLException(ErrorBuilder.New().SetMessage("Department name already exists.").SetCode("VALIDATION_ERROR").Build());
            }
            return await Update(id, input, db, validator, cancellationToken);
        }

        public async Task<bool> DeleteDepartment(
            int id,
            [Service] AppDbContext db,
            CancellationToken cancellationToken)
        {
            return await Delete(id, db, cancellationToken);
        }
    }
}