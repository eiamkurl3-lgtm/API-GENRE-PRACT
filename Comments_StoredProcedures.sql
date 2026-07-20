-- =============================================
-- Table and Stored Procedures for Comments
-- =============================================

-- Create Table if not exists
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Comments' AND xtype='U')
CREATE TABLE Comments (
    id INT PRIMARY KEY IDENTITY(1,1),
    Body NVARCHAR(500) NOT NULL,
    MovieId INT NOT NULL,
    FOREIGN KEY (MovieId) REFERENCES Movies(id)
);
GO

-- Create_Comment
CREATE PROCEDURE Create_Comment
    @Body NVARCHAR(500),
    @MovieId INT
AS
BEGIN
    INSERT INTO Comments (Body, MovieId)
    VALUES (@Body, @MovieId);

    SELECT SCOPE_IDENTITY();
END;
GO

-- Comment_IfExists
CREATE PROCEDURE Comment_IfExists
    @Id INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Comments WHERE id = @Id)
        SELECT 1;
    ELSE
        SELECT 0;
END;
GO

-- Get_CommentsByMovieId
CREATE PROCEDURE Get_CommentsByMovieId
    @MovieId INT
AS
BEGIN
    SELECT * FROM Comments
    WHERE MovieId = @MovieId
    ORDER BY id;
END;
GO

-- Comment_GetByID
CREATE PROCEDURE Comment_GetByID
    @Id INT
AS
BEGIN
    SELECT * FROM Comments WHERE id = @Id;
END;
GO

-- Update_Comment
CREATE PROCEDURE Update_Comment
    @Id INT,
    @Body NVARCHAR(500),
    @MovieId INT
AS
BEGIN
    UPDATE Comments
    SET
        Body = @Body,
        MovieId = @MovieId
    WHERE id = @Id;
END;
GO

-- Delete_Comment
CREATE PROCEDURE Delete_Comment
    @Id INT
AS
BEGIN
    DELETE FROM Comments WHERE id = @Id;
END;
GO