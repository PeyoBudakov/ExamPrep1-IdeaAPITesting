using IdeaAPI12_08_2025.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace IdeaAPI12_08_2025
{
    public class IdeaAPI12_08_2025

    {
        private RestClient client;
        private const string BASEURL = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";
        private const string EMAIL = "oyep@example.com";
        private const string PASSWORD = "123123";

        private static string lastIdeaId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(EMAIL, PASSWORD);

            var options = new RestClientOptions(BASEURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            RestClient authClient = new RestClient(BASEURL);
            var request = new RestRequest("/api/User/Authentication");
            request.AddJsonBody(new
            {
                email,
                password
            });

            var response = authClient.Execute(request, Method.Post);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Access Token is null or empty");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected response type {response.StatusCode} with data {response.Content}");
            }
        }

        [OneTimeTearDown] public void TearDown() { this.client.Dispose(); }

        [Test, Order(1)]
        public void CreateANewIdea_WithTheRequiredFields_ShouldSucceed()
        {
            //Arrange
            var newIdea = new IdeaDTO
            {
                Title = "Test Idea Brev",
                Description = "Some Description",
                Url = ""
            };

            //Act
            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(newIdea);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Msg, Is.EqualTo("Successfully created!"));
        }

        [Test, Order(2)]
        public void GetAllIdeas_ShouldSucceed()
        {
            //Arrange

            //Act
            var request = new RestRequest("/api/Idea/All", Method.Get);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseDataArray = JsonSerializer.Deserialize<ApiResponseDTO[]>(response.Content);
            Assert.That(responseDataArray.Length, Is.GreaterThan(0));

            lastIdeaId = responseDataArray[responseDataArray.Length - 1].ideaId;

            Console.WriteLine(lastIdeaId);

        }

        [Test, Order(3)]
        public void EditTheLastIdeaYouCreated_ShouldSucceed() 
        {
            //Arrange
            var requestData = new IdeaDTO()
            {
                Title = "editedTestTitle",
                Description = "TestDescription with edits",
            };

            //Act
            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", lastIdeaId);
            request.AddJsonBody(requestData);
            var response = client.Execute(request, Method.Put);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(responseData.Msg, Is.EqualTo("Edited successfully"));

        }

        [Test, Order(4)]
        public void DeleteTheIdeaThatYouEdited_ShouldSucceed()
        {
            //Arrange

            //Act
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", lastIdeaId);
            var response = client.Execute(request, Method.Delete);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response.Content, Does.Contain("The idea is deleted!"));

        }

        [Test, Order(5)]
        public void CreateANewIdea_WithoutTheRequiredFields_ShouldFail()
        {
            //Arrange
            var newIdea = new IdeaDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };

            //Act
            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(newIdea);
            var response = this.client.Execute(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

          }

        [Test, Order(6)]
        public void EditNonExistentIdea_ShouldFail()
        {
            //Arrange
            var requestData = new IdeaDTO()
            {
                Title = "editedTestTitle",
                Description = "TestDescription with edits",
            };

            //Act
            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", 666111);
            request.AddJsonBody(requestData);
            var response = client.Execute(request, Method.Put);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            //var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.Content, Does.Contain("There is no such idea!"));

        }

        [Test, Order(7)]
        public void DeleteNonExistentIdea_ShouldFail()
        {
            //Arrange

            //Act
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", 765123);
            var response = client.Execute(request, Method.Delete);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            Assert.That(response.Content, Does.Contain("There is no such idea!"));

        }
    }
}