using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProductCatalogApi.Models;

namespace ProductCatalogApi.Data
{
    /// <summary>
    /// Stamps CreatedAt/CreatedBy on insert and UpdatedAt/UpdatedBy on update
    /// for every <see cref="IAuditable"/> entity, right before EF saves.
    /// "Who" comes from the JWT of the current request (ClaimTypes.Name = username);
    /// work done outside an authenticated request (register, login, seeding) is recorded as "system".
    /// </summary>
    public sealed class AuditInterceptor : SaveChangesInterceptor
    {
        public const string SystemUser = "system";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimeProvider _timeProvider;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor, TimeProvider timeProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _timeProvider = timeProvider;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyAudit(DbContext? context)
        {
            if (context is null) return;

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var user = CurrentUser();

            foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = user;
                        entry.Entity.UpdatedAt = null;
                        entry.Entity.UpdatedBy = null;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = user;
                        // Creation info is history: never let an update overwrite it.
                        entry.Property(e => e.CreatedAt).IsModified = false;
                        entry.Property(e => e.CreatedBy).IsModified = false;
                        break;
                }
            }
        }

        private string CurrentUser()
        {
            var name = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            return string.IsNullOrWhiteSpace(name) ? SystemUser : name;
        }
    }
}
