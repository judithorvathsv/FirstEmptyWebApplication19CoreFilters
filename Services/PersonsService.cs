using CsvHelper;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using System.Globalization;
using CsvHelper;
using System.IO;
using CsvHelper.Configuration;
using OfficeOpenXml;
using RepositoryContracts;

namespace Services
{
    public class PersonsService : IPersonsService
    {

        //private readonly ICountriesRepository _db;
        private readonly IPersonsRepository _personsRepository;      

        public PersonsService(IPersonsRepository personsRepository)
        {
            //Mockaroo.com
            _personsRepository = personsRepository;                    
        }


        /*
         * Erre nincs szukseg mar, mert a PersonResponse class-ba raktam Country = persons.Country?.CountryName-t.
        private PersonResponse ConvertPersonToPersonResponse(Person person)
        {
            PersonResponse personResponse = person.ToPersonResponse();

            personResponse.Country = person.Country?.CountryName;
            //Ez nem kell, ha van Include abban a methodban, ahova be van hivva ez a service:
            //personResponse.Country = _countryService.GetCountryByCountryId(personResponse.CountryID)?.CountryName;

            return personResponse;
        }
        */


        //check personAddRequest is not null
        //Validate all properties
        //convert personAddRequest to Person
        //generate new PersonID
        //add it into List<Person>
        //return PersonResponse with PersonID
        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            if (personAddRequest == null)
            {
                throw new ArgumentNullException(nameof(personAddRequest));
            }

            /*
            if (string.IsNullOrEmpty(personAddRequest.PersonName))
            {
                throw new ArgumentException("PersonName can't be blank");
            }
            */

            /*
            ValidationContext validationContext = new ValidationContext(personAddRequest);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            //true because all validation like required should be checked:
            bool isValid = Validator.TryValidateObject(
                    personAddRequest, validationContext, validationResults, true);

            if (!isValid)
            {
                throw new ArgumentException(validationResults.FirstOrDefault()?.ErrorMessage);
            }
            */
            ValidationHelper.ModelValidation(personAddRequest);

            Person person = personAddRequest.ToPerson();

            person.PersonID = Guid.NewGuid();

            await _personsRepository.AddPerson(person);            

            //with Stored Procedure 
            //_db.sp_InsertPerson(person);

            //PersonResponse personResponse = person.ToPersonResponse();
            //personResponse.Country = _countryService.GetCountryByCountryId(personResponse.CountryID)?.CountryName;
            
            //Navigation property a PersonResponse-ban, ezert ez mar nem kell:
            //return ConvertPersonToPersonResponse(person);
            return person.ToPersonResponse();
        }



        public async Task<List<PersonResponse>> GetAllPersons()
        {
            //ez azert kell, mert van benne include, es a convertPersonToPersonResponse-t modositottuk, 
            //a countryname-et igy kapjuk meg: person.Country.CountryName
            var persons = await _personsRepository.GetAllPersons();
           
            //var countryName = person.Country.CountryName;

            //SELECT * from Persons

            //Navigation property a PersonResponse-ban, ezert ez mar nem kell:
            /*
            return _db.Persons.ToList()
                .Select(temp => ConvertPersonToPersonResponse(temp)).ToList();
            */


            //Ezt stored procedure eseten hasznalnam:
            /*
            return _db.sp_GetAllPersons()
                .Select(temp => ConvertPersonToPersonResponse(temp)).ToList();
            */


            //Navigation property a PersonResponse-ban, ezert ez  kell:        
            return persons
                    .Select(temp => temp.ToPersonResponse()).ToList();
        }



        //personid is not null
        //get person from List<Person> by personid
        //convert person to personresponse
        public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            if (personID == null) return null;

            Person? person = await _personsRepository.GetPersonByPersonID(personID.Value);               
            //var countryName = person.Country.CountryName;


            if (person == null) return null;

