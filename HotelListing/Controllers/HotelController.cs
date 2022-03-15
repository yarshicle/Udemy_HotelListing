using AutoMapper;
using HotelListing.DTOs;
using HotelListing.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class HotelController : ControllerBase {
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<CountryController> _logger;
		private readonly IMapper _mapper;

		public HotelController(IUnitOfWork unitOfWork, ILogger<CountryController> logger, IMapper mapper) {
			_unitOfWork = unitOfWork;
			_logger = logger;
			_mapper = mapper;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetHotels() {
			try {
				var hotels = await _unitOfWork.Hotels.GetAllAsync();
				var results = _mapper.Map<List<HotelDTO>>(hotels);
				return Ok(results);
			} catch (Exception ex) {
				_logger.LogError(ex, $"Something Went Wrong in the {nameof(GetHotels)}");
				return StatusCode(500, "Internal Server Error. Please Try Again Later");
			}
		}

		[Authorize]
		[HttpGet("{id:int}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetHotel(int id) {
			try {
				var hotel = await _unitOfWork.Hotels.GetSingleAsync(q => q.Id == id, new List<string> { "Country" });
				var result = _mapper.Map<HotelDTO>(hotel);
				return Ok(result);
			} catch (Exception ex) {
				_logger.LogError(ex, $"Something Went Wrong in the {nameof(GetHotel)}");
				return StatusCode(500, "Internal Server Error. Please Try Again Later");
			}
		}
	}
}
