SELECT DISTINCT
  con.session_id,
  req.command AS current_cmd,
  cmd.text AS last_cmd_text,
  req.blocking_session_id,
  STUFF ((SELECT  ', ' + obj.name + ':' + obj.type_desc
    FROM sys.dm_tran_locks locks
    LEFT JOIN sys.objects obj ON obj.object_id = locks.resource_associated_entity_id
    WHERE locks.request_session_id = con.session_id AND locks.resource_database_id = ses.database_id
  FOR XML PATH('')), 1, 2, '') AS blocked_objects,
  req.start_time AS current_cmd_start_time,
  req.status AS current_cmd_status,
  req.wait_time AS current_cmd_wait_time,
  req.wait_type AS current_cmd_wait_type,
  ses.host_name,
  con.client_net_address,
  ses.program_name,
  ses.host_process_id,
  ses.login_name,
  ses.login_time
FROM sys.dm_exec_connections con
INNER JOIN sys.dm_exec_sessions ses ON ses.session_id = con.session_id
OUTER APPLY sys.dm_exec_sql_text(con.most_recent_sql_handle) cmd
LEFT JOIN sys.dm_exec_requests req ON req.session_id = con.session_id
WHERE ses.database_id = DB_ID()



SELECT
  blocked_requests.session_id AS blocked_session_id,
  blocked_requests.command AS blocked_cmd,
  blocked_requests_cmd.text AS blocked_cmd_text,
  blocked_requests_ses.program_name AS blocked_cmd_program_name,
  blocked_requests_ses.host_process_id AS blocked_cmd_host_process_id,
  STUFF ((SELECT  ', ' + obj.name
    FROM sys.dm_tran_locks locks
    LEFT JOIN sys.objects obj ON obj.object_id = locks.resource_associated_entity_id
    WHERE locks.request_session_id = blocked_requests_con.session_id AND locks.resource_database_id = blocked_requests_ses.database_id
  FOR XML PATH('')), 1, 2, '') AS blocked_objects,
  blocked_requests.wait_time AS blocked_cmd_wait_time,
  blocked_requests.wait_type AS blocked_cmd_wait_type,
  blocked_requests.start_time AS blocked_cmd_start_time,
  blocked_requests.status AS blocked_cmd_status,
  blocking_info.session_id AS blocking_session_id,
  blocking_info.command AS blocking_cmd,
  blocking_info.text AS blocking_cmd_text,
  blocking_info.blocking_session_id AS blocking_cmd_blocked_by_session_id,
  blocking_info.start_time AS blocking_cmd_start_time,
  blocking_info.status AS blocking_cmd_status,
  blocking_info.host_name AS blocking_cmd_host_name,
  blocking_info.client_net_address AS blocking_cmd_client_net_address,
  blocking_info.program_name AS blocking_cmd_program_name,
  blocking_info.host_process_id AS blocking_cmd_host_process_id,
  blocking_info.login_name AS blocking_cmd_login_name,
  blocking_info.login_time AS blocking_cmd_login_time
FROM sys.dm_exec_requests blocked_requests
INNER JOIN sys.dm_exec_connections blocked_requests_con ON blocked_requests_con.session_id = blocked_requests.session_id
INNER JOIN sys.dm_exec_sessions blocked_requests_ses ON blocked_requests_ses.session_id = blocked_requests_con.session_id
OUTER APPLY sys.dm_exec_sql_text(blocked_requests_con.most_recent_sql_handle) blocked_requests_cmd
INNER JOIN
(
  SELECT DISTINCT
    con.session_id,
    req.command,
    cmd.text,
    req.blocking_session_id,
    req.start_time,
    req.status,
    ses.host_name,
    con.client_net_address,
    ses.program_name,
    ses.host_process_id,
    ses.login_name,
    ses.login_time
  FROM sys.dm_exec_connections con
  INNER JOIN sys.dm_exec_sessions ses ON ses.session_id = con.session_id
  OUTER APPLY sys.dm_exec_sql_text(con.most_recent_sql_handle) cmd
  LEFT JOIN sys.dm_exec_requests req ON req.session_id = con.session_id
) blocking_info ON blocking_info.session_id = blocked_requests.blocking_session_id
WHERE blocked_requests.database_id = DB_ID()
  AND blocked_requests.blocking_session_id > 0
  AND blocked_requests_con.most_recent_sql_handle <> 0x0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
ORDER BY blocked_requests.session_id
