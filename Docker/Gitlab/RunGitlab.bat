rem C:\Users\UserName\.wslconfig
rem \\wsl$\docker-desktop-data\version-pack-data\community\docker\volumes

rem nano /etc/gitlab/gitlab.rb external_url "http://127.0.0.1:9080"
rem docker volume create gitlab_config
docker run --detach --hostname localhost --publish 9080:9080 --name gitlab --restart always --volume gitlab_config:/etc/gitlab --volume gitlab_logs:/var/log/gitlab --volume gitlab_data:/var/opt/gitlab gitlab/gitlab-ce:latest

rem gitlab-runner.exe install
rem gitlab-runner.exe start
rem gitlab-runner.exe register

pause