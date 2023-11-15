using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO
{
    public class PersonResponse
    {
        public Guid PersonID { get; set; }

        public string? PersonName { get; set; }

        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public double? Age { get; set; }

        public string? Gender { get; set; }

        public Guid? CountryID { get; set; }

        public string? Country { get; set; }

        public string? Address { get; set; }

        public bool ReceiveNewsLetters { get; set; }



        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            if (obj.GetType() != typeof(PersonResponse)) return false;

            PersonResponse person = (PersonResponse)obj;

            return this.PersonID == person.PersonID
                 && this.PersonName == person.PersonName
                 && this.Email == person.Email
                 && this.DateOfBirth == person.DateOfBirth
                 && this.Gender == person.Gender
                 && this.CountryID == person.CountryID
                 && this.Address == person.Address
                 && this.ReceiveNewsLetters == person.ReceiveNewsLetters;
        }

        //ezt a methodot nem fogjuk sehol hasznalni, csak a zold jelet akartuk eltuntetni
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"Person Id: {PersonID}, " +
                $"Person Name: {PersonName}," +
                $"Email: {Email}," +
                $"Date of Birth: {DateOfBirth?.ToString("dd MM yyy")}," +
                $"Gender: {Gender}," +
                $"Country Id: {CountryID}," +
                $"Country: {Country}," +
                $"Address: {Address}," +
                $"Receive News Letters: {ReceiveNewsLetters}";
        }

        public PersonUpdateRequest ToPersonUpdateRequest()
        {
            return new PersonUpdateRequest()
            {
                PersonID = PersonID,
                PersonName = PersonName,
                CountryID = CountryID,
                Email = Email,
                DateOfBirth = DateOfBirth,
                Gender = (GenderOptions)Enum.Parse(typeof(GenderOptions), Gender, true),
                Address = Address,
                ReceiveNewsLetters = ReceiveNewsLetters
            };
        }
    }


    public static class PersonExtensions
    {
        /// <summary>
        /// An extension method to convert an pbject of Person class into PersonResponse class
        /// </summary>
        /// <param name="person">Returns the converted PersonResponse</param>
        public static PersonResponse ToPersonResponse(this Person person)
        {
            //person => PersonResponse
            return new PersonResponse()
            {
                PersonID = person.PersonID,
                PersonName = person.PersonName,
                Email = person.Email,
                DateOfBirth = person.DateOfBirth,
                Gender = person.Gender,
                CountryID = person.CountryID,
                Address = person.Address,
                ReceiveNewsLetters = person.ReceiveNewsLetters,
                Age = (person.DateOfBirth != null) ?
                    Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365)
                    : null, Country = person.Country?.CountryName
            };
        }


    }
}
