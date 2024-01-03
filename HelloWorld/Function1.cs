using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Text;

namespace HelloWorld
{
    public static class Function1
    {


       // private const string ServiceBusConnectionString = "Endpoint=sb://helloservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=VjMz12seAoA4clIlI/gKXW8Tr+U4SIdpv+ASbLo/7Kk=";
        private const string QueueName = "helloqueue";

        [FunctionName("HelloWorld")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req
            )
        {


            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "pass a string"
                : $"Hello, {name}. function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        private static List<StudentModelcs> persons = new List<StudentModelcs>();

        [FunctionName("CreatePerson")]
        public static async Task<IActionResult> CreatePerson(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "persons")] HttpRequest req
            )
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newPerson = JsonConvert.DeserializeObject<StudentModelcs>(requestBody);

            persons.Add(newPerson);

            return new OkObjectResult(newPerson);
        }

        [FunctionName("GetPersons")]
        public static IActionResult GetPersons(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "persons")] HttpRequest req
           )
        {
            return new OkObjectResult(persons);
        }

        [FunctionName("UpdatePerson")]
        public static async Task<IActionResult> UpdatePerson(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "persons/{id}")] HttpRequest req,
            int id,
            ILogger log)
        {
            var existingPerson = persons.Find(p => p.Id == id);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedPerson = JsonConvert.DeserializeObject<StudentModelcs>(requestBody);

            existingPerson.Name = updatedPerson.Name;
            existingPerson.Age = updatedPerson.Age;
            existingPerson.Id = updatedPerson.Id;

            return new OkObjectResult(existingPerson);
        }

        [FunctionName("DeletePerson")]
        public static IActionResult DeletePerson(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "persons/{id}")] HttpRequest req,
            int id
            )
        {
            var personToDelete = persons.FirstOrDefault(p => p.Id == id);

            if (personToDelete == null)
            {
                return new NotFoundResult();
            }

            persons.Remove(personToDelete);

            return new OkResult();
        }


        [FunctionName("PatchPerson")]
        public static async Task<IActionResult> PatchPerson(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "persons/{id}")] HttpRequest req,
            ILogger log, int id)
        {


            var personToPatch = persons.FirstOrDefault(p => p.Id == id);

            if (personToPatch == null)
            {
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var patchData = JsonConvert.DeserializeObject<JsonPatchDocument<StudentModelcs>>(requestBody);

            patchData.ApplyTo(personToPatch);

            return new OkObjectResult(personToPatch);
        }

        [FunctionName("UploadImage")]
        public static async Task<IActionResult> Run3(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var formCollection = await req.ReadFormAsync();
                var file = formCollection.Files["file"];

                if (file != null && file.Length > 0)
                {
                    string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=helloworld20231212134615;AccountKey=QnCdqNQMyY7HTs4OZB3SygURAu8GFvHvMhXop5uatpfr2bMzM3rWXNpOUelgqphzC194K58OSBmI+AStqkNjtw==;EndpointSuffix=core.windows.net";

                    CloudStorageAccount storageAccount;
                    if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
                    {
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference("images");

                        await container.CreateIfNotExistsAsync();

                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.FileName);
                        using (var stream = file.OpenReadStream())
                        {
                            await blockBlob.UploadFromStreamAsync(stream);
                        }

                        return new OkObjectResult($"File uploaded successfully: {file.FileName}");
                    }
                    else
                    {
                        return new BadRequestObjectResult("Invalid storage connection string");
                    }
                }
                else
                {
                    return new BadRequestObjectResult("No file found in the request");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Exception: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        [FunctionName("CreateDocument")]
        public static async Task<IActionResult> Run5(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "documents")] HttpRequest req,
        [CosmosDB(
            databaseName: "1",
           containerName: "ContainerApi1",
           Connection = "CosmosDBConnectionString",
            CreateIfNotExists = true,
            PartitionKey = "/document")] IAsyncCollector<MyDocument> documents,
        ILogger log)
        {


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<MyDocument>(requestBody);



            await documents.AddAsync(data);

            return new OkObjectResult($"Document added: {data.Id}");
        }

        [FunctionName("UpdateDocument")]
        public static async Task<IActionResult> Run6(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "documents/{id}")] HttpRequest req,
        [CosmosDB(
            databaseName: "1",
            containerName: "ContainerApi1",
            Connection = "CosmosDBConnectionString")] IAsyncCollector<MyDocument> documents,
        [CosmosDB(
            databaseName: "1",
            containerName: "ContainerApi1",
            Connection = "CosmosDBConnectionString",

            Id = "{id}")] MyDocument existingDocument,
        ILogger log, string id)
        {
            log.LogInformation($"UpdateDocument function triggered for id: {id}");

            if (existingDocument == null)
            {
                log.LogInformation($"Document with id {id} not found.");
                return new NotFoundResult();
            }

            try
            {
                // Read the request body and update the existing document
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                MyDocument updatedDocument = JsonConvert.DeserializeObject<MyDocument>(requestBody);

                // Update properties of existingDocument with values from updatedDocument
              
                existingDocument.Update(updatedDocument);

                // Save the updated document back to Cosmos DB
                await documents.AddAsync(existingDocument);

                log.LogInformation($"Document with id {id} updated successfully.");

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Error updating document with id {id}: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }


        [FunctionName("SendMessageToQueue")]
        public static async Task<IActionResult> SendMessageToQueue(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [ServiceBus(QueueName, Connection = "ServiceBusConnectionString")] IAsyncCollector<string> messages,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            await messages.AddAsync(requestBody);

            log.LogInformation($"Message added to the queue: {requestBody}");

            return new OkResult();
        }

           
        }


    }

   


