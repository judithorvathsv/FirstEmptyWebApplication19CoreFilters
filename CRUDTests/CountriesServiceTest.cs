using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using EntityFrameworkCoreMock;
using Moq;

namespace CRUDTests
{
    public class CountriesServiceTest
    {

        private readonly ICountriesService _countryService;

        public CountriesServiceTest()
        {
            var countriesInitialData = new List<Country>() {  };

            //Kell EntityFrameworkCoreMock.Moq es Moq nuget package
            //mocking dbcontext:
            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(
                new DbContextOptionsBuilder<ApplicationDbContext>().Options
                );

            ApplicationDbContext dbContext = dbContextMock.Object;

            //mocking dbset:
            dbContextMock.CreateDbSetMock(c => c.Countries, countriesInitialData);
            _countryService = new CountriesService(dbContext);
        }



        #region AddCountry
        //When CountryAddREquest is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                //Act
               await  _countryService.AddCountry(request);
            });
        }



        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest()
            {
                CountryName = null
            };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async() =>
            {
                //Act
                await _countryService.AddCountry(request);
            });
        }



        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName()
        {
            //Arrange
            CountryAddRequest? request1 = new CountryAddRequest()
            {
                CountryName = "USA"
            };
            CountryAddRequest? request2 = new CountryAddRequest()
            {
                CountryName = "USA"
            };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async() =>
            {
                //Act
                await _countryService.AddCountry(request1);
                await _countryService.AddCountry(request2);
            });
        }



        //When you supply proper country name, it should insert (add) the country to the existing list
        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest()
            {
                CountryName = "Japan"
            };

            //Act
            CountryResponse response = await _countryService.AddCountry(request);
            List<CountryResponse> countries_from_getallcountries = await _countryService.GetAllCountries();

            //Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains(response, countries_from_getallcountries);
            //objA.Equals(objB) -> does not compare actual value!
            //kell a CountryResponse classba egy Equals method hogy mukodjon a Contains!
        }
        #endregion


        #region GetAllCountries
        [Fact]
        //the list of countries should be empty by default
        public async Task GetAllCountries_EmptyList()
        {
            //Acts
            List<CountryResponse> actual_country_response_list = await _countryService.GetAllCountries();

            //Assert
            Assert.Empty(actual_country_response_list);
        }



        [Fact]
        //the list of countries should be empty by default
        public async Task GetAllCountries_AddFewCountries()
        {
            //Arrange
            List<CountryAddRequest> reqList = new List<CountryAddRequest>() {
                    new CountryAddRequest() {CountryName ="USA" },
                    new CountryAddRequest() {CountryName ="UK" }
                };

            //Act
            List<CountryResponse> responseList = new List<CountryResponse>();
            foreach (CountryAddRequest country_req in reqList)
            {
                responseList.Add(await _countryService.AddCountry(country_req));
            }

            List<CountryResponse> actualList = await _countryService.GetAllCountries();
            foreach (CountryResponse expected_country in responseList)
            {
                Assert.Contains(expected_country, actualList);
                //objA.Equals(objB) -> does not compare actual value!
                //kell a CountryResponse classba egy Equals method hogy mukodjon a Contains!
            }
        }
        #endregion



        #region GetCountryByCountryId
        [Fact]
        public async Task GetCountryByCountryId_NullCountryId()
        {
            //Arrange
            Guid? guidId = null;

            //Act
            CountryResponse? response = await _countryService.GetCountryByCountryId(guidId);

            //Assert
            Assert.Null(response);
        }



        [Fact]
        public async Task GetCountryByCountryId_ValidCountryId()
        {
            //Arrange
            //minden test case independent, igy letre kell hozni ez Countryt-t amire kereshetunk
            CountryAddRequest request = new CountryAddRequest()
            {
                CountryName = "China"
            };

            CountryResponse new_country_response = await _countryService.AddCountry(request);

            //Act
            CountryResponse? response = await _countryService.GetCountryByCountryId(new_country_response.CountryID);

            //Assert
            Assert.Equal(new_country_response, response);
            //objA.Equals(objB) -> does not compare actual value!
            //kell a CountryResponse classba egy Equals method hogy mukodjon a Contains!
        }
        #endregion
    }
}
