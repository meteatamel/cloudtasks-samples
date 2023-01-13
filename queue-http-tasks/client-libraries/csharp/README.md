# Create HTTP Task sample - C#

A sample to show how to create HTTP tasks.

```sh
PROJECT_ID=$(gcloud config get-value project)
LOCATION=us-central1
QUEUE=http-queue
URL=$(gcloud run services describe hello1 --region $LOCATION --format 'value(status.url)')

dotnet run $PROJECT_ID $LOCATION $QUEUE $URL
```
