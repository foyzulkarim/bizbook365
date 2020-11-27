using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace BizBook365.Sale.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RabbitMessageController : ControllerBase
    {
        [HttpGet]
        public bool Get()
        {
            var factory = new ConnectionFactory() { HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "events", type: ExchangeType.Fanout);

                //channel.QueueDeclare(queue: "hello-y", durable: false, exclusive: false, autoDelete: false, arguments: null);

                string message = $"{Guid.NewGuid().ToString()} at {DateTime.Now:G}";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "events", routingKey: "", basicProperties: null, body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            return true;
        }
    }
}
