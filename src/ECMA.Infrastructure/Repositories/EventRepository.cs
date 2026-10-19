using ECMA.Domain.Entities;
using ECMA.Domain.Enums;
using ECMA.Domain.Interfaces;
using ECMA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECMA.Infrastructure.Repositories;

public sealed class EventRepository(EcmaDbContext context) : IEventRepository
{
    public async Task<Event?> FindAsync(Guid id, bool forUpdate, CancellationToken cancellationToken = default)
    {
        if (forUpdate)
            await context.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM events WHERE id = {id} AND NOT is_deleted FOR UPDATE", cancellationToken);
        return await context.Events.Include(x => x.AgendaItems).SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> ListAsync(EventQuery query, DateTime now, CancellationToken cancellationToken = default)
    {
        IQueryable<Event> items = context.Events.AsNoTracking().Where(x => !x.IsDeleted);
        if (query.Role != UserRole.Admin) items = items.Where(x => x.OwnerId == query.ActorId);
        if (query.Keyword is not null) items = items.Where(x => EF.Functions.ILike(x.Title, $"%{query.Keyword}%"));
        if (query.Type is not null) items = items.Where(x => x.Type == query.Type);
        if (query.Status is EffectiveEventStatus.Completed) items = items.Where(x => x.Status == EventStatus.Published && x.EndAt <= now);
        else if (query.Status is EffectiveEventStatus.Published) items = items.Where(x => x.Status == EventStatus.Published && x.EndAt > now);
        else if (query.Status is not null) items = items.Where(x => x.Status == (EventStatus)query.Status);
        return await items.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id).Skip((query.Page - 1) * 20).Take(20).ToListAsync(cancellationToken);
    }

    public void Add(Event item) => context.Events.Add(item);
}
