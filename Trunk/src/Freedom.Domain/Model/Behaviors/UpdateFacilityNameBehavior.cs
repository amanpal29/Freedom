using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Hedgehog.CommandModel;
using Hedgehog.Services.Command;

namespace Hedgehog.Model.Behaviors
{
    public class UpdateFacilityNameBehavior : IDbContextCommittingBehavior
    {
        public async Task OnCommittingAsync(DbContext context, CommandBase command, CommandExecutionContext commandContext, CancellationToken cancellationToken)
        {
            HedgehogLocalContext db = context as HedgehogLocalContext;

            if (db == null) return;

            List<Site> changedSites = db.ChangeTracker.Entries<Site>()
                .Where(FacilityNameNeedsUpdating)
                .Select(e => e.Entity)
                .ToList();

            List<Facility> changedFacilities = db.ChangeTracker.Entries<Facility>()
                .Where(FacilityNameNeedsUpdating)
                .Select(e => e.Entity)
                .ToList();

            if (changedSites.Count == 0 && changedFacilities.Count == 0)
                return;

            await EnsureFacilitiesOfModifiedSitesAreLoadedAsync(db, cancellationToken);

            await EnsureSitesOfModifiedFacilitiesAreLoadedAsync(db, cancellationToken);

            await db.FacilityType.LoadAsync(cancellationToken);

            HashSet<Facility> facilitiesToUpdate = new HashSet<Facility>(changedFacilities);

            foreach (Site site in changedSites)
                foreach (Facility facility in site.Facilities)
                    facilitiesToUpdate.Add(facility);

            foreach (Facility facility in facilitiesToUpdate)
            {
                Site site = await db.Site.FindAsync(cancellationToken, facility.SiteId);

                FacilityType facilityType = await db.FacilityType.FindAsync(cancellationToken, facility.FacilityTypeId);

                facility.FacilityName = Facility.GetFacilityName(
                    site?.Name, facility.UnitName, facilityType?.Description);
            }
        }

        private static async Task EnsureSitesOfModifiedFacilitiesAreLoadedAsync(HedgehogLocalContext db, CancellationToken cancellationToken)
        {
            List<Facility> changedFacilities = db.ChangeTracker.Entries<Facility>()
                .Where(FacilityNameNeedsUpdating)
                .Select(e => e.Entity)
                .ToList();

            List<Guid> neededSiteIds = changedFacilities
                .Select(f => f.SiteId)
                .Distinct()
                .Where(id => IsSiteLoaded(db, id))
                .ToList();

            if (neededSiteIds.Count > 0)
            {
                await db.Site.Where(s => neededSiteIds.Contains(s.Id)).LoadAsync(cancellationToken);
            }
        }

        private static async Task EnsureFacilitiesOfModifiedSitesAreLoadedAsync(HedgehogLocalContext db, CancellationToken cancellationToken)
        {
            List<Guid> modifiedSiteIds = db.ChangeTracker.Entries<Site>()
                .Where(e => e.State == EntityState.Modified && PropertyChanged(e, s => s.Name))
                .Select(e => e.Entity.Id)
                .ToList();

            if (modifiedSiteIds.Count > 0)
            {
                await db.Facility.Where(f => modifiedSiteIds.Contains(f.SiteId)).LoadAsync(cancellationToken);
            }
        }

        private static bool IsSiteLoaded(HedgehogLocalContext db, Guid siteId)
        {
            return db.Site.Local.Any(s => s.Id == siteId);
        }

        private static bool FacilityNameNeedsUpdating(DbEntityEntry<Site> arg)
        {
            return arg.State == EntityState.Added ||
                   arg.State == EntityState.Modified && PropertyChanged(arg, s => s.Name);
        }

        private static bool FacilityNameNeedsUpdating(DbEntityEntry<Facility> arg)
        {
            return arg.State == EntityState.Added ||
                   arg.State == EntityState.Modified &&
                   (PropertyChanged(arg, f => f.UnitName) || PropertyChanged(arg, f => f.FacilityTypeId));
        }

        private static bool PropertyChanged<TEntity, TProperty>(DbEntityEntry<TEntity> arg, Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : class
        {
            MemberExpression memberExpression = (MemberExpression) propertyExpression.Body;

            string memberName = memberExpression.Member.Name;

            return !Equals(arg.OriginalValues[memberName], arg.CurrentValues[memberName]);
        }
    }
}
