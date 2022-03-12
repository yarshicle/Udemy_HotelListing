using AutoMapper;
using HotelListing.Data;
using HotelListing.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelListing.Configurations {
	public class MapperInitializer : Profile {
		public MapperInitializer() {
			CreateMap<Country, CountryDTO>().ReverseMap();
			CreateMap<Country, CreateCountryDTO>().ReverseMap();
			CreateMap<Hotel, HotelDTO>().ReverseMap();
			CreateMap<Hotel, CreateHotelDTO>().ReverseMap();
		}
	}
}
