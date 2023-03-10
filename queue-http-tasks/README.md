# Create a regular queue for HTTP target tasks

In this sample, you'll see how to create a Cloud Tasks queue and add HTTP tasks
to it to target a Cloud Run service.

![Architecture](architecture.png)

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
SERVICE1=hello1
REGION=us-central1

gcloud run deploy $SERVICE1 \
  --allow-unauthenticated \
  --image=gcr.io/cloudrun/hello \
  --region=$REGION
```

Save the URL of the service for later:

```sh
SERVICE1_URL=$(gcloud run services describe $SERVICE1 --region $REGION --format 'value(status.url)')
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

Create an HTTP task with `gcloud`:

```sh
gcloud tasks create-http-task \
    --queue=$QUEUE \
    --location=$LOCATION \
    --url=$SERVICE1_URL \
    --method=GET
```

You can also create an HTTP task with client libraries. For example, you can
check out the [Program.cs](./client-libraries/csharp/Program.cs) for a C# sample
where an HTTP request is wrapped into a `Task` and then a `TaskRequest` before being
sent to Cloud Tasks with the `CloudTasksClient`:

```csharp
var taskRequest = new CreateTaskRequest
{
    Parent = new QueueName(projectId, location, queue).ToString(),
    Task = new Task
    {
        HttpRequest = new HttpRequest
        {
            HttpMethod = HttpMethod.Get,
            Url = url
        }
    }
};

var client = CloudTasksClient.Create();
var response = client.CreateTask(taskRequest);
```

You can run it as follows:

```sh
dotnet run $PROJECT_ID $LOCATION $QUEUE $SERVICE1_URL
```

## Test the HTTP task

At this point, the task is created but it's in pending state as the queue is
paused:

```sh
gcloud tasks queues list \
  --location=$LOCATION

QUEUE_NAME  STATE   MAX_NUM_OF_TASKS  MAX_RATE (/sec)  MAX_ATTEMPTS
http-queue  PAUSED  1000              500.0            100
```

Resume the queue:

```sh
gcloud tasks queues resume $QUEUE \
    --location=$LOCATION
```

You should see that the Cloud Run service received an HTTP GET request from
Cloud Tasks:

```sh
gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=$SERVICE1" --limit 1
---
httpRequest:
  latency: 0.227597158s
  protocol: HTTP/1.1
  remoteIp: 35.243.23.192
  requestMethod: GET
  requestSize: '415'
  requestUrl: https://hello1-idcwffc3yq-uc.a.run.app/
  responseSize: '5510'
  serverIp: 216.239.32.53
  status: 200
  userAgent: Google-Cloud-Tasks
```
