#setup local k3d cluster (on win-local machine turn off IIS cuz it occupies 80 and 443 ports on host)
docker run -d --name registry --restart=unless-stopped -p 5555:5000 registry:2.7.0
k3d cluster create dev --image rancher/k3s:v1.20.15-k3s1 -p 80:80@loadbalancer -p 443:443@loadbalancer --k3s-arg "--disable=traefik@server:0" -v $pwd/registries.yaml:/etc/rancher/k3s/registries.yaml
helm repo update
helm upgrade --install --create-namespace ingress-nginx ingress-nginx --repo https://kubernetes.github.io/ingress-nginx -n ingress-nginx --set "controller.extraArgs.enable-ssl-passthrough="

---------------------------------------------------------------
#setup elasticsearch and kibana
docker run -d --name elasticsearch --restart=unless-stopped -p 9200:9200 -e "discovery.type=single-node" elasticsearch:7.17.5
docker run -d --name kibana --restart=unless-stopped -p 5601:5601 -e "ELASTICSEARCH_HOSTS=http://host.docker.internal:9200" kibana:7.17.5

---------------------------------------------------------------
#access services
https://localhost/srv-app1
curl -X GET http://localhost:5555/v2/_catalog
curl -X GET http://localhost:5555/v2/web/tags/list
http://localhost:9200
http://localhost:5601

---------------------------------------------------------------
#fix dns after nodes restart (after computer restart)
kubectl -n kube-system edit configmaps coredns -o yaml
add to NodeHosts line (192.168.65.2 here is some magic ip that host.docker.internal resolves to):
192.168.65.2 host.k3d.internal
kubectl delete pods --all -n testns

---------------------------------------------------------------
#disable all pods in cluster
kubectl scale deployment --replicas=0 --all -n testns
