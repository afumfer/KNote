USE [KNote03DesaDB]
GO 

--------------------------------------------------
-- Users
--------------------------------------------------
ALTER TABLE [dbo].[Users] NOCHECK CONSTRAINT ALL  
GO

--DELETE from [dbo].[Users]
--GO

INSERT INTO [dbo].[Users]  (    
	[UserName]
	,[FullName]
	,[EMail]
	,[Disabled]
	,[UserId]
) 
SELECT DISTINCT Usuario, Usuario + ' - ' + Usuario as FullName, Usuario + '@gobcan.org' as Expr3, 0 AS Expr1, NEWID() AS Expr2
FROM              TareasDesarrolloDB.dbo.Notas
GROUP BY Usuario
ORDER BY Usuario

GO

ALTER TABLE [dbo].[Users] CHECK CONSTRAINT ALL  
GO
--------------------------------------------------


--------------------------------------------------
-- Folders
--------------------------------------------------
ALTER TABLE [dbo].[Folders] NOCHECK CONSTRAINT ALL  
GO

DELETE from [dbo].[Folders]
GO

INSERT INTO [dbo].[Folders] (
[FolderId], 
[FolderNumber],
[Name], 
[CreationDateTime], 
[ModificationDateTime], 
[Order], 
[OrderNotes], 
[ParentId]
)
SELECT NEWID ( ), [IdCarpeta], [NombreCarpeta], getdate() as d1, getdate() as d2, [Orden], [OrdenNotas], null
FROM [TareasDesarrolloDB].[dbo].[Carpetas]
GO 

UPDATE Folders 
	set ParentId = Folders_1.FolderId     
FROM Folders 
   INNER JOIN [TareasDesarrolloDB].[dbo].[Carpetas] AS TCarpetas ON Folders.FolderNumber = TCarpetas.IdCarpeta
   INNER JOIN   Folders AS Folders_1 ON TCarpetas.IdCarpetaPadre = Folders_1.FolderNumber
WHERE        
   (TCarpetas.IdCarpetaPadre > 0)

GO

ALTER TABLE [dbo].[Folders] CHECK CONSTRAINT ALL  
GO
----------------------------------------------------



--------------------------------------------------
-- Notes
--------------------------------------------------
ALTER TABLE [dbo].[Notes] NOCHECK CONSTRAINT ALL  
GO

DELETE from [dbo].[Notes]
GO

INSERT INTO [dbo].[Notes]  (
	[NoteId]
	,[NoteNumber]
	,[Topic]
	,[CreationDateTime]
	,[ModificationDateTime]
	,[Description]
	,[InternalTags]
	,[Tags] 
	,[Priority]
	,[FolderId]	
) 
SELECT        
	newid()
	, TNotas.IdNota, TNotas.Asunto
	, TNotas.FechaHoraCreacion, TNotas.FechaModificacion 
	, TNotas.Nota
	, TNotas.Vinculo, TNotas.PalabrasClave
	, TNotas.Prioridad
	, Folders.FolderId	
FROM            
      TareasDesarrolloDB.dbo.Notas as TNotas INNER JOIN
    Folders ON TNotas.IdCarpeta = Folders.FolderNumber INNER JOIN
    Users ON TNotas.Usuario = Users.UserName COLLATE Modern_Spanish_CI_AI
GO

ALTER TABLE [dbo].[Notes] CHECK CONSTRAINT ALL  
GO
----------------------------------------------------



--------------------------------------------------
-- KMessagaes
--------------------------------------------------
ALTER TABLE [dbo].[KMessages]  NOCHECK CONSTRAINT ALL  

DELETE from [dbo].[KMessages]
GO

INSERT INTO [dbo].[KMessages] (
      [KMessageId]
      ,[AlarmDateTime]
      ,[AlarmActivated]
      ,[AlarmOk]
      ,[AlarmType] 
	  ,[Disabled]
      ,[NotificationType]
      ,[NoteId]       
      ,[UserId] 
	  ,[Content]     
	  ,[ActionType]
) 
SELECT
	NEWID() AS Expr1 
	, TNotas.Alarma
	, TNotas.ActivarAlarma
	, TNotas.AlarmaOk
	, 0 AS Expr2
	, 0 AS Expr3
	, TNotas.TipoAlarma
	,Notes.NoteId AS idN1
	,Users.UserId AS U1	
	,'-' 
	,0
