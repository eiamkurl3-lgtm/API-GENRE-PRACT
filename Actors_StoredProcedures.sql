-- =============================================
-- Stored Procedures for Actors
-- =============================================

-- Actor_IfExistsByName
CREATE PROCEDURE Actor_IfExistsByName
    @Id INT,
    @FirstName NVARCHAR(75),
    @LastName NVARCHAR(75)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Actors 
               WHERE FirstName = @FirstName 
               AND LastName = @LastName 
               AND Id <> @Id)
        SELECT 1;
    ELSE
        SELECT 0;
END;
GO
