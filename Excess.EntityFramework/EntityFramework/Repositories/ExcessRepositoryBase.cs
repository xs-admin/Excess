using Abp.Domain.Entities;
using Abp.EntityFramework.Repositories;

namespace Excess.EntityFramework.Repositories
{
    public abstract class ExcessRepositoryBase<TEntity, TPrimaryKey> : EfRepositoryBase<ExcessDbContext, TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
    }

    public abstract class ExcessRepositoryBase<TEntity> : ExcessRepositoryBase<TEntity, int>
        where TEntity : class, IEntity<int>
    {

    }
}
