USE NewsEntities;
GO

INSERT INTO [Roles] (Name) 
VALUES ('ChiefAdmin'),('Admin'),('User');

--Если выполнять не в приложении пароли будут не совпадать из-за хеширования
declare @afterhash varbinary(500) = HASHBYTES('SHA2_256', '123456');
INSERT INTO [Users] (UserName,Email,PasswordHash)
VALUES ('admin','admin@gmail.com',convert(nvarchar(MAX), @afterhash));

INSERT INTO [UserRoles] (UserId,RoleId)
VALUES ((SELECT TOP 1 Id FROM Users WHERE UserName = 'admin'),(SELECT Id FROM Roles WHERE Name = 'ChiefAdmin'));

INSERT INTO [HashTags] (Name) 
VALUES ('Игры'),('Новости'),('Технологии');

INSERT INTO [Articles] (Name,State,Text,UserId)
VALUES ('Test Article',1,'Example',(SELECT ID FROM [Users] WHERE UserName = 'admin'));

INSERT INTO [ArticleHashTags] (ArticleId,HashTagId)
VALUES ((SELECT Id FROM [Articles] WHERE Name = 'Test Article'),(SELECT Id FROM [HashTags] WHERE Name = 'Игры'));

