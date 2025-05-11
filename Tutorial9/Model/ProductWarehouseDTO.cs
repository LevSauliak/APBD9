using System.ComponentModel.DataAnnotations;

namespace Tutorial9.Model;

public class ProductWarehouseDTO
{
    public required int IdProduct { get; set; }
    
    public required int IdWarehouse { get; set; }
    
    [Range(1, int.MaxValue)]
    public required int Amount { get; set; }
    
    public required DateTime CreatedAt { get; set; }
}