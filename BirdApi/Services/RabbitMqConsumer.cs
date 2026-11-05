using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using BirdApi.Data;
using BirdApi.Models;

namespace BirdApi.Services;

public class RabbitMQConsumer : BackgroundService
{
    private readonly IConnection _connection;//-------------для соеденения с rabbit mq
    private readonly IServiceProvider _serviceProvider;//---для работы с бд и т.д. втроенный интерфейс .NET

    public RabbitMQConsumer(IConnection connection, IServiceProvider serviceProvider)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)//---асинхронная функция для принятния сообщения с продюсера ребита, CancellationToken stoppingToken для остановки при ctrl c в терминале 
    {
        await using var channel = await _connection.CreateChannelAsync();//---создание умного какала связи, await using означает что канал закроется при выполнении или ошибке, _connection.CreateChannelAsync() передаем соеденение и создаем канал связи
        await channel.QueueDeclareAsync(
            queue: "history_queue", 
            durable: false, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null);//-----------------------------------------------проверяет связь с такими параметрами, она прописана в RabbitMqProducer
        var consumer = new AsyncEventingBasicConsumer(channel);//-------------создаем слушателя по этой связи
        //---делает из слушателя некое асинхронное событие, когда слушатель поймает сообщение по этому каналу то после этого активируется эта функция, model - обьет сообщения от ребит, ea - вся информация которая нужна
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();//-----------------------------------переводим байты информации в строку
            var messageJson = Encoding.UTF8.GetString(body);//----------------переводим ее в строку
            
            try
            {
                using var scope = _serviceProvider.CreateScope();//-----------врменное подлючение к бд для AppDbContext
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();//---создаем прямую переменную для работой с бд

                var data = JsonSerializer.Deserialize<MessageDto>(messageJson);//-------------преобразовываем строку похожую на json в экземляр класса MessageDto 

                if (data != null)//-----------------------------------------------------------если ответ не пусто то
                {
                    var rabbitAnswer = new MainTable
                    {
                        UserId = data.UserId,
                        Type = data.Type,
                        IdTable = data.IdTable,
                        Species = data.Species,
                        InputQuery = data.InputQuery,
                        CreatedAt = DateTime.UtcNow
                    };//-----------------------------------------создаем из данных типо экземпляр для отправку в бд BaseQuary

                    dbContext.MainTables.Add(rabbitAnswer);//---запись в бд но настоящее сохранение в бд строкой ниже
                    await dbContext.SaveChangesAsync();//--------сохранение изменений в бд
                }
            }
            catch (Exception ex)//-------------------------------если ошибка то вывод
            {
                Console.WriteLine($"Ошибка при сохранении истории: {ex.Message}");
            }
        };
        //---типо подтверждение работы с этим запросом
        await channel.BasicConsumeAsync(
            queue: "history_queue",
            autoAck: true,
            consumer: consumer);

        //---чтобы эта функция работала бесконечно пока сервер не запроется
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
//---для преобразования ответа от сервера в этот дто класс
public class MessageDto
{
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int IdTable { get; set; }
    public string? Species { get; set; }
    public string? InputQuery { get; set; }
}