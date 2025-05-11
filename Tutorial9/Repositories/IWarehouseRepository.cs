using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Tutorial9.Model;

namespace Tutorial9.Repositories;

public interface IWarehouseRepository
    
{
    Task<bool> ProductExists(int id);
    
    Task<bool> WarehouseExists(int id);

    Task<int?> FindOrder(ProductWarehouseDTO pwdto);
    
    Task<bool> OrderExists(int id);

    Task<bool> OrderCompleted(int id);
    
    Task<Result<int>> FulfillOrder(ProductWarehouseDTO productWarehouse, int idOrder, int productPrice);

    Task<int?> GetProductPrice(int id);

    Task<int> AddProductToWarehouseProcedure(ProductWarehouseDTO productWarehouse);
}