using Microsoft.EntityFrameworkCore;

namespace Entities
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions options): base(options)
        {
            
        }

        public virtual DbSet<Country> Countries { get; set; }

        public virtual DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().ToTable(nameof(Countries));

            modelBuilder.Entity<Person>().ToTable(nameof(Persons));


            //Seed
            string countriesJson = System.IO.File.ReadAllText("countries.json");
            List<Country> countries = 
            System.Text.Json.JsonSerializer.Deserialize <List<Country>>(countriesJson);

            foreach(Country country in countries)
            {
                modelBuilder.Entity<Country>().HasData(country);
            }

            string personsJson = System.IO.File.ReadAllText("persons.json");
            List<Person> persons =
            System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personsJson);

            foreach (Person person in persons)
            {
                modelBuilder.Entity<Person>().HasData(person);
            }


            //Fluent API----------------------------------------------------------------------
            modelBuilder.Entity<Person>().Property(temp => temp.TIN)
                .HasColumnName("TaxIdentificationNumber")
                .HasColumnType("varchar(8)")
                .HasDefaultValue("ABC12345");

            //Index-et hoz letre a tablaban
            //modelBuilder.Entity<Person>().HasIndex(temp=>temp.TIN).IsUnique();

            //insert es update eseten ezt csekkolja. ha false les, exceptiont dob
            //csekkolja, hogy a max length 8 character lehet
            modelBuilder.Entity<Person>().HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber])=8");



            //Table relation ------------------------------------------------------------------
            //erre itt lent nincs szukseg, mert a Person-ban van Country country es CountryID. Ezt nem szoktuk csinalni.

            //A Country-ben lehet ICollection<Person> Persons, ha a countrybol akarom elerni a persont
            //Ez nem kell, de ha ezt megis hasznalni akarjuk, akkor kell a Country-ba a Persons:
            /*
            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasOne<Country>(c => c.Country)
                .WithMany(p => p.Persons)
                .HasForeignKey(p=>p.CountryID);
            });
            */

        }


        //StoredProcedure-hoz kell
        /*
        public List<Person> sp_GetAllPersons()
        {
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
        }
      



        public int sp_InsertPerson(Person person)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PersonID", person.PersonID),
                new SqlParameter("@PersonName", person.PersonName ),
                new SqlParameter("@Email", person.Email ),
                new SqlParameter("@DateOfBirth", person.DateOfBirth ),
                new SqlParameter("@Gender", person.Gender ),
                new SqlParameter("@CountryID", person.CountryID ),
                new SqlParameter("@Address", person.Address ),
                new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters )
            };

            return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters", parameters);
        }
          */





    }
}
