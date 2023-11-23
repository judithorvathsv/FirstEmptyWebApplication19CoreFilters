using Entities;

namespace RepositoryContracts
{

    /// <summary>
    /// Represents data access logic for managing Person entity 
    /// </summary>
    public interface ICountriesRepository
    {
        //itt entity kell, mig a service-be dto!!!

        Task<Country> AddCountry(Country country);

        Task<List<Country>> GetAllCountries();

        Task<Country?> GetCountryByCountryID(Guid countryID);

        Task<Country?> GetCountryByCountryName(string countryName);
    }
}