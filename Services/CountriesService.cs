using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;



namespace Services
{
    public class CountriesService : ICountriesService
    {

        private readonly ApplicationDbContext _db;

        public CountriesService(ApplicationDbContext personsDbContext)
        {
            _db = personsDbContext;           
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
            if (await _db.Countries.CountAsync(temp => temp.CountryName == countryAddRequest.CountryName) > 0)
                throw new ArgumentException("Given country name already exists");


            //convert CountryAddRequest to Country
            Country country = countryAddRequest.ToCountry();

            //generate new ID
            country.CountryID = Guid.NewGuid();

            //add country into list
            _db.Countries.Add(country);
            await _db.SaveChangesAsync();

            //country id es country name lesz a countryResponse-ban.
            //CountryResponse classban van a method
            return country.ToCountryResponse();
        }


        public async Task<List<CountryResponse>> GetAllCountries()
        {
            return await _db.Countries.Select(country => country.ToCountryResponse()).ToListAsync();
        }


        public async Task<CountryResponse?> GetCountryByCountryId(Guid? countryID)
        {
            if (countryID == null) return null;

            Country? response = await _db.Countries.FirstOrDefaultAsync(temp => temp.CountryID == countryID);

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
                      if(_db.Countries.Where(c=>c.CountryName == countryName).Count() == 0)
                        {
                            Country newCountry = new Country()
                            {
                                CountryName = countryName
                            };

                            _db.Countries.Add(newCountry);
                            await _db.SaveChangesAsync();
                            countriesInserted++;
                        }
                    }
                }
            }
            return countriesInserted;
        }



    }
}
