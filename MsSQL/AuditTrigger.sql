IF (OBJECT_ID(N'dbo.SomeTableAuditLog') IS NOT NULL)
BEGIN
	DROP TABLE dbo.SomeTableAuditLog
END
GO

CREATE TABLE SomeTableAuditLog
(
	event_info nvarchar(max),
	program_name nvarchar(max),
	host_name nvarchar(max),
	user_id int,
	login_name nvarchar(max),
	login_time datetime,
	request_start_time datetime,
	last_request_start_time datetime,
	last_request_end_time datetime,
	connect_time datetime,
	session_id smallint,
	event_type nvarchar(max),
	parameters smallint
)
GO



IF (OBJECT_ID(N'dbo.SomeTableAuditLogTrigger') IS NOT NULL)
BEGIN
	DROP TRIGGER dbo.SomeTableAuditLogTrigger
END
GO


CREATE TRIGGER dbo.SomeTableAuditLogTrigger
ON  dbo.SomeTable 
AFTER UPDATE, INSERT
AS 
BEGIN
	IF EXISTS (SELECT TOP 1 1 FROM Inserted i where i.SomeField='Something')
	BEGIN

		SET NOCOUNT ON;

		--DECLARE @inputbuffer TABLE (EventType NVARCHAR(30),Parameters INT,EventInfo NVARCHAR(4000))
		--INSERT INTO @inputbuffer EXEC('dbcc inputbuffer('+@@Spid+') WITH NO_INFOMSGS')
		--SELECT * FROM @inputbuffer AS I

		INSERT INTO SomeTableAuditLog
		(
			event_info,
			program_name,
			host_name,
			user_id,
			login_name,
			login_time,
			request_start_time,
			last_request_start_time,
			last_request_end_time,
			connect_time,
			session_id,
			event_type,
			parameters
		)
		SELECT
			buf.event_info,
			ses.program_name,
			ses.host_name,
			req.user_id,
			ses.login_name,
			ses.login_time,
			req.start_time,
			ses.last_request_start_time,
			ses.last_request_end_time,
			con.connect_time,
			req.session_id,
			buf.event_type,
			buf.parameters
		FROM sys.dm_exec_input_buffer (@@Spid, 0) buf
		INNER JOIN sys.dm_exec_requests req on req.session_id=@@Spid
		INNER JOIN sys.dm_exec_connections con on con.session_id=req.session_id
		INNER JOIN sys.dm_exec_sessions ses on ses.session_id=req.session_id

	END
END
GO