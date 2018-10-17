using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Freedom.Annotations;
using Freedom.Linq;

namespace Freedom.Extensions
{
    public static class DbContextExtensions
    {
        private static readonly MethodInfo DbContextSetMethod =
            ExpressionHelper.GetMethod<DbContext>(x => x.Set<object>());

        public static IQueryable DbSet(this DbContext context, [NotNull] string entityTypeName)
        {
            if (string.IsNullOrEmpty(entityTypeName))
                throw new ArgumentNullException(nameof(entityTypeName));

            Type type = context.GetEntityTypeByName(entityTypeName);

            if (type == null)
                throw new InvalidOperationException(
                    $"The entity type named '{entityTypeName}' was not found in the CSpace Metadata of the DbContext.");

            return DbSet(context, type);
        }

        private static IQueryable DbSet(DbContext context, [NotNull] Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            MethodInfo genericMethod = DbContextSetMethod.MakeGenericMethod(type);

            return (IQueryable) genericMethod.Invoke(context, null);
        }
    }
}
