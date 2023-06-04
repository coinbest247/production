using Core.Application.Implementation;
using Core.Application.Interfaces;
using Core.Data.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Core.Web.Controllers
{
    public class TrafficController : Controller
    {
        private readonly IHttpService _httpService;
        public TrafficController(IHttpService httpService)
        {
            _httpService = httpService;
        }



        public IActionResult Index()
        {
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTraffics(DateTime startDate)
        {
            var model = await _httpService.GetTraffics(startDate);

            return new OkObjectResult(new { Results =  model.viewer.zones[0].httpRequests1dGroups[0].sum.countryMap });
        }

        
    }
}
