using Entities;

namespace ServiceContracts.DTO
{
    public class CountryResponse
    {
        public Guid CountryID { get; set; }
        public string? CountryName { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            if (obj.GetType() != typeof(CountryResponse)) return false;

            CountryResponse country_to_compare = (CountryResponse)obj;
            return this.CountryID == country_to_compare.CountryID
                && this.CountryName == country_to_compare.CountryName;
        }

        //ezt a methodot nem fogjuk sehol hasznalni, csak a zold jelet akartuk eltuntetni
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public static class CountryExternsions
    {
        //Country-bol CountryResponse, amit a user fog latni, ha leker egy Country-t
        public static CountryResponse ToCountryResponse(this Country country)
        {
            return new CountryResponse()
            {
                CountryID = country.CountryID,
                CountryName = country.CountryName
            };
        }
    }

}
