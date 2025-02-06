using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThroughputMetricController : ControllerBase
    {
        // GET: api/<ThroughputController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ThroughputController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        // PUT api/<ThroughputController>/5
        [HttpPut("{id}")]
        public void Put(Guid requestId, int count)
        {
            
        }

        // POST api/<ThroughputController>
        [HttpPost]
        public void Post(Guid requestId, int count)
        {
        }

        // DELETE api/<ThroughputController>/5
        [HttpDelete("{id}")]
        public void Delete(Guid requestId, int count)
        {
        }
    }
}
