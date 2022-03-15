using AutoMapper;
using HotelListing.Core.DTOs;
using HotelListing.Core.IRepository;
using HotelListing.Core.Models;
using HotelListing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
		public async Task<IActionResult> GetCountries([FromQuery] RequestParams requestParams) {
			var countries = await _unitOfWork.Countries.GetAllAsync(requestParams);
			var results = _mapper.Map<List<CountryDTO>>(countries);
			return Ok(results);
		}

		[HttpGet("{id:int}", Name = "GetCountry")]
		[ResponseCache(CacheProfileName = "120SecondsDuration")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetCountry(int id) {
			var country = await _unitOfWork.Countries.GetSingleAsync(q => q.Id == id, include: q => q.Include(x => x.Hotels!));
			var result = _mapper.Map<CountryDTO>(country);
			return Ok(result);
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


			var country = _mapper.Map<Country>(countryDTO);
			await _unitOfWork.Countries.InsertAsync(country);
			await _unitOfWork.SaveAsync();
			return CreatedAtRoute("GetCountry", new { id = country.Id }, country);

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

			var country = await _unitOfWork.Countries.GetSingleAsync(q => q.Id == id);
			if (country == null) {
				_logger.LogError($"Invalid UPDATE attempt in {nameof(UpdateCountry)}");
				return BadRequest("Submitted data is invalid");
			}

			_mapper.Map(countryDTO, country);
			_unitOfWork.Countries.Update(country);
			await _unitOfWork.SaveAsync();

			return NoContent();

		}

		[Authorize(Roles = "Administrator")]
		[HttpDelete("{id:int}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteCountry(int id) {
			if (!ModelState.IsValid || id < 1) {
				_logger.LogError($"Invalid DELETE attempt in {nameof(DeleteCountry)}");
				return BadRequest(ModelState);
			}

			var country = await _unitOfWork.Countries.GetSingleAsync(q => q.Id == id);
			if (country == null) {
				_logger.LogError($"Invalid DELETE attempt in {nameof(DeleteCountry)}");
				return BadRequest("Submitted data is invalid");
			}

			await _unitOfWork.Countries.DeleteAsync(id);
			await _unitOfWork.SaveAsync();

			return NoContent();

		}
	}
}
