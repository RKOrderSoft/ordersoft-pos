using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrderSoft {
	public class OrderObject {
        [JsonIgnore]
		public DateTime? TimeSubmitted;
        [JsonIgnore]
		public DateTime? TimeCompleted;
        [JsonIgnore]
		public DateTime? TimePaid;
        [JsonIgnore]
		public string[] Dishes;

		[JsonProperty("orderId")]
		public string OrderId;

		[JsonProperty("dishes")]
        public string DishesString {
            get { return String.Join(",", Dishes); }
            set { Dishes = value.Split(','); }
		}

		[JsonProperty("notes")]
		public string Notes;

		[JsonProperty("timeSubmitted")]
		public string TimeSubmittedString {
            get { if (!TimeSubmitted.HasValue) return null;
                return TimeSubmitted.Value.ToString("yyyy-MM-dd HH:mm:ss", 
				System.Globalization.CultureInfo.InvariantCulture); }
			set { TimeSubmitted = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", 
				System.Globalization.CultureInfo.InvariantCulture); }
		}

		[JsonProperty("timeCompleted")]
		public string TimeCompletedString {
            get { if (!TimeCompleted.HasValue) return null;
                return TimeCompleted.Value.ToString("yyyy-MM-dd HH:mm:ss", 
				System.Globalization.CultureInfo.InvariantCulture); }
			set { TimeCompleted = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", 
				System.Globalization.CultureInfo.InvariantCulture); }
		}

		[JsonProperty("timePaid")]
		public string TimePaidString {
            get { if (!TimePaid.HasValue) return null;
                return TimePaid.Value.ToString("yyyy-MM-dd HH:mm:ss", 
				System.Globalization.CultureInfo.InvariantCulture); }
			set { TimePaid = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", 
				System.Globalization.CultureInfo.InvariantCulture); }
		}

		[JsonProperty("serverId")]
		public string ServerId;

		[JsonProperty("tableNumber")]
		public int TableNumber;

		[JsonProperty("amtPaid")]
		public float? AmtPaid;
	}

	public class DishObject {
        [JsonIgnore]
		public string[] Sizes;

		[JsonProperty("dishId")]
		public int DishId;

		[JsonProperty("name")]
		public string Name;

		[JsonProperty("basePrice")]
		public float BasePrice;

		[JsonProperty("upgradePrice")]
		public float UpgradePrice;

		[JsonProperty("sizes")]
		public string SizesString {
			get { return String.Join(",", Sizes); }
			set { Sizes = value.Split(','); }
		}

		[JsonProperty("category")]
		public string Category;

		[JsonProperty("image")]
		public string Image;

		[JsonProperty("description")]
		public string Description;
	}
}