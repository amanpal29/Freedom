-- Sets all passwords in the database to be the same as the administrator password.

UPDATE [User]
SET [Password] =
(
	SELECT [Password]
	FROM [User]
	WHERE [Id] = 'DA378215-F0A8-4F47-AD19-48002D0B8E67'
);