FROM 
	  TareasDesarrolloDB.dbo.Notas as TNotas INNER JOIN
	Notes ON TNotas.IdNota = Notes.NoteNumber INNER JOIN 
    Users ON TNotas.Usuario = Users.UserName COLLATE Modern_Spanish_CI_AI
                         
WHERE Alarma > '19000101'
GO

ALTER TABLE [dbo].[KMessages] CHECK CONSTRAINT ALL  
----------------------------------------------------




--------------------------------------------------
-- Windows
--------------------------------------------------
ALTER TABLE [dbo].[Windows] NOCHECK CONSTRAINT ALL  

DELETE from [dbo].[Windows] 
GO

INSERT INTO [dbo].[Windows]  (    
	  [WindowId]
      ,[Visible] --    
      ,[AlwaysOnTop] -- 
      ,[PosX] -- 
      ,[PosY] -- 
      ,[Width] -- 
      ,[Height] -- 
      ,[FontName]
      ,[FontSize]
      ,[FontBold]
      ,[FontItalic]
      ,[FontUnderline]
      ,[FontStrikethru]
      ,[ForeColor]
	  ,[NoteColor]
      ,[TitleColor] -- 
      ,[TextTitleColor]      
      ,[TextNoteColor]
      ,[NoteId]      
      ,[UserId]
) 
SELECT      
	newid()  	
	,[Visible]	
	, [SiempreArriba]
	, [PosX]
	, [PosY]
	, [Ancho]
	, [Alto]
	, [FontName]
	, [FontSize]
	, [FontBold]
	, [FontItalic]
	, [FontUnderline]
	, [FontStrikethru]
	, [ForeColor]
	, [ColorNota]
	, [ColorBanda]
	, [ColorBanda] as CTB
	, [ColorTextoBanda]	
	, Notes.NoteId
	, Users.UserId
FROM            
	  TareasDesarrolloDB.dbo.Notas as TNotas INNER JOIN
    Users ON TNotas.Usuario COLLATE Modern_Spanish_CI_AS = Users.UserName INNER JOIN
    Notes ON TNotas.IdNota = Notes.NoteNumber
GO

ALTER TABLE [dbo].[Windows] CHECK CONSTRAINT ALL  
--------------------------------------------------






----------------------------------------------------
-- Tasks
----------------------------------------------------
ALTER TABLE [dbo].[NoteTasks] NOCHECK CONSTRAINT ALL  

DELETE from [dbo].[NoteTasks]
GO

INSERT INTO [dbo].[NoteTasks]  ( 
	[NoteTaskId]
    ,[NoteId]	
    ,[UserId]
	,[CreationDateTime]
	,[ModificationDateTime]	
	,[Description]
	,[Tags]
	,[Priority]
	,[Resolved]
	,[EstimatedTime]
	,[SpentTime]
	,[DifficultyLevel]
	,[ExpectedStartDate]
	,[ExpectedEndDate]
	,[StartDate]
	,[EndDate]
) 
SELECT        
	NEWID() AS Expr1, 
	Notes.NoteId, 	
	Users.UserId AS Expr2, 
	TNotas.FechaHoraCreacion, 
	TNotas.FechaModificacion, 
	TNotas.Asunto, 
	TNotas.PalabrasClave, 
	TNotas.Prioridad, 
	TNotas.Resuelto, 
    TNotas.TiempoEstimado, 
	TNotas.TiempoInvertido, 
	TNotas.NivelDificultad, 
	TNotas.FechaPrevistaInicio, 
    TNotas.FechaPrevistaFin, 
	TNotas.FechaInicio, 
	TNotas.FechaResolucion
FROM
      TareasDesarrolloDB.dbo.Notas as TNotas INNER JOIN
    Users ON TNotas.Usuario COLLATE Modern_Spanish_CI_AS = Users.UserName INNER JOIN
    Notes ON TNotas.IdNota = Notes.NoteNumber
