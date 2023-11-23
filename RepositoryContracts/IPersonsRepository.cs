using Entities;
using System.Linq.Expressions;

namespace RepositoryContracts
{
    public interface IPersonsRepository
    {
        //itt entity kell, mig a service-be dto!!!

        Task<Person> AddPerson(Person person);

        Task<List<Person>> GetAllPersons();

        Task<Person?> GetPersonByPersonID(Guid personID);

        Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate);

        Task<bool> DeletePersonByPersonID(Guid personID);

        Task<Person> UpdatePerson(Person person);
    }
}
