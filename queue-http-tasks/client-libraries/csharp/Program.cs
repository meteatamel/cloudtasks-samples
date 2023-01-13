// Copyright 2023 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using Google.Cloud.Tasks.V2;
using HttpMethod = Google.Cloud.Tasks.V2.HttpMethod;
using Task = Google.Cloud.Tasks.V2.Task;

var commandArgs = Environment.GetCommandLineArgs();
var projectId = commandArgs[1];
var location = commandArgs[2];
var queue = commandArgs[3];
var url = commandArgs[4];


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

Console.WriteLine($"Created Task {response.Name}");