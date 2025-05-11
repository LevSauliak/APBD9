using System.Runtime.InteropServices.JavaScript;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;
using Tutorial9.Repositories;

namespace Tutorial9.Services;

public class WarehouseService : IWarehouseService
{
    private IWarehouseRepository _warehouseRepository;

    public WarehouseService(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<Result<int>> AddProductWarehouse(ProductWarehouseDTO productWarehouse)
    {
        int? productPrice = await _warehouseRepository.GetProductPrice(productWarehouse.IdProduct);
        if (productPrice == null)
        {
            return Result<int>.Failure(ErrorType.NotFound, "Product not found");
        }

        if (!await _warehouseRepository.WarehouseExists(productWarehouse.IdWarehouse))
        {
            return Result<int>.Failure(ErrorType.NotFound, "Warehouse not found");
        }

        if (productWarehouse.Amount <= 0)
        {
            return Result<int>.Failure(ErrorType.Validation, "Amount must be greater than 0");
        }

        int? idOrder = await _warehouseRepository.FindOrder(productWarehouse);

        if (idOrder == null)
        {
            return Result<int>.Failure(ErrorType.NotFound, "Order not found");
        }

        if (await _warehouseRepository.OrderCompleted((int)idOrder))
        {
            return Result<int>.Failure(ErrorType.Conflict, "Order is already completed");
        }

        return await _warehouseRepository.FulfillOrder(productWarehouse, (int)idOrder, (int)productPrice);
    }

    public async Task<Result<int>> AddProductWarehouseProcedure(ProductWarehouseDTO productWarehouse)
    {
        try
        {
            int newId = await _warehouseRepository.AddProductToWarehouseProcedure(productWarehouse);
            return Result<int>.Success(newId);
        }
        catch (SqlException e)
        {
            ErrorType type = ErrorType.Internal;

            foreach (SqlError error in e.Errors)
            {
                switch (error.Message)
                {
                    case string msg when msg.Contains("IdProduct does not exist"):
                    case string msg2 when msg2.Contains("IdWarehouse does not exist"):
                        type = ErrorType.NotFound;
                        break;

                    case string msg when msg.Contains("no order to fullfill"):
                        type = ErrorType.Conflict;
                        break;

                    default:
                        type = ErrorType.Internal;
                        break;
                }
            }

            return Result<int>.Failure(type, e.Message);
        }
    }
}