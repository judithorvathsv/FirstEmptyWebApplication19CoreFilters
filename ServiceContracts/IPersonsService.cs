using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Microsoft.AspNetCore.Http;

namespace ServiceContracts
{
    public interface IPersonsService
    {
        /// <summary>
        /// Adds a new person into the list of persons
        /// </summary>
        /// <param name="personAddRequest">Person to add</param>
        /// <returns></returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);


        /// <summary>
        /// Returns all persons
        /// </summary>
        /// <returns>Returns a list of objects of PersonResponse type</returns>
        Task<List<PersonResponse>> GetAllPersons();

        /// <summary>
        /// Return the person object based on the given personId
        /// </summary>
        /// <param name="personID">Person Id to search</param>
        /// <returns>Matching person object</returns>
        Task<PersonResponse?> GetPersonByPersonID(Guid? personID);


        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);

        Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder);

        Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);

        Task<bool> DeletePerson(Guid? personID);


        /// <summary>
        /// Return persons as CSV
        /// </summary>
        /// <returns>Return the memory stream with CSV data</returns>
        Task<MemoryStream> GetPersonsCSV();


        Task<MemoryStream> GetPersonsExcel();



    }
}
