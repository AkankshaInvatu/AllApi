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

namespace HelloWorld
{
    public static class Function1
    {
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


    }
    }
}
