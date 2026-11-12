using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartPalm.CropService.Application.Abstractions;
using SmartPalm.CropService.Domain.Events;
using SmartPalm.CropService.Infrastructure.Messaging;
using SmartPalm.CropService.Infrastructure.Persistence;

namespace SmartPalm.CropService.Application.EventHandlers;

public class UserDeletionEventHandler(CropDbContext db, IIntegrationEventWriter events)
{
    public async Task HandleAsync(UserDeletionRequestedIntegrationEvent message, CancellationToken token)
    {
        if (await db.InboxMessages.AnyAsync(x => x.Id == message.OperationId, token)) return;

        var plantationIds = await db.Plantations
            .Where(x => x.PalmGrowerId == message.UserId)
            .Select(x => x.Id)
            .ToArrayAsync(token);
        var removedAffiliations = await db.Affiliations
            .Where(x => x.AgronomistId == message.UserId || plantationIds.Contains(x.PlantationId))
            .ExecuteDeleteAsync(token);
        var removedPlantations = await db.Plantations
            .Where(x => plantationIds.Contains(x.Id))
            .ExecuteDeleteAsync(token);

        db.InboxMessages.Add(new InboxMessage(message.OperationId, "smartpalm.identity.user-deletion-requested.v1"));
        events.Enqueue("smartpalm.identity.user-deletion-service-completed.v1",
            new UserDeletionServiceCompletedIntegrationEvent(
                message.OperationId, message.UserId, "crop", removedAffiliations + removedPlantations, DateTime.UtcNow));
        await db.SaveChangesAsync(token);
    }
}
