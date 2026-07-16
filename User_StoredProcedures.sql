-- =============================================
-- Stored Procedures for User
-- =============================================

-- Create_User
CREATE PROCEDURE Create_User
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Age INT,
    @Name NVARCHAR(100),
    @Gender NVARCHAR(50),
    @IsActive BIT,
    @PhoneNumber NVARCHAR(50),
    @Email NVARCHAR(100)
AS
BEGIN
    INSERT INTO Users (FirstName, LastName, Age, Name, Gender, IsActive, PhoneNumber, Email)
    VALUES (@FirstName, @LastName, @Age, @Name, @Gender, @IsActive, @PhoneNumber, @Email);

    SELECT SCOPE_IDENTITY();
END;
GO

-- User_IfExists
CREATE PROCEDURE User_IfExists
    @Id INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Users WHERE Id = @Id)
        SELECT 1;
    ELSE
        SELECT 0;
END;
GO

-- User_GetAll
CREATE PROCEDURE User_GetAll
AS
BEGIN
    SELECT * FROM Users ORDER BY FirstName;
END;
GO

-- User_GetByID
CREATE PROCEDURE User_GetByID
    @Id INT
AS
BEGIN
    SELECT * FROM Users WHERE Id = @Id;
END;
GO

-- Update_User
CREATE PROCEDURE Update_User
    @Id INT,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Age INT,
    @Name NVARCHAR(100),
    @Gender NVARCHAR(50),
    @IsActive BIT,
    @PhoneNumber NVARCHAR(50),
    @Email NVARCHAR(100)
AS
BEGIN
    UPDATE Users
    SET
        FirstName = @FirstName,
        LastName = @LastName,
        Age = @Age,
        Name = @Name,
        Gender = @Gender,
        IsActive = @IsActive,
        PhoneNumber = @PhoneNumber,
        Email = @Email
    WHERE Id = @Id;
END;
GO

-- User_Delete
CREATE PROCEDURE User_Delete
    @Id INT
AS
BEGIN
    DELETE FROM Users WHERE Id = @Id;
END;
GO
