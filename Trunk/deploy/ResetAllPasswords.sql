-- Sets all passwords in the database to be the same as the administrator password.

UPDATE [User]
SET [Password] =
(
	SELECT [Password]
	FROM [User]
	WHERE [Id] = '3B526C4E-50F3-425D-9787-6DB0696290FF'
);
