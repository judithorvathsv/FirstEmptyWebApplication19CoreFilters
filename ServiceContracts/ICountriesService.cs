﻿using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    public interface ICountriesService
    {
        /// <summary>
        /// Adds a country object to the list of countries
        /// </summary>
        /// <param name="countryAddRequest">Country object to add</param>
        /// <returns>Returns the country object after adding it (including newly generated country id)  </returns>
        Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);

        /// <summary>
        /// Returns all countries from the list
        /// </summary>
        /// <returns>Returns all countries from the list as List of CountryResponse</returns>
        Task<List<CountryResponse>> GetAllCountries();


        /// <summary>
        /// Returns a country object based on the given country id
        /// </summary>
        /// <param name="CountryID"></param>
        /// <returns>Matchimg country as CountryResponse object</returns>
        Task<CountryResponse?> GetCountryByCountryId(Guid? CountryID);


        /// <summary>
        /// Uploads countries from excel file into database
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns>Returns number of countries added</returns>
        Task<int> UploadCountriesFromExcelFile(IFormFile formFile);
    }
}
