using CoolMessages.App.Models;
using CoolMessages.App.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace CoolMessages.App.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ConnectionFactory _factory;
        private readonly RabbitMqConfiguration _config;
        public MessagesController(IOptions<RabbitMqConfiguration> options)
        {
            _config = options.Value;

            _factory = new ConnectionFactory
            {
                HostName = _config.Host
            };
        }

        [HttpPost]
        public IActionResult PostMessage([FromBody] MessageInputModel message)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: _config.Queue,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var stringfiedMessage = JsonConvert.SerializeObject(message);
                    var bytesMessage = Encoding.UTF8.GetBytes(stringfiedMessage);

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: _config.Queue,
                        basicProperties: null,
                        body: bytesMessage);
                }
            }

            return Accepted();
        }
    }
}
