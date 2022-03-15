using System.ComponentModel.DataAnnotations;

namespace HotelListing.DTOs {
	public class CreateCountryDTO {
		[Required]
		[StringLength(maximumLength: 100, ErrorMessage = "Country Name Is Too Long")]
		public string Name { get; set; } = null!;
		[Required]
		[StringLength(maximumLength: 2, ErrorMessage = "Short Country Name Is Too Long")]
		public string ShortName { get; set; } = null!;
	}

	public class UpdateCountryDTO : CreateCountryDTO {
		public IList<CreateHotelDTO>? Hotels { get; set; }
	}
	public class CountryDTO : CreateCountryDTO {
		public int Id { get; set; }
		public virtual IList<HotelDTO>? Hotels { get; set; }
	}


}
