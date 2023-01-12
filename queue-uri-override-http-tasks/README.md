# Create a queue with uri override and add HTTP target tasks

> **Note:** *Queue-level Task Routing Configuration* is an experimental feature
> in *preview*. Only allow-listed projects can currently take advantage of it.

This sample builds on the previous [Create a regular queue and add HTTP target
tasks](../queue-http-tasks/) sample. Make sure you go through that sample before
continuing with this one.

In this sample, you'll see how to create a Cloud Tasks queue with a uri override
and add HTTP tasks to it to target a secondary Cloud Run service.

## Deploy a second Cloud Run service

In the first sample, you deployed a Cloud Run service (`hello-a`). Deploy a
second Cloud Run service which will serve as the target of the HTTP uri override
later:

```sh
SERVICE_B=hello-b
REGION=us-central1

gcloud run deploy $SERVICE_B \
  --allow-unauthenticated \
  --image=gcr.io/cloudrun/hello \
  --region=$REGION
```

Save the host part of URL of the service for later:

```sh
SERVICE_B_URL=$(gcloud run services describe $SERVICE_B --region $REGION --format 'value(status.url)')
SERVICE_B_HOST=$(echo $SERVICE_B_URL | sed 's,http[s]*://,,g')
```

## Setup for Queue-level Task Routing Configuration

*Queue-level Task Routing Configuration* is currently an experimental feature.
As such, it doesn't have proper `gcloud` support. Instead, we will use `curl`.

First, get an access token:

```sh
gcloud auth application-default login
ACCESS_TOKEN=$(gcloud auth application-default print-access-token)
```

Set some environment variables that we'll use later:

```sh
PROJECT_ID=$(gcloud config get-value project)
LOCATION=us-central1
QUEUES_PATH=projects/$PROJECT_ID/locations/$LOCATION/queues
TASKS_API="https://cloudtasks.googleapis.com/v2beta3"
TASKS_QUEUES_API=$TASKS_API/$QUEUES_PATH
```

## Create a Cloud Tasks queue

Create a queue with HTTP target uri override. Note that, the uri override refers
to the second Cloud Run service:

```sh
QUEUE=http-queue-uri-override

curl -X POST $TASKS_QUEUES_API \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d @- << EOF
{
  "name": "$QUEUES_PATH/$QUEUE",
  "httpTarget": {"uriOverride":{"host":"$SERVICE_B_HOST"}}
}
EOF
```

Pause the queue temporarily, so we can observe HTTP tasks as they are created:

```sh
gcloud tasks queues pause $QUEUE \
    --location=$LOCATION
```

## Create an HTTP task

Create an HTTP task. Note that we're using the first service's URL still:

```sh
gcloud tasks create-http-task \
    --queue=$QUEUE \
    --location=$LOCATION \
    --url=$SERVICE_A_URL \
    --method=GET
```

At this point, the task is created but it's in pending state as the queue is
paused:

```sh
gcloud tasks queues list \
  --location=$LOCATION

QUEUE_NAME               STATE    MAX_NUM_OF_TASKS  MAX_RATE (/sec)  MAX_ATTEMPTS
http-queue               RUNNING  1000              500.0            100
http-queue-uri-override  PAUSED   1000              500.0            100
```

## Test the HTTP task

Resume the queue:

```sh
gcloud tasks queues resume $QUEUE \
    --location=$LOCATION
```

You should see that the second (not the first) Cloud Run service received an HTTP GET request from
Cloud Tasks due to the override:

```sh
gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=$SERVICE_B" --limit 1
---
httpRequest:
  latency: 0.228982142s
  protocol: HTTP/1.1
  remoteIp: 35.187.132.84
  requestMethod: GET
  requestSize: '426'
  requestUrl: https://hello-b-idcwffc3yq-uc.a.run.app/
  responseSize: '5510'
  serverIp: 216.239.34.53
  status: 200
  userAgent: Google-Cloud-Tasks
```
