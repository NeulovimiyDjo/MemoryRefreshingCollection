SELECT session_id, blocking_session_id, start_time, wait_type, wait_type
FROM sys.dm_exec_requests
WHERE blocking_session_id > 0;

SELECT DISTINCT DEC.session_id, DST.text AS 'SQL'
FROM sys.dm_exec_requests AS DER
  JOIN sys.dm_exec_connections AS DEC
    ON DER.blocking_session_id = DEC.session_id
  CROSS APPLY sys.dm_exec_sql_text(DEC.most_recent_sql_handle) AS DST;