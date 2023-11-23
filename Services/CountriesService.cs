using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;
using RepositoryContracts;
using ICountriesRepository = RepositoryContracts.ICountriesRepository;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ICountriesRepository _countriesRepository;       

        public CountriesService(ICountriesRepository countriesRepository)
        {
            _countriesRepository = countriesRepository;           
        }


        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            //Validation: CountryAddRequest egyik parametere se lehet nulla
            if (countryAddRequest == null)
                throw new ArgumentNullException(nameof(countryAddRequest));

            //Validation: countryName cannot be null:
            if (countryAddRequest.CountryName == null)
                throw new ArgumentException(nameof(countryAddRequest.CountryName));

            //Validation: countryName cannot be duplicate
            if (await _countriesRepository.GetCountryByCountryName(countryAddRequest.CountryName) != null)
                throw new ArgumentException("Given country name already exists");


            //convert CountryAddRequest to Country
            Country country = countryAddRequest.ToCountry();

            //generate new ID
            country.CountryID = Guid.NewGuid();

            //add country into list
            await _countriesRepository.AddCountry(country);            

            //country id es country name lesz a countryResponse-ban.
            //CountryResponse classban van a method
            return country.ToCountryResponse();
        }


        public async Task<List<CountryResponse>> GetAllCountries()
        {
            return (await _countriesRepository.GetAllCountries())
                                                .Select(country => country.ToCountryResponse())
                                                .ToList();
        }


        public async Task<CountryResponse?> GetCountryByCountryId(Guid? countryID)
        {
            if (countryID == null) return null;

            Country? response = await _countriesRepository.GetCountryByCountryID(countryID.Value);

            if (response == null) return null;

            return response.ToCountryResponse();
        }




        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            MemoryStream memoryStream = new MemoryStream();

            await formFile.CopyToAsync(memoryStream);
            int countriesInserted = 0;

            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                //Ehhez kell csinalnom egy Excel filet. Abban legyen egy Countries sheet. Abban A1 legyen beirva: CountryName
                //csak az A oszlopba irhatunk country neveket egymas ala
                ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets["Countries"];

                int rowCount = workSheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    //cellabol kivesz adatot
                    string? cellValue = Convert.ToString(workSheet.Cells[row, 1].Value);

                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        string? countryName = cellValue;

                        //megnezni, hogy a database-ben van-e mar ilyen country name.
                        //ha nincs ilyen country meg, akkor mentjuk a database-be.
                      if(await _countriesRepository.GetCountryByCountryName(countryName)  == null)
                        {
                            Country newCountry = new Country()
                            {
                                CountryName = countryName
                            };

                            await _countriesRepository.AddCountry(newCountry);                   
                            countriesInserted++;
                        }
                    }
                }
            }
            return countriesInserted;
        }



    }
}
