# Create HTTP Task with BufferTask API sample - C#

A sample to show how to create HTTP tasks with BufferTask API.

```sh
PROJECT_ID=$(gcloud config get-value project)
LOCATION=us-central1
QUEUE=http-queue-uri-override-buffer
ACCESS_TOKEN=$(gcloud auth application-default print-access-token)

dotnet run $PROJECT_ID $LOCATION $QUEUE $ACCESS_TOKEN
```
