using System;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using BlazorKafkaUi.Models;

namespace BlazorKafkaUi.Controllers
{
    public abstract class KafkaControllerBase : Controller
    {
        protected abstract KafkaConsumeQueue ConsumeQueue { get; }
        protected abstract KafkaProduceQueue ProduceQueue { get; }

        [HttpGet("kafka/consume")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult KafkaConsume([FromQuery] long? offset)
        {
            if (ConsumeQueue is null)
                return BadRequest("Consume queue for this doesn't exist");
            if (!ModelState.IsValid || offset is null)
                return BadRequest("Invalid offset");

            KafkaMessage message;
            lock (ConsumeQueue.Lock)
            {
                message = ConsumeQueue.Messages
                    .Where(m => m.Offset > Math.Max(ConsumeQueue.LastConsumedOffset, offset.Value))
                    .OrderBy(m => m.Offset).FirstOrDefault();
            }

            if (message is null)
                return NoContent();
            return Ok(message);
        }

        [HttpPost("kafka/consume")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult KafkaCommit([FromBody] long? offset)
        {
            if (ConsumeQueue is null)
                return BadRequest("Consume queue for this doesn't exist");
            if (!ModelState.IsValid || offset is null || offset <= 0)
                return BadRequest("Invalid offset");

            lock (ConsumeQueue.Lock)
                ConsumeQueue.LastConsumedOffset = offset.Value;
            return Ok();
        }

        [HttpPost("kafka/produce")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult KafkaProduce([FromBody] string message)
        {
            if (ProduceQueue is null)
                return BadRequest("Produce queue for this doesn't exist");
            if (!ModelState.IsValid || string.IsNullOrEmpty(message))
                return BadRequest("Invalid message");

            lock (ProduceQueue.Lock)
            {
                ProduceQueue.Messages.Enqueue(new KafkaMessage()
                {
                    Topic = ProduceQueue.Name,
                    Partition = 1,
                    Offset = ++ProduceQueue.LastOffset,
                    MessageKey = null,
                    MessageValue = message,
                    MessageTimestamp = DateTime.UtcNow,
                });
            }
            return Ok();
        }
    }
}
