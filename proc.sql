CREATE PROCEDURE AddProductToWarehouse @IdProduct INT, @IdWarehouse INT, @Amount INT,
@CreatedAt DATETIME
AS
BEGIN

 DECLARE @IdProductFromDb INT, @IdOrder INT, @Price DECIMAL(5,2);

 SELECT @IdProductFromDb=Product.IdProduct, @Price=Product.Price FROM Product WHERE IdProduct=@IdProduct

 IF @IdProductFromDb IS NULL
 BEGIN
  RAISERROR('Invalid parameter: Provided IdProduct does not exist', 18, 0);
  RETURN;
 END;

 IF NOT EXISTS(SELECT 1 FROM Warehouse WHERE IdWarehouse=@IdWarehouse)
 BEGIN
  RAISERROR('Invalid parameter: Provided IdWarehouse does not exist', 18, 0);
  RETURN;
 END;

 SELECT TOP 1 @IdOrder=IdOrder FROM "Order"
 WHERE IdProduct=@IdProductFromDb AND Amount=@Amount AND
 CreatedAt<@CreatedAt AND FulfilledAt IS NULL;

 IF @IdOrder IS NULL
 BEGIN
  RAISERROR('Invalid parameter: There is no order to fullfill', 18, 0);
  RETURN;
 END;

 SET XACT_ABORT ON;
 BEGIN TRAN;

 UPDATE "Order" SET
 FulfilledAt=@CreatedAt
 WHERE IdOrder=@IdOrder;

 INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
 VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Amount*@Price, @CreatedAt);

 SELECT @@IDENTITY AS NewId;

 COMMIT;
END
