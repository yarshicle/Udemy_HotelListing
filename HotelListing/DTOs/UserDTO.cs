﻿using System.ComponentModel.DataAnnotations;

namespace HotelListing.DTOs {
	public class LoginUserDTO {
		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; } = null!;
		[Required]
		[StringLength(15, ErrorMessage = "Your Password is limited to {2} to {1} characters", MinimumLength = 6)]
		public string Password { get; set; } = null!;
	}
	public class UserDTO : LoginUserDTO {
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		[DataType(DataType.PhoneNumber)]
		public string? PhoneNumber { get; set; }
		public ICollection<string>? Roles { get; set; }

	}
}
