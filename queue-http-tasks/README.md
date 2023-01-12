# Create a regular queue and add HTTP target tasks

In this sample, you'll see how to create a Cloud Tasks queue and add HTTP tasks
to it to target a Cloud Run service.

## Enable APIs

First, make sure the required APIs are enabled:

```sh
gcloud services enable \
  cloudtasks.googleapis.com \
  run.googleapis.com
```

## Deploy a Cloud Run service

Deploy a Cloud Run service which will serve as the target of the HTTP tasks
later:

```sh
SERVICE=hello-a
REGION=us-central1

gcloud run deploy $SERVICE \
  --allow-unauthenticated \
  --image=gcr.io/cloudrun/hello \
  --region=$REGION
```

Save the URL of the service for later:

```sh
SERVICE_URL=$(gcloud run services describe $SERVICE --region $REGION --format 'value(status.url)')
```

## Create a Cloud Tasks queue

Create a regular Cloud Tasks queue:

```sh
QUEUE=http-queue
LOCATION=us-central1

gcloud tasks queues create $QUEUE \
    --location=$LOCATION
```

Pause the queue temporarily, so we can observe HTTP tasks as they are created:

```sh
gcloud tasks queues pause $QUEUE \
    --location=$LOCATION
```

## Create an HTTP task

Create an HTTP task:

```sh
gcloud tasks create-http-task \
    --queue=$QUEUE \
    --location=$LOCATION \
    --url=$SERVICE_URL \
    --method=GET

Created task [projects/atamel-tasks/locations/us-central1/queues/http-queue/tasks/1795553800989510802]
```

At this point, the task is created but it's in pending state as the queue is
paused:

```sh
gcloud tasks queues list \
  --location=$LOCATION

QUEUE_NAME  STATE   MAX_NUM_OF_TASKS  MAX_RATE (/sec)  MAX_ATTEMPTS
http-queue  PAUSED  1000              500.0            100
```

## Test the HTTP task

Resume the queue:

```sh
gcloud tasks queues resume $QUEUE \
    --location=$LOCATION
```

You should see that the Cloud Run service received an HTTP GET request from
Cloud Tasks:

```sh
gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=$SERVICE" --limit 1
---
httpRequest:
  latency: 0.227597158s
  protocol: HTTP/1.1
  remoteIp: 35.243.23.192
  requestMethod: GET
  requestSize: '415'
  requestUrl: https://hello-a-idcwffc3yq-uc.a.run.app/
  responseSize: '5510'
  serverIp: 216.239.32.53
  status: 200
  userAgent: Google-Cloud-Tasks
```
