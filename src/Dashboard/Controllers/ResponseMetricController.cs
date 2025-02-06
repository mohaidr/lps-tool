using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponseMetricController : ControllerBase
    {
        // GET: api/<ResponseMetricController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ResponseMetricController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        // PUT api/<ResponseMetricController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // POST api/<ResponseMetricController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            throw new NotSupportedException();
        }

        // DELETE api/<ResponseMetricController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            throw new NotSupportedException();
        }
    }
}
