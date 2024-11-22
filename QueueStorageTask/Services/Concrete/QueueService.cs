using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using QueueStorageTask.Services.Abstract;

namespace QueueStorageTask.Services.Concrete;

public class QueueService : IQueueService
{
    private readonly QueueClient queueClient;

    public QueueService(string connectionString, string queueName)
    {
        queueClient = new QueueClient(connectionString, queueName);
        queueClient.CreateIfNotExists();
    }

    public async Task DeleteMessageAsync(string messageId, string popReceipt)
    {
       await queueClient.DeleteMessageAsync(messageId, popReceipt);
    }

    public async Task<QueueMessage> ReceiveMessageAsync()
    {
        var messageResponse = await queueClient.ReceiveMessageAsync(TimeSpan.FromSeconds(2));
        if (messageResponse.Value != null)
        {
            var message = messageResponse.Value;
            // deleting message after reading :
            // await queueClient.DeleteMessageAsync(messageResponse.Value.MessageId,messageResponse.Value.PopReceipt);
            return message;
        }
        return null;
    }

    public async Task SendMessageAsync(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            await queueClient.SendMessageAsync(message);
        }
    }
}
