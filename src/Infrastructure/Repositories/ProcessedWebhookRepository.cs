namespace PokeStore.Api.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using PokeStore.Api.Core.Entities;
using PokeStore.Api.Core.Interfaces;
using PokeStore.Api.Infrastructure.Data;

/// <summary>
/// Repository implementation for ProcessedWebhook operations
/// </summary>
public class ProcessedWebhookRepository : IProcessedWebhookRepository
{
    private readonly PokestoreDbContext _context;

    public ProcessedWebhookRepository(PokestoreDbContext context)
    {
        _context = context;
    }

    public async Task<ProcessedWebhook?> GetByWebhookIdAsync(string webhookId)
    {
        return await _context.ProcessedWebhooks
            .FirstOrDefaultAsync(w => w.WebhookId == webhookId);
    }

    public async Task<ProcessedWebhook?> GetByTransactionIdAsync(string transactionId)
    {
        return await _context.ProcessedWebhooks
            .FirstOrDefaultAsync(w => w.TransactionId == transactionId);
    }

    public async Task<ProcessedWebhook> CreateAsync(ProcessedWebhook webhook)
    {
        _context.ProcessedWebhooks.Add(webhook);
        await _context.SaveChangesAsync();
        return webhook;
    }

    public async Task<ProcessedWebhook> UpdateAsync(ProcessedWebhook webhook)
    {
        _context.ProcessedWebhooks.Update(webhook);
        await _context.SaveChangesAsync();
        return webhook;
    }
}
