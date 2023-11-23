using AutoFixture;
using Moq;
using ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FirstEmptyWebApplication19Core1.Controllers;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Microsoft.AspNetCore.Mvc;
using Entities;

namespace CRUDTests
{
    public class PersonsControllerTest
    {
        private readonly ICountriesService _countriesService;
        private readonly IPersonsService _personsService;

        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly Mock<IPersonsService> _personsServiceMock;

        private readonly Fixture _fixture;


        public PersonsControllerTest()
        {
            _fixture = new Fixture();
            _countriesServiceMock = new Mock<ICountriesService>();
            _personsServiceMock = new Mock<IPersonsService>();

            _countriesService = _countriesServiceMock.Object;
            _personsService = _personsServiceMock.Object;
        }

        #region Index
        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonList()
        {
            //------------------ Arrange (mi az ami kell a mockba vagy mashova) ------------------ 
            //csinalj egy listat
            //mock controller
            //controller Index methodjat atnezni, ha van benne repository, akkor azt a repo methodot mockolni
            //(methodot mockolni: beallitani, hogy mi az input es mi az output)
            List<PersonResponse> personResponseList = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            //Index methodban van 2 db repository method, azokat kell mockolni:
            _personsServiceMock
                .Setup(t => t.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personResponseList);

            _personsServiceMock
                .Setup(t => t.GetSortedPersons(
                    It.IsAny<List<PersonResponse>>(), 
                    It.IsAny<string>(), 
                    It.IsAny<SortOrderOptions>()))
                .ReturnsAsync(personResponseList);

            //------------------ Act -------------------> behivjuk a controllert
            IActionResult result = await personsController.Index(
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<SortOrderOptions>());

            //------------------ Assert ------------------
            //viewt kapunk-e:
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            //a megfelelo model tipus van-e a view-ban:
            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();
            //a megfelelo model van-e a view-ban:
            viewResult.ViewData.Model.Should().Be(personResponseList);
        }
        #endregion



        #region Create
        [Fact]
        public async Task Create_IfModelErrors_ToReturnCreateView() {

            //------------------ Arrange (mi az ami kell a mockba vagy mashova) ------------------
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            // GetAllCountries es AddPersons methodok vannak.
            // Minden methodot mockolni kell ami a controller Create-ben van.
            _countriesServiceMock
                .Setup(t=>t.GetAllCountries())
                .ReturnsAsync(countries);

            _personsServiceMock
                .Setup(t=>t.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);


            //------------------ Act ------------------
            //create error:
            personsController.ModelState.AddModelError("PersonName", "PersonName cannot be blank");
            //call controller:
            IActionResult result = await personsController.Create(personAddRequest);


            //------------------ Assert ------------------
            //viewt kapunk-e:
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            //a megfelelo model tipus van-e a view-ban:
            viewResult.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();
            //a megfelelo model van-e a view-ban:
            viewResult.ViewData.Model.Should().Be(personAddRequest);
        }



        [Fact]
        public async Task Create_IfNoModelErrors_ToReturnRedirectToIndex()
        {
            //------------------ Arrange (mi az ami kell a mockba vagy mashova) ------------------
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            // GetAllCountries es AddPersons methodok vannak.
            // Minden methodot mockolni kell ami a controller Create-ben van.
            _countriesServiceMock
                .Setup(t => t.GetAllCountries())
                .ReturnsAsync(countries);

            _personsServiceMock
                .Setup(t => t.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);

            //------------------ Act ------------------
            //create error:
            //personsController.ModelState.AddModelError("PersonName", "PersonName cannot be blank");
            //call controller:
            IActionResult result = await personsController.Create(personAddRequest);


            //------------------ Assert ------------------
            //redirectet kapunk-e:
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            //hova megy a redirect:
            redirectResult.ActionName.Should().Be("Index");
        }
        #endregion

    }
}
