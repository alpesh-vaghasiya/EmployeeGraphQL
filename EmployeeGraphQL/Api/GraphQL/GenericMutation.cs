using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Api.GraphQL
{
    public abstract class GenericMutation<TEntity, TInput>
        where TEntity : class, new()
        where TInput : class
    {
        protected abstract TEntity MapInputToEntity(TInput input, TEntity? existingEntity = null);
        protected abstract void SetEntityId(TEntity entity, int id);
        protected abstract int GetEntityId(TEntity entity);
        protected virtual IQueryable<TEntity> IncludeNavigation(IQueryable<TEntity> query)
        {
            return query;
        }

        public async Task<TEntity> Create(
            TInput input,
            [Service] AppDbContext db,
            [Service] IValidator<TInput> validator,
            CancellationToken cancellationToken)
        {
            // Validate input
            var validationResult = await validator.ValidateAsync(input, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Map input to entity
            var entity = MapInputToEntity(input);

            // Add to database
            db.Set<TEntity>().Add(entity);
            await db.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public async Task<TEntity> Update(
            int id,
            TInput input,
            [Service] AppDbContext db,
            [Service] IValidator<TInput> validator,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(input, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            IQueryable<TEntity> query = db.Set<TEntity>();

            // include navigation properties if overridden
            query = IncludeNavigation(query);

            // ⭐ Detect primary key dynamically
            var entityType = db.Model.FindEntityType(typeof(TEntity));
            var primaryKey = entityType.FindPrimaryKey();
            var keyProperty = primaryKey.Properties.First().Name;

            var entity = await query.FirstOrDefaultAsync(
                e => EF.Property<object>(e, keyProperty).Equals(id),
                cancellationToken);

            if (entity == null)
                throw new Exception($"{typeof(TEntity).Name} with ID {id} not found");

            MapInputToEntity(input, entity);

            await db.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public async Task<bool> Delete(
            int id,
            [Service] AppDbContext db,
            CancellationToken cancellationToken)
        {
            var entity = await db.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
            {
                throw new Exception($"Entity with ID {id} not found");
            }

            db.Set<TEntity>().Remove(entity);
            await db.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
