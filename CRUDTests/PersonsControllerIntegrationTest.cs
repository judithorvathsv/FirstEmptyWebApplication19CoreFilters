using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using FluentAssertions;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc.Testing;
using Fizzler;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace CRUDTests
{
    public class PersonsControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        /*kell:
1. CrudTests project: CustomWebApplicationFactory class valamint
2. FirstEmptyWebApplication project: Program aljaba: partial class Program { }
3. ebben a projectben jobbeger, edit project file, abba:
    <ItemGroup>
        <InternalsVisibleTo Include = "CRUDTests" />
    </ItemGroup>
4. CrudTests project: Nuget: Microsoft.AspNetCore.Mvc.Testing
5. CrudTests project: CustomWebApplicationFactory classba kell using
6. CrudTests project: Nuget: Microsoft.EntityFrameworkCore.InMemory. u.a. verzio mint Testing-hez a 4.pontban
7. CrudTests project: CustomWebApplicationFactory classba kell using Microsoft.EntityFrameworkCore.InMemory;
 */

        private readonly HttpClient _client;


        public PersonsControllerIntegrationTest(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }


        #region Index
        [Fact]
        public async Task Index_ToReturnView()
        {
            //---- Arrange ----

            //---- Act
            HttpResponseMessage response = await _client.GetAsync("/Persons/Index");

            //---- Assert ----
            //2xx-et ellenorzi, de respopnse body-t nem.
            //response header, body, start line van a responseban 
            response.Should().BeSuccessful();

            //body csekk-hez kell Nugetpackage: Fizzler es Fizzler.Systems.HtmlAgilityPack
            string responseBody = await response.Content.ReadAsStringAsync();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(responseBody); //ez visszaadja az Views/Persons/Index.cshtml-t

            var document = html.DocumentNode;
            document.QuerySelectorAll("table.persons").Should().NotBeNull(); //Index.cshtml -bol ez a table kell:  <table class="table w-100 mt persons">
        }
        #endregion
    }
}
