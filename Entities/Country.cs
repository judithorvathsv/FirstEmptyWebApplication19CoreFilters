using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public class Country
    {
        [Key]
        public Guid CountryID { get; set; }

        public string? CountryName { get; set; }

        //from partent to child class (county -> person)
        //ez nem mindig kell
        //PersonDBcontextben ha van modelBuilder.Entity<Person>(entity => {, akkor ez is kell
        public virtual ICollection<Person>? Persons { get; set;}
    }
}
