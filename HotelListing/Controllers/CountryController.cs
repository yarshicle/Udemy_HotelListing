using AutoMapper;
using HotelListing.Data;
using HotelListing.DTOs;
using HotelListing.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class CountryController : ControllerBase {
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<CountryController> _logger;
		private readonly IMapper _mapper;

		public CountryController(IUnitOfWork unitOfWork, ILogger<CountryController> logger, IMapper mapper) {
			_unitOfWork = unitOfWork;
			_logger = logger;
			_mapper = mapper;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetCountries() {
			try {
				var countries = await _unitOfWork.Countries.GetAllAsync();
				var results = _mapper.Map<List<CountryDTO>>(countries);
				return Ok(results);
			} catch (Exception ex) {
				_logger.LogError(ex, $"Something Went Wrong in the {nameof(GetCountries)}");
				return StatusCode(500, "Internal Server Error. Please Try Again Later");
			}
		}

		[HttpGet("{id:int}", Name = "GetCountry")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetCountry(int id) {
			try {
				var country = await _unitOfWork.Countries.GetSingleAsync(q => q.Id == id, new List<string> { "Hotels" });
				var result = _mapper.Map<CountryDTO>(country);
				return Ok(result);
			} catch (Exception ex) {
				_logger.LogError(ex, $"Something Went Wrong in the {nameof(GetCountry)}");
				return StatusCode(500, "Internal Server Error. Please Try Again Later");
			}
		}

		[Authorize(Roles = "Administrator")]
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateCountry([FromBody] CreateCountryDTO countryDTO) {
			if (!ModelState.IsValid) {
				_logger.LogError($"Invalid POST attempt in {nameof(CreateCountry)}");
				return BadRequest(ModelState);
			}

			try {
				var country = _mapper.Map<Country>(countryDTO);
				await _unitOfWork.Countries.InsertAsync(country);
				await _unitOfWork.SaveAsync();
				return CreatedAtRoute("GetCountry", new { id = country.Id }, country);
			} catch (Exception ex) {
				_logger.LogError(ex, $"Something Went Wrong in the {nameof(CreateCountry)}");
				return StatusCode(500, "Internal Server Error. Please Try Again Later");
			}
		}

		[Authorize]
		[HttpPut("{id:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> UpdateCountry(int id, [FromBody] UpdateCountryDTO countryDTO) {
			if (!ModelState.IsValid || id < 1) {
				_logger.LogError($"Invalid UPDATE attempt in {nameof(UpdateCountry)}");
				return BadRequest(ModelState);
			}

			try {
				var country = await _unitOfWork.Countries.GetSingleAsync(q => q.Id == id);
				if (country == null) {
					_logger.LogError($"Invalid UPDATE attempt in {nameof(UpdateCountry)}");
					return BadRequest("Submitted data is invalid");
				}

				_mapper.Map(countryDTO, country);
				_unitOfWork.Countries.Update(country);
				await _unitOfWork.SaveAsync();

				return NoContent();
			} catch (Exception ex) {
				_logger.LogError(ex, $"Something Went Wrong in the {nameof(UpdateCountry)}");
				return StatusCode(500, "Internal Server Error. Please Try Again Later");
			}
		}

		[Authorize(Roles = "Administrator")]
		[HttpPut("{id:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteCountry(int id) {
			if (!ModelState.IsValid || id < 1) {
				_logger.LogError($"Invalid DELETE attempt in {nameof(DeleteCountry)}");
				return BadRequest(ModelState);
			}

			try {
				var country = await _unitOfWork.Countries.GetSingleAsync(q => q.Id == id);
				if (country == null) {
					_logger.LogError($"Invalid DELETE attempt in {nameof(DeleteCountry)}");
					return BadRequest("Submitted data is invalid");
				}

				await _unitOfWork.Countries.DeleteAsync(id);
				await _unitOfWork.SaveAsync();

				return NoContent();
			} catch (Exception ex) {
				_logger.LogError(ex, $"Something Went Wrong in the {nameof(DeleteCountry)}");
				return StatusCode(500, "Internal Server Error. Please Try Again Later");
			}
		}
	}
}
