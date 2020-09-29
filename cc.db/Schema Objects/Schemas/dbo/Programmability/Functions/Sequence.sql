CREATE FUNCTION [dbo].[Sequence](@min INT, @max INT, @step INT)
RETURNS @ret TABLE (id INT PRIMARY KEY)
AS
BEGIN
    WITH numbers(id) as
    (
        SELECT @min id
        UNION ALL 
        SELECT id+@step
        FROM numbers
        WHERE id < @max
    )
    INSERT @ret 
    SELECT id FROM Numbers
    OPTION(MAXRECURSION 0)
    RETURN
END