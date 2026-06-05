namespace PokeStore.Api.Core.Interfaces;

using PokeStore.Api.Core.Entities;

public interface IProcessedWebhookRepository
{
    Task<ProcessedWebhook?> GetByWebhookIdAsync(string webhookId);
    Task<ProcessedWebhook?> GetByTransactionIdAsync(string transactionId);
    Task<ProcessedWebhook> CreateAsync(ProcessedWebhook webhook);
    Task<ProcessedWebhook> UpdateAsync(ProcessedWebhook webhook);
}
