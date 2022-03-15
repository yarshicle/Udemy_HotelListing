﻿using AutoMapper;
using HotelListing.Core.DTOs;
using HotelListing.Data;

namespace HotelListing.Core.Configurations {
	public class MapperInitializer : Profile {
		public MapperInitializer() {
			CreateMap<Country, CountryDTO>().ReverseMap();
			CreateMap<Country, CreateCountryDTO>().ReverseMap();
			CreateMap<Hotel, HotelDTO>().ReverseMap();
			CreateMap<Hotel, CreateHotelDTO>().ReverseMap();
			CreateMap<ApiUser, UserDTO>().ReverseMap();
		}
	}
}
