using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController(IWarehouseService warehouseService) : ControllerBase
    {
        [HttpPost("v1")]
        public async Task<IActionResult> AddProductWarehouseV1([FromBody] ProductWarehouseDTO productWarehouse)
        {
            Console.WriteLine("v1");
            var result = await warehouseService.AddProductWarehouse(productWarehouse);
            return result.ToActionResult();
        }

        [HttpPost("v2")]
        public async Task<IActionResult> AddProductWarehouseV2([FromBody] ProductWarehouseDTO productWarehouse)
        {
            Console.WriteLine("v2");
            var result = await warehouseService.AddProductWarehouseProcedure(productWarehouse);
            return result.ToActionResult();
        }
    }
}