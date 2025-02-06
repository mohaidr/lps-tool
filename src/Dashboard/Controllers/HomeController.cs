using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apis.Controllers
{
    public class HomeController : Controller
    {
        [Route("{*path}")]
        public IActionResult Index(string path)
        {
            return View(); // This will render the 'Index.cshtml' view
        }
    }
}
