using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace OrderSoft {
	public class OSClient {
		public const string CLIENT_ID = "dotnet";

		private static readonly HttpClient httpClient = new HttpClient();
		private string sessionId;
		private int accessLevel;
		private Uri endpoint;

		public OSClient () {
			httpClient.DefaultRequestHeaders.Add("client", CLIENT_ID);
		}

		/// <summary>
		///   Check if newUrl is an OrderSoft server and, if it is, set endpoint.
		/// </summary>
		public async Task Init (string newUrl) {
			var provisionalEndpoint = new Uri(Path.Combine(newUrl, "api/"));

			var vals = new TestRequest();
			vals.Test = true;

			var response = await sendRequest("test", vals, provisionalEndpoint);

			// Check Content-Type of response
			if (response.Content.Headers.ContentType.ToString() != "application/json; charset=utf-8") {
				Console.WriteLine(response.Content.Headers.ContentType.ToString());
				var exceptionText = newUrl + " is not an OrderSoft server (did not return JSON)";
				throw new NotOrderSoftServerException(exceptionText);
			}
			var responseBody = await getResponseObject<Response>(response);

			// Validate response
			if (responseBody.ServerVersion != null) {
				if (response.StatusCode != HttpStatusCode.OK) {
					throw new Exception(responseBody.Reason);
				} else {
					endpoint = provisionalEndpoint;
				}
			} else {
				var exceptionText = newUrl + " is not an OrderSoft server (did not return a server_version)";
				throw new NotOrderSoftServerException(exceptionText);
			}
		}

		/// <summary>
		///   Logs in and sets sessionId
		/// </summary>
		public async Task Login (string username, string password) {
			var vals = new LoginRequest();
			vals.Username = username;
			vals.Password = password;

			var response = await sendRequest("login", vals);
			var responseBody = await getResponseObject<LoginResponse>(response);

			if (response.StatusCode == HttpStatusCode.OK) {
				sessionId = responseBody.SessionId;
				accessLevel = responseBody.AccessLevel;

				// Add sessionId header to httpClient
				httpClient.DefaultRequestHeaders.Add("sessionId", sessionId);
			} else if (response.StatusCode == HttpStatusCode.Unauthorized) {
				throw new IncorrectDetailsException(responseBody.Reason);
			} else {
				// this shouldn't happen
				throw new Exception(responseBody.Reason);
			}
		}

		/// <summary>
		///   Get order object, given an orderId or table number
		/// </summary>
		public async Task<OrderObject> GetOrder (string orderId = null, int? tableNum = null) {
			// orderId and tableNum are optional and are intended to be passed
			// as named arguments, as per documentation
			var vals = new GetOrderRequest();

			// Set body values for request
			if (orderId != null) vals.OrderId = orderId;
			if (tableNum.HasValue) vals.TableNumber = tableNum;

			// Send request
			var rawResponse = await sendRequest("getOrder", vals);
			var responseBody = await getResponseObject<GetOrderResponse>(rawResponse);

            // Check that request returned HTTP OK
            if (rawResponse.StatusCode == HttpStatusCode.BadRequest) {
                throw new MalformedRequestException(responseBody.Reason);
            } else if (rawResponse.StatusCode == HttpStatusCode.NotFound) {
                throw new NotFoundException(responseBody.Reason);
            } else if (rawResponse.StatusCode == HttpStatusCode.Unauthorized
                      || rawResponse.StatusCode == HttpStatusCode.Forbidden) {
                throw new UnauthenticatedException(responseBody.Reason);
            }

            return responseBody.Order;
		}

        /// <summary>
        ///   Send order object to server to update/set new order
        /// </summary>
        public async Task<string> SetOrder (OrderObject toSet) {
            var vals = new SetOrderRequest();
            vals.Order = toSet;

            // Send request
            var rawResponse = await sendRequest("setOrder", vals);
            var responseBody = await getResponseObject<SetOrderResponse>(rawResponse);

            // Check for errors
            if (rawResponse.StatusCode == HttpStatusCode.BadRequest) {
                throw new MalformedRequestException(responseBody.Reason);
            } else if (rawResponse.StatusCode == HttpStatusCode.NotFound) {
                throw new NotFoundException(responseBody.Reason);
            } else if (rawResponse.StatusCode == HttpStatusCode.Unauthorized
                      || rawResponse.StatusCode == HttpStatusCode.Forbidden) {
                throw new UnauthenticatedException(responseBody.Reason);
            }

            return responseBody.OrderId;
        }

        /// <summary>
        ///   Search database for dishes and return based on criteria
        /// </summary>
        public async Task<DishObject[]> GetDishes (string category = null, 
                                                   int? dishId = null, 
                                                   float? minPrice = null, 
                                                   float? maxPrice = null) {
            // Each parameter should be passed as a named parameter
            var vals = new GetDishesRequest();

            // Populate body values
            if (category != null) vals.Category = category;
            if (dishId.HasValue) vals.DishId = dishId;
            if (minPrice.HasValue) vals.MinPrice = minPrice;
            if (maxPrice.HasValue) vals.MaxPrice = maxPrice;

            // Send request
            var rawResponse = await sendRequest("getDishes", vals);
            var responseBody = await getResponseObject<GetDishesResponse>(rawResponse);

            // Check for errors
            if (rawResponse.StatusCode == HttpStatusCode.BadRequest) {
                throw new MalformedRequestException(responseBody.Reason);
            } else if (rawResponse.StatusCode == HttpStatusCode.NotFound) {
                throw new NotFoundException(responseBody.Reason);
            } else if (rawResponse.StatusCode == HttpStatusCode.Unauthorized
                      || rawResponse.StatusCode == HttpStatusCode.Forbidden) {
                throw new UnauthenticatedException(responseBody.Reason);
            }

            return responseBody.Results;
        }

		/// <summary>
		///   Gets a list of open orders from the server
		/// </summary>
		public async Task<string[]> GetOpenOrders () {
			var vals = new RequestBody(); // blank request for openOrders

			var rawResponse = await sendRequest("openOrders", vals);
			var responseBody = await getResponseObject<OpenOrdersResponse>(rawResponse);

			return responseBody.OpenOrders;
		}

		/// <summary>
		///   Gets a list of unpaid orders from the server
		/// </summary>
		public async Task<string[]> GetUnpaidOrders () {
			var vals = new RequestBody(); // blank request for unpaidOrders

			var rawResponse = await sendRequest("unpaidOrders", vals);
			var responseBody = await getResponseObject<UnpaidOrdersResponse>(rawResponse);

			return responseBody.UnpaidOrders;
		}

		/// <summary>
		///   Given an HttpResponseMessage, returns an object by deserialising the content JSON.
		/// </summary>
		private async Task<T> getResponseObject<T>(HttpResponseMessage response) {
			var responseText = await response.Content.ReadAsStringAsync();
			responseText = parseResponse(responseText);

			// Create settings object to disregard null values
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
			return JsonConvert.DeserializeObject<T>(responseText, settings);
		}

		/// <summary>
		///   Parse string received from server so JsonConvert can use it
		///   TODO find a better way of doing this??
		/// </summary>
		private static string parseResponse (string inString) {
			return inString.Trim('"').Replace("\\", "");
		}

		/// <summary>
		///   Sends request to endpoint/relativeUrl, using bodyVals
		/// </summary>
		private Task<HttpResponseMessage> sendRequest (String relativeUrl, RequestBody bodyVals, Uri currentEndpoint = null) {
			if (currentEndpoint == null) currentEndpoint = endpoint;
			if (currentEndpoint == null) throw new NotInitiatedException();

			var urlToPost = new Uri(currentEndpoint, relativeUrl);

			// Create setting so serializer ignores null values
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.MissingMemberHandling = MissingMemberHandling.Ignore;

			// Convert dict to JSON, StringContent
			var bodyJSON = JsonConvert.SerializeObject(bodyVals, settings);
			var bodyContent = new StringContent(bodyJSON, Encoding.UTF8, "application/json");

			return httpClient.PostAsync(urlToPost, bodyContent);
		}
	}
}
