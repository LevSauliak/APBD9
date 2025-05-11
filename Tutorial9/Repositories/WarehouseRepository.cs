using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IConfiguration _configuration;
    public WarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> ProductExists(int id)
    {
        string command = "SELECT 1 FROM Product WHERE IdProduct = @Id";
        await using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand(command, con);
        cmd.Parameters.AddWithValue("@Id", id);

        await con.OpenAsync();

        return 1 == Convert.ToInt32( await cmd.ExecuteScalarAsync());
    }

    public async Task<bool> WarehouseExists(int id)
    {
        string command = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @Id";
        await using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand(command, con);
        cmd.Parameters.AddWithValue("@Id", id);

        await con.OpenAsync();

        return 1 == Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<int?> FindOrder(ProductWarehouseDTO productWarehouseDTO)
    {
        string command = "SELECT TOP 1 IdOrder FROM \"Order\" WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt AND FulfilledAt IS NULL";
        
        await using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand(command, con);
        cmd.Parameters.AddWithValue("@IdProduct", productWarehouseDTO.IdProduct);
        cmd.Parameters.AddWithValue("@Amount", productWarehouseDTO.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt", productWarehouseDTO.CreatedAt);

        await con.OpenAsync();

        var id = await cmd.ExecuteScalarAsync();

        if (id == DBNull.Value || id == null) 
        {
            return null;
        }

        return Convert.ToInt32(id);
    }

    public async Task<bool> OrderExists(int id)
    {
        string command = "SELECT 1 FROM \"Order\" WHERE IdOrder == @IdOrder";
        
        await using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand(command, con);
        cmd.Parameters.AddWithValue("@IdOrder", id);

        await con.OpenAsync();
        
        return 1 == Convert.ToInt32( await cmd.ExecuteScalarAsync());
    }

    public async Task<bool> OrderCompleted(int id)
    {
        string command = "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder";
        
        await using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand(command, con);
        cmd.Parameters.AddWithValue("@IdOrder", id);

        await con.OpenAsync();
        
        return 1 == Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<Result<int>> FulfillOrder(ProductWarehouseDTO productWarehouse, int idOrder, int productPrice)
    {
        string insertCommand = @"
            INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) 
            OUTPUT INSERTED.IdProductWarehouse
            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)
        ";

        string updateCommand = "UPDATE \"Order\" SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";

        await using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default"));
        await con.OpenAsync();
        await using var transaction = await con.BeginTransactionAsync();
        
        try
        {
            await using SqlCommand updateCmd = new SqlCommand(updateCommand, con);

            updateCmd.Transaction = transaction as SqlTransaction;
            updateCmd.Parameters.AddWithValue("@IdOrder", idOrder);
            updateCmd.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);

            await updateCmd.ExecuteNonQueryAsync();

            await using SqlCommand insertCmd = new SqlCommand(insertCommand, con);
            insertCmd.Transaction = transaction as SqlTransaction;
            insertCmd.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
            insertCmd.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
            insertCmd.Parameters.AddWithValue("@IdOrder", idOrder);
            insertCmd.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
            insertCmd.Parameters.AddWithValue("@Price", productPrice * productWarehouse.Amount);
            insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            
            int id =  Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
            
            await transaction.CommitAsync();
            return Result<int>.Success(id);
        }
        catch
        {
            await transaction.RollbackAsync();
            return Result<int>.Failure(ErrorType.Internal, "Internal system error when fulfilling order");
        }

    }

    public async Task<int?> GetProductPrice(int id)
    {
        string command = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
        
        await using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand(command, con);
        
        cmd.Parameters.AddWithValue("@IdProduct", id);
        
        await con.OpenAsync();
        
        var price = await cmd.ExecuteScalarAsync();

        if (price == DBNull.Value || price == null) return null;

        return Convert.ToInt32(price);
    }

    public async Task<int> AddProductToWarehouseProcedure(ProductWarehouseDTO productWarehouse) 
    {
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        command.CommandText = "AddProductToWarehouse";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
        command.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);
        
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }
}