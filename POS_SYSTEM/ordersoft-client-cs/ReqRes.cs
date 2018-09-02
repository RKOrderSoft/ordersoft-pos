using System;
using Newtonsoft.Json;

namespace OrderSoft {

	// ===============
	// Request objects
	// ===============

	public class RequestBody { 	}

	public class TestRequest : RequestBody {
		[JsonProperty("test")]
		public bool Test;
	}

	public class LoginRequest : RequestBody {
		[JsonProperty("username")]
		public string Username;

		[JsonProperty("password")]
		public string Password;
	}

	public class GetOrderRequest : RequestBody {
		[JsonProperty("orderId")]
		public string OrderId;

		[JsonProperty("tableNumber")]
		public int? TableNumber;
	}

	public class SetOrderRequest : RequestBody {
		[JsonProperty("order")]
		public OrderObject Order;
	}

	public class GetDishesRequest : RequestBody {
		[JsonProperty("dishId")]
		public int? DishId;

		[JsonProperty("category")]
		public string Category;

		[JsonProperty("minPrice")]
		public float? MinPrice;

		[JsonProperty("maxPrice")]
		public float? MaxPrice;
	}

	// ================
	// Response objects
	// ================

	public class Response {
		[JsonProperty("ordersoft_version")]
		public string ServerVersion;

		[JsonProperty("reason")]
		public string Reason;
	}

	public class LoginResponse : Response {
		[JsonProperty("sessionId")]
		public string SessionId;

		[JsonProperty("accessLevel")]
		public int AccessLevel;
	}

	public class GetOrderResponse : Response {
		[JsonProperty("order")]
		public OrderObject Order;
	}

	public class SetOrderResponse : Response {
		[JsonProperty("orderId")]
		public string OrderId;
	}

	public class OpenOrdersResponse : Response {
		[JsonProperty("openOrders")]
		public string[] OpenOrders;
	}

	public class UnpaidOrdersResponse : Response {
		[JsonProperty("unpaidOrders")]
		public string[] UnpaidOrders;
	}

	public class GetDishesResponse : Response {
		[JsonProperty("results")]
		public DishObject[] Results;
	}
}