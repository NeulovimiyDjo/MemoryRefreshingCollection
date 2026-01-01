USE msdb
GO

IF OBJECT_ID('tempdb..#RecreateJob') IS NOT NULL
BEGIN
    DROP PROC #RecreateJob
END
GO

CREATE PROCEDURE #RecreateJob
(
	@JobName nvarchar(255),
	@JobDescription nvarchar(max),
	@JobCommand nvarchar(max),
	@IntervalSeconds int
)
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			DECLARE @JobStepName nvarchar(255) = @JobName + N'_step'
			DECLARE @JobScheduleName nvarchar(255) = @JobName + N'_schedule'

			-- Delete job if it already exists:
			IF EXISTS(SELECT job_id FROM msdb.dbo.sysjobs WHERE (name = @JobName))
			BEGIN
				EXEC msdb.dbo.sp_delete_job
					@job_name = @JobName
			END

			--Add a job
			EXEC dbo.sp_add_job
				@job_name = @JobName,
				@enabled=1,
				@description=@JobDescription

			--Add a job step named process step. This step runs the stored procedure
			EXEC sp_add_jobstep
				@job_name = @JobName,
				@step_name = @JobStepName,
				@subsystem = N'TSQL',
				@command = @JobCommand,
				@database_name=N'Test_DB_Name'

			--Schedule the job at a specified date and time
			exec sp_add_jobschedule
				@job_name = @JobName,
				@name = @JobScheduleName,
				@enabled=1, 
				@freq_type=4, -- Daily
				@freq_interval=1, 
				@freq_subday_type=2, -- Seconds
				@freq_subday_interval=@IntervalSeconds, 
				@freq_relative_interval=0, 
				@freq_recurrence_factor=0, 
				@active_start_date=20210101, 
				@active_end_date=99991231, 
				@active_start_time=000029, 
				@active_end_time=235959

			-- Add the job to the SQL Server Server
			EXEC dbo.sp_add_jobserver
				@job_name =  @JobName,
				@server_name = N'(local)'

		COMMIT TRAN
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN

		DECLARE @ErrorMessage nvarchar(max), @ErrorSeverity int, @ErrorState int;
		SELECT @ErrorMessage = ERROR_MESSAGE() + ' Line ' + cast(ERROR_LINE() as nvarchar(5)), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
		raiserror(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO


DECLARE @JobName nvarchar(255) = N'job_test1'
DECLARE @JobDescription nvarchar(max) = N'test job'
DECLARE @JobCommand nvarchar(max) = N'
USE Test_DB_Name
EXEC sp_test1
EXEC sp_test2
'
DECLARE @IntervalSeconds int = 60
EXEC #RecreateJob @JobName=@JobName, @JobDescription=@JobDescription, @JobCommand=@JobCommand, @IntervalSeconds=@IntervalSeconds
GO


IF OBJECT_ID('tempdb..#RecreateJob') IS NOT NULL
BEGIN
    DROP PROC #RecreateJob
END
GO