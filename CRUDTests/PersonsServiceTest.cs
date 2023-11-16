using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using Xunit.Abstractions;
using Moq;
using AutoFixture;
using FluentAssertions;
using System.Collections.Generic;
using System;
using static System.Collections.Specialized.BitVector32;


namespace CRUDTests
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        /*
        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _personsService = new PersonsService(false);
            _countriesService = new CountriesService(false);
            _testOutputHelper = testOutputHelper;
        }

        */
     
        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            //Kell Autofixture Nuget package:
            _fixture = new Fixture();

            _testOutputHelper = testOutputHelper;

            //Kell EntityFrameworkCoreMock.Moq es Moq nuget package
            var countriesInitialData = new List<Country>() { };
            var personsInitialData = new List<Person>() { };

            //mocking dbcontext:
            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(
                new DbContextOptionsBuilder<ApplicationDbContext>().Options
                );

            ApplicationDbContext dbContext = dbContextMock.Object;

            //mocking dbset:
            dbContextMock.CreateDbSetMock(c => c.Countries, countriesInitialData);
            _countriesService = new CountriesService(dbContext);
            dbContextMock.CreateDbSetMock(c => c.Persons, personsInitialData);
            _personsService = new PersonsService(dbContext, _countriesService);
        }

        #region AddPerson
        [Fact]
        public async Task AddPerson_NullPerson()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;

            /*
            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async() =>
            {
               await _personsService.AddPerson(personAddRequest);
            });
            */

            //Assert with FluentAssertions package
            Func<Task> action = async () =>
            {
                await _personsService.AddPerson(personAddRequest);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();
        }



        [Fact]
        public async Task AddPerson_PersonNameIsNull()
        {
            //Arrange
            /*
            PersonAddRequest? personAddRequest = new PersonAddRequest()
            {
                PersonName = null
            };
            */
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(p => p.PersonName, null as string)
                .Create();

            /*
            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async() =>
            {
                await _personsService.AddPerson(personAddRequest);
            });
            */

            //Assert with FluentAssertions package
            Func<Task> action = async () =>
            {
                await _personsService.AddPerson(personAddRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }



        [Fact]
        public async Task AddPerson_ProperPersonDetails()
        {
            //Arrange
            /*
            PersonAddRequest personAddRequest = new PersonAddRequest()
            {
                PersonName = "Person name",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Gender = GenderOptions.Male,
                CountryID = Guid.NewGuid(),
                Address = "sample address",
                ReceiveNewsLetters = true
            };
            */

            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(p=>p.Email, "someone@example.com")
                .Create();


            //Act
            PersonResponse response = await _personsService.AddPerson(personAddRequest);

            List<PersonResponse> list = await _personsService.GetAllPersons();

            /*
            //Assert
            Assert.True(response.PersonID != Guid.Empty);
            Assert.Contains(response, list);
            */

            //Assert with FluentAssertions package
            response.PersonID.Should().NotBe(Guid.Empty);
            list.Should().Contain(response);





        }
        #endregion



        #region GetPersonByPersonID
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID()
        {
            Guid personId = Guid.Empty;

            PersonResponse? response = await _personsService.GetPersonByPersonID(personId);
            /*
            //Assert
            Assert.Null(response);
            */

            //Assert with FluentAssertions package           
            response.Should().BeNull();
        }


        [Fact]
        public async Task GetPersonByPersonID_WithPersonID()
        {
            /*
            CountryAddRequest countryRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };
            */
            //mivel nem kell customize semmit, igy nem kell Build.
            CountryAddRequest countryRequest = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse = await _countriesService.AddCountry(countryRequest);

            /*
            PersonAddRequest personRequest = new PersonAddRequest()
            {
                PersonName = "Person name",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse.CountryID,
                Address = "sample address",
                ReceiveNewsLetters = true
            };
            */
            PersonAddRequest personRequest = _fixture.Build<PersonAddRequest>()
                                                .With(p => p.Email, "someone@example.com")
                                                .Create();

            PersonResponse personResponse = await _personsService.AddPerson(personRequest);

            PersonResponse? response = await _personsService.GetPersonByPersonID(personResponse.PersonID);

            /*
            //Assert
            Assert.Equal(personResponse, response);
            */

            //Assert with FluentAssertions package           
            response.Should().Be(personResponse);
      
        }
        #endregion


        #region GetAllPersons
        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            List<PersonResponse> getAll = await _personsService.GetAllPersons();

            /*
            //Assert
            Assert.Empty(getAll);
            */

            //Assert with FluentAssertions package           
            getAll.Should().BeEmpty();
        }


        [Fact]
        public async Task GetAllPersons_AddFewPersons()
        {
            /*
            CountryAddRequest countryRequest1 = new CountryAddRequest()
            {
                CountryName = "USA"
            };

            CountryAddRequest countryRequest2 = new CountryAddRequest()
            {
                CountryName = "India"
            };
            */
            CountryAddRequest countryRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryRequest2);

            /*
            PersonAddRequest personRequest1 = new PersonAddRequest()
            {
                PersonName = "Person name 1",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse1.CountryID,
                Address = "sample address 1",
                ReceiveNewsLetters = true
            };

            PersonAddRequest personRequest2 = new PersonAddRequest()
            {
                PersonName = "Person name 2",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-02-02"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse2.CountryID,
                Address = "sample address 2",
                ReceiveNewsLetters = true
            };

            PersonAddRequest personRequest3 = new PersonAddRequest()
            {
                PersonName = "Person name 3",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-03-03"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse2.CountryID,
                Address = "sample address 3",
                ReceiveNewsLetters = true
            };
            */
            PersonAddRequest personRequest1 = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "person@example.com")
                .Create();

            PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "person@example2.com")
                .Create();

            PersonAddRequest personRequest3 = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "person@example3.com")
                .Create();

            /*
            _personsService.AddPerson(personRequest1);
            _personsService.AddPerson(personRequest2);
            _personsService.AddPerson(personRequest3);
            */

            List <PersonAddRequest> personAddRequestsList = new List<PersonAddRequest>()
            {
                personRequest1, personRequest2, personRequest3
            };

            List<PersonResponse> personResponseList = new List<PersonResponse>();

            foreach (var req in personAddRequestsList)
            {
                PersonResponse personResponse = await _personsService.AddPerson(req);
                personResponseList.Add(personResponse);
            }


            List<PersonResponse> listFromDB = await _personsService.GetAllPersons();


            //print out output Expected
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse p in personResponseList)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }
            //print out output Actual from Db
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse p in listFromDB)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }

            /*
            //ASSERT
            foreach (PersonResponse p in personResponseList)
            {
                Assert.Contains(p, listFromDB);
            }
            */
            //Assert with FluentAssertions package           
            listFromDB.Should().BeEquivalentTo(personResponseList);

        }
        #endregion


        #region GetFilteredPersons
        //if search text is empty and searchBy is PersonName -> return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText()
        {
            CountryAddRequest countryRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "person@example.com")
                .Create();

            PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "person@example2.com")
                .Create();

            PersonAddRequest personRequest3 = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "person@example3.com")
                .Create();

            /*
            CountryAddRequest countryRequest1 = new CountryAddRequest()
            {
                CountryName = "USA"
            };

            CountryAddRequest countryRequest2 = new CountryAddRequest()
            {
                CountryName = "India"
            };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryRequest2);


            PersonAddRequest personRequest1 = new PersonAddRequest()
            {
                PersonName = "Person name Mariam",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse1.CountryID,
                Address = "sample address 1",
                ReceiveNewsLetters = true
            };

            PersonAddRequest personRequest2 = new PersonAddRequest()
            {
                PersonName = "Person name Enigma",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-02-02"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse2.CountryID,
                Address = "sample address 2",
                ReceiveNewsLetters = true
            };

            PersonAddRequest personRequest3 = new PersonAddRequest()
            {
                PersonName = "Person name 3",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-03-03"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse2.CountryID,
                Address = "sample address 3",
                ReceiveNewsLetters = true
            };
            */

            /*
            _personsService.AddPerson(personRequest1);
            _personsService.AddPerson(personRequest2);
            _personsService.AddPerson(personRequest3);
            */

            List<PersonAddRequest> personAddRequestsList = new List<PersonAddRequest>()
            {
                personRequest1, personRequest2, personRequest3
            };

            List<PersonResponse> personResponseList = new List<PersonResponse>();

            foreach (var req in personAddRequestsList)
            {
                PersonResponse personResponse = await _personsService.AddPerson(req);
                personResponseList.Add(personResponse);
            }

            //print out output Expected
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse p in personResponseList)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }

            //---------------------------------------------------------------

            List<PersonResponse> listFromDB =
               await _personsService.GetFilteredPersons(nameof(PersonAddRequest.PersonName), "");

            //print out output Actual from Db
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse p in listFromDB)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }

            //---------------------------------------------------------------

            /*
            //ASSERT
            foreach (PersonResponse p in personResponseList)
            {
                Assert.Contains(p, listFromDB);
            }
            */
            //Assert with FluentAssertions package           
            listFromDB.Should().BeEquivalentTo(personResponseList);
        }


        //if search text is NOT empty and searchBy is PersonName -> return matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName()
        {
            CountryAddRequest countryRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = _fixture.Build<PersonAddRequest>()
                .With(p=>p.PersonName, "Rahman")
                .With(p => p.Email, "person@example.com")
                .With(P=>P.CountryID, countryResponse1.CountryID)
                .Create();

            PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
                .With(p => p.PersonName, "Martha")
                .With(p => p.Email, "person@example2.com")
                .With(P => P.CountryID, countryResponse1.CountryID)
                .Create();

            PersonAddRequest personRequest3 = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "person@example3.com")
                .With(P => P.CountryID, countryResponse2.CountryID)
                .Create();
            /*
            CountryAddRequest countryRequest1 = new CountryAddRequest()
            {
                CountryName = "USA"
            };

            CountryAddRequest countryRequest2 = new CountryAddRequest()
            {
                CountryName = "India"
            };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryRequest2);


            PersonAddRequest personRequest1 = new PersonAddRequest()
            {
                PersonName = "Person name Mariam",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse1.CountryID,
                Address = "sample address 1",
                ReceiveNewsLetters = true
            };

            PersonAddRequest personRequest2 = new PersonAddRequest()
            {
                PersonName = "Person name Enigma",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-02-02"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse2.CountryID,
                Address = "sample address 2",
                ReceiveNewsLetters = true
            };

            PersonAddRequest personRequest3 = new PersonAddRequest()
            {
                PersonName = "Person name 3",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-03-03"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse2.CountryID,
                Address = "sample address 3",
                ReceiveNewsLetters = true
            };
            */

            /*
            _personsService.AddPerson(personRequest1);
            _personsService.AddPerson(personRequest2);
            _personsService.AddPerson(personRequest3);
            */

            List<PersonAddRequest> personAddRequestsList = new List<PersonAddRequest>()
            {
                personRequest1, personRequest2, personRequest3
            };

            List<PersonResponse> personResponseList = new List<PersonResponse>();

            foreach (var req in personAddRequestsList)
            {
                PersonResponse personResponse = await _personsService.AddPerson(req);
                personResponseList.Add(personResponse);
            }

            //print out output Expected
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse p in personResponseList)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }

            //---------------------------------------------------------------

            List<PersonResponse> listFromDB =
                await _personsService.GetFilteredPersons(nameof(PersonAddRequest.PersonName), "ma");

            //print out output Actual from Db
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse p in listFromDB)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }

            //---------------------------------------------------------------

            /*
            //ASSERT
            foreach (PersonResponse p in personResponseList)
            {
                if (p.PersonName != null && p.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.Contains(p, listFromDB);
                }
            }
            */
            //Assert with FluentAssertions package           
            listFromDB.Should()
                .OnlyContain(temp=>temp.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase));
        }

        #endregion



        #region GetSortedPersons
        //sort based on PersonName Desc order
        [Fact]
        public async Task GetSortedPersons_byPersonName()
        {
            CountryAddRequest countryRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = _fixture.Build<PersonAddRequest>()
                .With(p => p.PersonName, "Cloe")
                .With(p => p.Email, "person@example.com")
                .With(P => P.CountryID, countryResponse1.CountryID)
                .Create();

            PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
                .With(p => p.PersonName, "Albert")
                .With(p => p.Email, "person@example2.com")
                .With(P => P.CountryID, countryResponse1.CountryID)
                .Create();

            PersonAddRequest personRequest3 = _fixture.Build<PersonAddRequest>()
                .With(p => p.PersonName, "Bo")
                .With(p => p.Email, "person@example3.com")
                .With(P => P.CountryID, countryResponse2.CountryID)
                .Create();
            /*
            CountryAddRequest countryRequest1 = new CountryAddRequest()
            {
                CountryName = "USA"
            };

            CountryAddRequest countryRequest2 = new CountryAddRequest()
            {
                CountryName = "India"
            };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryRequest2);


            PersonAddRequest personRequest1 = new PersonAddRequest()
            {
                PersonName = "Person name Mariam",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse1.CountryID,
                Address = "sample address 1",
                ReceiveNewsLetters = true
            };

            PersonAddRequest personRequest2 = new PersonAddRequest()
            {
                PersonName = "Person name Enigma",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-02-02"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse2.CountryID,
                Address = "sample address 2",
                ReceiveNewsLetters = true
            };

            PersonAddRequest personRequest3 = new PersonAddRequest()
            {
                PersonName = "Person name Ebba",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-03-03"),
                Gender = GenderOptions.Male,
                CountryID = countryResponse2.CountryID,
                Address = "sample address 3",
                ReceiveNewsLetters = true
            };
            */

            /*
            _personsService.AddPerson(personRequest1);
            _personsService.AddPerson(personRequest2);
            _personsService.AddPerson(personRequest3);
            */

            List<PersonAddRequest> personAddRequestsList = new List<PersonAddRequest>()
            {
                personRequest1, personRequest2, personRequest3
            };

            List<PersonResponse> personResponseList = new List<PersonResponse>();

            foreach (var req in personAddRequestsList)
            {
                PersonResponse personResponse = await _personsService.AddPerson(req);
                personResponseList.Add(personResponse);
            }

            personResponseList = personResponseList.OrderByDescending(p => p.PersonName).ToList();

            //print out output Expected
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse p in personResponseList)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }

            //---------------------------------------------------------------
            List<PersonResponse> allPersons = await _personsService.GetAllPersons();

            List<PersonResponse> listFromDB =
                await _personsService.GetSortedPersons(
                    allPersons,
                    nameof(PersonAddRequest.PersonName),
                    SortOrderOptions.DESC);

            //print out output Actual from Db
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse p in listFromDB)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }

            //---------------------------------------------------------------

            /*
            //ASSERT
            for (int i = 0; i < personResponseList.Count(); i++)
            {
                Assert.Equal(personResponseList[i], listFromDB[i]);
            }
            */
            //Assert with FluentAssertions package          
            listFromDB.Should().BeInDescendingOrder(t => t.PersonName);
        }
        #endregion


        #region UpdatePerson
        //null regest => argument null exception
        [Fact]
        public async Task UpdatePerson_NullPerson()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = null;

            /*
            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async() =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            });
             */
            //Assert with FluentAssertions package          
            Func<Task> action = async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();
        }


        //id invalid => argument null exception
        [Fact]
        public async Task UpdatePerson_InvalidPersonId()
        {
            //Arrange
            /*
            PersonUpdateRequest? personUpdateRequest = new PersonUpdateRequest()
            {
                PersonID = Guid.NewGuid()
            };
            */
            PersonUpdateRequest? personUpdateRequest = _fixture.Build<PersonUpdateRequest>()
                .Create();

            /*
            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async() =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            });
            */
            //Assert with FluentAssertions package          
            Func<Task> action = async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();

        }


        //personName is null => argumentexception
        [Fact]
        public async Task UpdatePerson_InvalidPersonName()
        {
            CountryAddRequest countryRequest = _fixture.Create<CountryAddRequest>();    

            CountryResponse countryResponse = await _countriesService.AddCountry(countryRequest);

            PersonAddRequest personRequest = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "person@example.com")
                .With(P => P.CountryID, countryResponse.CountryID)
                .Create();
            PersonResponse personResponse = await _personsService.AddPerson(personRequest);

            //Arrange
            /*
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "UK"
            };
            CountryResponse country = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest()
            {
                PersonName = "Person name",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Gender = GenderOptions.Male,
                CountryID = country.CountryID,
                Address = "sample address",
                ReceiveNewsLetters = true
            };
          

            PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);
              */

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            personUpdateRequest.PersonName = null;

            /*
            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async() =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            });
            */
            //Assert with FluentAssertions package          
            var action = async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }


        //add new person
        //update that person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetails()
        {
            CountryAddRequest countryRequest = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse = await _countriesService.AddCountry(countryRequest);

            PersonAddRequest personRequest = _fixture.Build<PersonAddRequest>()           
                .With(p => p.Email, "person@example.com")
                .With(P => P.CountryID, countryResponse.CountryID)
                .Create();
            PersonResponse personResponse = await _personsService.AddPerson(personRequest);
            /*
            //Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "UK"
            };
            CountryResponse country = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest()
            {
                PersonName = "Person name",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Gender = GenderOptions.Male,
                CountryID = country.CountryID,
                Address = "sample address",
                ReceiveNewsLetters = true
            };
            PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);
            */

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            personUpdateRequest.PersonName = "John";
            personUpdateRequest.Email = "john@gmail.com";

            PersonResponse updatedPersonResponseFromService = await _personsService.UpdatePerson(personUpdateRequest);

            PersonResponse? updatedPersonFromDB = await _personsService.GetPersonByPersonID(updatedPersonResponseFromService.PersonID);

            /*
            //Assert
            Assert.Equal(updatedPersonFromDB, updatedPersonResponseFromService);
            */
            //Assert with FluentAssertions package          
            updatedPersonResponseFromService.Should().Be(updatedPersonFromDB);

        }
        #endregion



        #region DeletePerson
        [Fact]
        public async Task DeletePerson_ValidPersonID()
        {
            CountryAddRequest countryRequest = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse = await _countriesService.AddCountry(countryRequest);

            PersonAddRequest personRequest = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "person@example.com")
                .With(P => P.CountryID, countryResponse.CountryID)
                .Create();
            PersonResponse personResponse = await _personsService.AddPerson(personRequest);
            /*
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "UK"
            };
            CountryResponse country = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest()
            {
                PersonName = "Person name",
                Email = "person@example.com",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Gender = GenderOptions.Male,
                CountryID = country.CountryID,
                Address = "sample address",
                ReceiveNewsLetters = true
            };
            PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);
            */
            bool isDeleted = await _personsService.DeletePerson(personResponse.PersonID);

            /*
            //Assert
            Assert.True(isDeleted);
            */
            //Assert with FluentAssertions package 
            isDeleted.Should().BeTrue();
        }


        [Fact]
        public async Task DeletePerson_InvalidPersonID()
        {
            bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());

            /*
            //Assert
            Assert.False(isDeleted);
            */
            //Assert with FluentAssertions package 
            isDeleted.Should().BeFalse();
        }
        #endregion

    }
}
