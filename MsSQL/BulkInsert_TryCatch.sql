BEGIN TRY
	BEGIN TRAN


	If(OBJECT_ID('tempdb..#TempDbName') Is Not Null)
	Begin
		Drop Table #TempDbName
	End

	CREATE TABLE #TempDbName
	(
		ID UNIQUEIDENTIFIER,
		SomeName NVARCHAR(50) NULL
	)

	BULK
	INSERT #TempDbName
	FROM 'D:\SomePath\filename.csv'
	WITH
	(
		FIELDTERMINATOR = ';',
		ROWTERMINATOR = '\n'
	)


	If(OBJECT_ID('tempdb..#TempDbName') Is Not Null)
	Begin
		Drop Table #TempDbName
	End


	COMMIT TRAN
END TRY
BEGIN CATCH
	ROLLBACK TRAN
	--PRINT ERROR_MESSAGE()
	
	declare @ErrorMessage nvarchar(max), @ErrorSeverity int, @ErrorState int;
	select @ErrorMessage = ERROR_MESSAGE() + ' Line ' + cast(ERROR_LINE() as nvarchar(5)), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
	raiserror(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH