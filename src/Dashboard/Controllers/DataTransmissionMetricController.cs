using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataTransmissionMetricController : ControllerBase
    {
        // GET: api/<DataTransmissionMetricController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<DataTransmissionMetricController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        // PUT api/<DataTransmissionMetricController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // POST api/<DataTransmissionMetricController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            throw new NotSupportedException();
        }

        // DELETE api/<ThroughputController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            throw new NotSupportedException();
        }
    }
}
