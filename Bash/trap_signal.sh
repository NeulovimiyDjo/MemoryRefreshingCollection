#!/bin/bash
#./1.sh &
#while true; do echo 11; sleep 1; done &
while true; do
  if [ -f /var/log_files/nginx/access.log ]; then
    wc -l /var/log_files/nginx/access.log | awk '{print $1}' > /nginxwc/current.txt
  fi
  logrotate -s /tmp/logrotate.status /etc/logrotate.d/nginx
  sleep 29
done &
child_background=$!
echo "Started background process with pid $child_background"

_sig_handler() {
  echo "Caught SIGTERM or SIGINT signal"
  kill -TERM $child_main 2>/dev/null
  kill -TERM $child_background 2>/dev/null
  echo "Sent SIGTERM to children"
  wait $child_main
  echo "Main child graceful shutdown complete"
  wait $child_background
  echo "Background child graceful shutdown complete"
}
trap _sig_handler SIGTERM SIGINT

#./2.sh &
#while true; do echo 22; sleep 1; done &
nginx -g "daemon off;error_log /dev/stdout;" &
child_main=$!
echo "Started main process with pid $child_main"

wait $child_main
#sleep 5
#ps -aux
echo "Root shutdown complete"
