server

```
docker build -t agones-magiconionchatapp:0.1.3 -f ChatApp.Server/Dockerfile .
docker tag agones-magiconionchatapp:0.1.3 guitarrapc/agones-magiconionchatapp:0.1.3
docker push guitarrapc/agones-magiconionchatapp:0.1.3
```

```
kubectl apply -f ./k8s/fleet-magiconionchatapp.yaml
kubectl get fleet -w
kubectl delete -f ./k8s/fleet-magiconionchatapp.yaml
```


match

```
docker build -t magiconionchatapp-match:0.0.1 -f ChatApp.Match/Dockerfile .
docker tag magiconionchatapp-match:0.0.1 guitarrapc/magiconionchatapp-match:0.0.1
docker push guitarrapc/magiconionchatapp-match:0.0.1
```

```
kubectl kustomize ./k8s/ | kubectl apply -f -
```