GO

ALTER TABLE [dbo].[NoteTasks] CHECK CONSTRAINT ALL  
----------------------------------------------------



----------------------------------------------
-- Attributes 
--------------------------------------------------
ALTER TABLE [dbo].[KAttributes] NOCHECK CONSTRAINT ALL  
GO

DELETE FROM [dbo].[KAttributes]
go

INSERT INTO [dbo].[KAttributes] (
	[KAttributeId] 
	,[Key]
	,[Name] 
	,[Script] 
	,[KAttributeDataType] 
	,[RequiredValue]
	, [Order]
	,[Disabled]	
)
SELECT 
	  newid() as nid 
      ,[Seccion] AS Sec
      ,[Valor]
	  ,[Variable]
	  ,0
	  ,0
	  ,1000
	  ,0
	  
--FROM [TareasDesarrolloDB].[dbo].[_Sys] as SYS Where Variable <> 'REC'
FROM [TareasDesarrolloDB].[dbo].[_Sys] as SYS Where Variable = '[!EtiquetaRaiz]' or Variable = 'UU' and Seccion <> 'TB' and Seccion <> 'ND'
GO

ALTER TABLE [dbo].[KAttributes] CHECK CONSTRAINT ALL  
GO
-----------------------------------------------


----------------------------------------------
-- AttributeTabulatedValues
--------------------------------------------------
--select Seccion, Valor, Variable as SS from [TareasDesarrolloDB].[dbo].[_Sys] as SYS Where Variable <> '[!EtiquetaRaiz]' and Variable <> 'UU' 
--   and SUBSTRING (Variable, 1, 2) <> 'TB' and SUBSTRING (Variable, 1, 2) <> 'ND' and Seccion <> '[!sys]'

ALTER TABLE [dbo].KAttributeTabulatedValues NOCHECK CONSTRAINT ALL  
GO

DELETE FROM [dbo].KAttributeTabulatedValues
go

INSERT INTO [dbo].KAttributeTabulatedValues (
	[KAttributeTabulatedValueId]
	,[KAttributeId] 
	,[Key]
	,[Value] 
	,[Order]	
)
SELECT  
	  newid() as nid 
      ,(select distinct [KAttributeId] from [dbo].[KAttributes] as ATt Where ATt.[Key] COLLATE Modern_Spanish_CI_AI = SYS.Variable COLLATE Modern_Spanish_CI_AI) as AttrID
      ,[Seccion]
	  ,[Valor] 
	  ,0 as OrdenX  
FROM [TareasDesarrolloDB].[dbo].[_Sys] as SYS Where Variable <> '[!EtiquetaRaiz]' and Variable not in ('UU', 'REC')
   and SUBSTRING (Variable, 1, 2) <> 'TB' and SUBSTRING (Variable, 1, 2) <> 'ND'
   --and Seccion <> '[!sys]'
   and Seccion not in('[!sys]', 'PRT', 'F1', 'F2', 'F3', 'F4', 'F5', 'F6', 'F7', 'F8', 'F9', 'F10')

   
GO

ALTER TABLE [dbo].KAttributeTabulatedValues CHECK CONSTRAINT ALL  
GO







-- ////////////////////////////
-- ////////////////////////////
--- select * from [TareasDesarrolloDB].[dbo].[_Sys] where Seccion = 'afumfer'
--Select * FROM [TareasDesarrolloDB].[dbo].[_Sys] as SYS Where Variable <> 'REC' and Variable = '[!EtiquetaRaiz]'
--Select * FROM [TareasDesarrolloDB].[dbo].[_Sys] as SYS Where Variable = '[!EtiquetaRaiz]'  or Variable = 'UU'
--Select * FROM [TareasDesarrolloDB].[dbo].[_Sys] as SYS Where Variable = 'UC'




--UPDATE [Atributes]
--	set ParentId = (Select top 1 AtributeId from Atributes as AtParent where AtParent.[Key] = [Atributes].[PathAtribute])

