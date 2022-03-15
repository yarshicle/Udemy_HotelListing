using AutoMapper;
using HotelListing.Core.DTOs;
using HotelListing.Core.IRepository;
using HotelListing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
			var hotels = await _unitOfWork.Hotels.GetAllAsync();
			var results = _mapper.Map<List<HotelDTO>>(hotels);
			return Ok(results);
		}

		[HttpGet("{id:int}", Name = "GetHotel")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetHotel(int id) {
			var hotel = await _unitOfWork.Hotels.GetSingleAsync(q => q.Id == id, include: q => q.Include(x => x.Country!));
			var result = _mapper.Map<HotelDTO>(hotel);
			return Ok(result);
		}

		[Authorize(Roles = "Administrator")]
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDTO hotelDTO) {
			if (!ModelState.IsValid) {
				_logger.LogError($"Invalid POST attempt in {nameof(CreateHotel)}");
				return BadRequest(ModelState);
			}

			var hotel = _mapper.Map<Hotel>(hotelDTO);
			await _unitOfWork.Hotels.InsertAsync(hotel);
			await _unitOfWork.SaveAsync();
			return CreatedAtRoute("GetHotel", new { id = hotel.Id }, hotel);
		}

		[Authorize]
		[HttpPut("{id:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> UpdateHotel(int id, [FromBody] UpdateHotelDTO hotelDTO) {
			if (!ModelState.IsValid || id < 1) {
				_logger.LogError($"Invalid UPDATE attempt in {nameof(UpdateHotel)}");
				return BadRequest(ModelState);
			}

			var hotel = await _unitOfWork.Hotels.GetSingleAsync(q => q.Id == id);
			if (hotel == null) {
				_logger.LogError($"Invalid UPDATE attempt in {nameof(UpdateHotel)}");
				return BadRequest("Submitted data is invalid");
			}

			_mapper.Map(hotelDTO, hotel);
			_unitOfWork.Hotels.Update(hotel);
			await _unitOfWork.SaveAsync();

			return NoContent();
		}

		[Authorize(Roles = "Administrator")]
		[HttpDelete("{id:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteHotel(int id) {
			if (!ModelState.IsValid || id < 1) {
				_logger.LogError($"Invalid DELETE attempt in {nameof(DeleteHotel)}");
				return BadRequest(ModelState);
			}

			var hotel = await _unitOfWork.Hotels.GetSingleAsync(q => q.Id == id);
			if (hotel == null) {
				_logger.LogError($"Invalid DELETE attempt in {nameof(DeleteHotel)}");
				return BadRequest("Submitted data is invalid");
			}
			await _unitOfWork.Hotels.DeleteAsync(id);
			await _unitOfWork.SaveAsync();

			return NoContent();
		}
	}
}
