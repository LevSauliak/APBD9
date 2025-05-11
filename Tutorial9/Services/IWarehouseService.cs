using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;

namespace Tutorial9.Services;

public interface IWarehouseService
{

    public Task<Result<int>> AddProductWarehouse(ProductWarehouseDTO productWarehouse);
    
    public Task<Result<int>> AddProductWarehouseProcedure(ProductWarehouseDTO productWarehouse);
    
}