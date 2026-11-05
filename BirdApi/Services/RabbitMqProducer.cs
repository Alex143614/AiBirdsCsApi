using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using BirdApi.Models;

namespace BirdApi.Services;

public class RabbitMqProducer
{
    private readonly IConnection _connection;

    public RabbitMqProducer(IConnection connection)
    {
        _connection = connection;
    }
    //---асинхроная операя через rabbit mq которая отправляет запрос консумеру на добавление записи в таблицу BaseQuery
    public async Task SendRabbitMqMessageAsync(int userId, string type, int idTable, string? species, string? inputQuery)
    {
        await using var channel = await _connection.CreateChannelAsync();//--создание канала связи
        await channel.QueueDeclareAsync(
            queue: "history_queue", 
            durable: false, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null);//----------------------------------------------создание очереди для работы с rabbit mq

        //---формирование сообщения для отправки в переменную
        var messageObj = new { UserId = userId, Type = type, IdTable = idTable, Species = species, InputQuery = inputQuery };
        //---преобразования переменой в json
        var messageJson = JsonSerializer.Serialize(messageObj);
        //---превращение json в масив байтов, слышал так надо для каналов rabbitmq
        var body = Encoding.UTF8.GetBytes(messageJson);
        //---отправка сообщения
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "history_queue",
            body: body);
    }
}