            //Navigation property a PersonResponse-ban, ezert ez mar nem kell:
            //return ConvertPersonToPersonResponse(person);
            return person.ToPersonResponse();
        }




        //check searchBy is not null
        //get matching data from List<Person>
        //convert from Person to PersonResponse
        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<Person> persons = searchBy switch   
            {
                nameof(PersonResponse.PersonName) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                            temp.PersonName.Contains(searchString)),

                nameof(PersonResponse.Email) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                            temp.Email.Contains(searchString)),

                nameof(PersonResponse.DateOfBirth) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                            temp.DateOfBirth.Value.ToString("yyyy MMMM dd").Contains(searchString)),

                nameof(PersonResponse.Gender) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                            temp.Gender.Contains(searchString)),

                nameof(PersonResponse.CountryID) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                            temp.Country.CountryName.Contains(searchString)),

                nameof(PersonResponse.Address) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                            temp.Address.Contains(searchString)),

               _=> await _personsRepository.GetAllPersons()
            };
            return persons.Select(p=>p.ToPersonResponse()).ToList();
        }




        public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return allPersons;
            }

            List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
            {
                (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) =>
                allPersons.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.ASC) =>
                allPersons.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC) =>
                allPersons.OrderBy(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.ASC) =>
                allPersons.OrderBy(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.ASC) =>
                allPersons.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.ASC) =>
                allPersons.OrderBy(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.ASC) =>
                allPersons.OrderBy(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASC) =>
                allPersons.OrderBy(temp => temp.ReceiveNewsLetters).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(temp => temp.ReceiveNewsLetters).ToList(),

                _ => allPersons
            };

            return sortedPersons;
        }




        //check reqest is not null
        //validate properties
        //get Person by PersonID from List<Person>
        //check Person is not null
        //PersonUpdate request to Person
        //convert Person to PersonResponse    
        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest == null) throw new ArgumentNullException(nameof(personUpdateRequest));

            ValidationHelper.ModelValidation(personUpdateRequest);

            //Person? person = _persons.Where(i=> i.PersonID == personUpdateRequest.PersonID).FirstOrDefault();
            Person? matchingPerson = await _personsRepository.GetPersonByPersonID(personUpdateRequest.PersonID);        

            if (matchingPerson == null) throw new ArgumentException("Given person id does not exist");

            matchingPerson.PersonID = personUpdateRequest.PersonID;
            matchingPerson.PersonName = personUpdateRequest.PersonName;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.CountryID = personUpdateRequest.CountryID;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            await _personsRepository.UpdatePerson(matchingPerson);           

            //Navigation property a PersonResponse-ban, ezert ez mar nem kell:
            //return ConvertPersonToPersonResponse(matchingPerson);
            return matchingPerson.ToPersonResponse();
                
        }



        public async Task<bool> DeletePerson(Guid? personID)
        {
            if (personID == null) throw new ArgumentNullException(nameof(personID));

            Person? person = await _personsRepository.GetPersonByPersonID(personID.Value);

            if (person == null) return false;

            await _personsRepository.DeletePersonByPersonID(personID.Value);
         
            return true;
        }



        //CsvHelper Kell mint NugetPackage
        public async Task<MemoryStream> GetPersonsCSV()
        {
            //ez akkor jo, ha az osszes propertyt ki akarom menteni a Person-hoz. De akkor nem jo, ha pl a country-t,Addresst,
            //vagy barmit NEM akarok kimenteni
            /*
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            CsvWriter csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture, leaveOpen: true);
            csvWriter.WriteHeader<PersonResponse>();  //PersonID,PersonName, ....
            csvWriter.NextRecord();

            List<PersonResponse> persons = _db.Persons.Include("Country").Select(p => p.ToPersonResponse()).ToList();
            await csvWriter.WriteRecordsAsync(persons); //1, John Doe, ....

            memoryStream.Position = 0;
            return memoryStream;
            */

            //--------------------------------------------------------------------------------------

            //Ezt akkor hasznaljuk, ha pl az Address-t nem akarjuk a kimentett file-ba
            //vagy pl mas sorrendet akarunk
            //vagy mas DateOfBirth formatumot stb.
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);

            //lehetne pl en-us, vagy barmi az Invariant helyett
            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);

            CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);
            //Amit header-ben akarok latni, tehat az elso sorban a file-ban:
            csvWriter.WriteField(nameof(PersonResponse.PersonName));            //PersonName, mint header
            csvWriter.WriteField(nameof(PersonResponse.Email));                 //Email, mint header
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));           //DateOfBirth..
            csvWriter.WriteField(nameof(PersonResponse.Age));                   //Age..
            csvWriter.WriteField(nameof(PersonResponse.Gender));                //Gender..
            csvWriter.WriteField(nameof(PersonResponse.Country));               //Country..
            csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));     //ReceiveNewsLetters..

            csvWriter.NextRecord();

            List<PersonResponse> persons = await GetAllPersons();           

            foreach (PersonResponse person in persons)
            {
                csvWriter.WriteField(person.PersonName);
                csvWriter.WriteField(person.Email);
                if (person.DateOfBirth.HasValue)
                {
                    csvWriter.WriteField(person.DateOfBirth.Value.ToString("yyyy-MM-dd"));
                }
                else
                {
                    csvWriter.WriteField("");
                }
                csvWriter.WriteField(person.Age);
                csvWriter.WriteField(person.Gender);
                csvWriter.WriteField(person.Country);
                csvWriter.WriteField(person.ReceiveNewsLetters);

                csvWriter.NextRecord();
                csvWriter.Flush();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }



        //kell EPPLus nuget package, es modositas az appsettings is a FirstEmptyWebApplicationCore1-ben.
        public async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");

                //row 1
                worksheet.Cells["A1"].Value = "Person Name";
                worksheet.Cells["B1"].Value = "Email";
                worksheet.Cells["C1"].Value = "Date of Birth";
                worksheet.Cells["D1"].Value = "Age";
                worksheet.Cells["E1"].Value = "Gender";
                worksheet.Cells["F1"].Value = "Contry";
                worksheet.Cells["G1"].Value = "Address";
                worksheet.Cells["H1"].Value = "Receive News Letters";

                using (ExcelRange headerCells = worksheet.Cells["A1:H1"])
                {
                    //formazasi lehetosegek: https://epplussoftware.com/docs
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerCells.Style.Font.Bold = true;
                }

                int row = 2;
                List<PersonResponse> persons = await GetAllPersons();

                foreach(PersonResponse person in persons)
                {
                    //row 2, column 1
                    worksheet.Cells[row, 1].Value = person.PersonName;

                    //row2, column 2
                    worksheet.Cells[row, 2].Value = person.Email;
                    if (person.DateOfBirth.HasValue)
                    {
                        worksheet.Cells[row, 3].Value = person.DateOfBirth.Value.ToString("yyy-MM-dd");
                    }
                   
                    worksheet.Cells[row, 4].Value = person.Age;
                    worksheet.Cells[row, 5].Value = person.Gender;
                    worksheet.Cells[row, 6].Value = person.Country;
                    worksheet.Cells[row, 7].Value = person.Address;
                    worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;

                    row++;
                }

                worksheet.Cells[$"A1:H{row}"].AutoFitColumns();

                await excelPackage.SaveAsync();
            }

            memoryStream.Position = 0;
            return memoryStream;                
        }



    }
}
