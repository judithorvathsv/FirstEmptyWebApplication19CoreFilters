using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    public class Person
    {
        [Key]
        public Guid PersonID { get; set; }

        [StringLength(40)] //nvarchar(40)
        public string? PersonName { get; set; }

        [StringLength(40)]
        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [ForeignKey("CountryID")]
        public Guid? CountryID { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public bool ReceiveNewsLetters { get; set; }

        //TaxIdentificationNumber
        public string? TIN { get; set; }

        //from child to parent class (person -> country)
        //kell Include(parentClass) a methodba, ha a person-hoz akarok Countryt kapni!
        public virtual Country? Country { get; set; }
    }
}
