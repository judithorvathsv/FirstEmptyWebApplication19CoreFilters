using AutoFixture;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RepositoryContracts;
using Serilog;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System.Linq.Expressions;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        private readonly IPersonsRepository _personsRepository;
        private readonly Mock<IPersonsRepository> _personsRepositoryMock;

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
            //---------Autofixture + mocking repo:----------------

            //Kell Autofixture Nuget package:
            _fixture = new Fixture();

            //mocking repository
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;
            var diagnosticContextMock = new Mock<IDiagnosticContext>();
            var loggerMock = new Mock<ILogger<PersonsService>>();
            _personsService = new PersonsService(_personsRepository, 
                loggerMock.Object,
                diagnosticContextMock.Object);

            _testOutputHelper = testOutputHelper;




            //-----------Ezek mind torolhetok, csak a regi methodokhoz hagytam meg:-----
            //Kell EntityFrameworkCoreMock.Moq es Moq nuget package
            var countriesInitialData = new List<Country>() { };
            var personsInitialData = new List<Person>() { };

            //mocking dbcontext:
            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(
                new DbContextOptionsBuilder<ApplicationDbContext>().Options
             );
            ApplicationDbContext dbContext = dbContextMock.Object;

            //mocking dbset with mocked repository:
            dbContextMock.CreateDbSetMock(c => c.Countries, countriesInitialData);
            _countriesService = new CountriesService(null);
            dbContextMock.CreateDbSetMock(c => c.Persons, personsInitialData);  
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

        //we do not need reposmocking:
        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;

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

        //reposmocking:
        [Fact]
        public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
        {
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(p => p.PersonName, null as string)
                .Create();

            Person person = personAddRequest.ToPerson();

            //Assert with FluentAssertions package + Mock repo
            _personsRepositoryMock
                .Setup(t=>t.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);
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
                .With(p => p.Email, "someone@example.com")
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


        [Fact]
        public async Task AddPerson_FullProperPersonDetails_ToBeSuccessful()
        {

            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "someone@example.com")
                .Create();

            //mocking all service methods which using repository:
            //Add method returns Person, which is the parameter in the method
            Person person = personAddRequest.ToPerson();
            PersonResponse person_response_expected = person.ToPersonResponse();
            _personsRepositoryMock
                .Setup(t => t.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);
      

            //Act
            PersonResponse response = await _personsService.AddPerson(personAddRequest);
            //ez nem kell a repo mocking miatt:
            //List<PersonResponse> list = await _personsService.GetAllPersons();

            /*
            //Assert
            Assert.True(response.PersonID != Guid.Empty);
            Assert.Contains(response, list);
            */

            //Assert with FluentAssertions package
            response.PersonID.Should().NotBe(Guid.Empty);
            //list.Should().Contain(response);

            person_response_expected.PersonID = response.PersonID;
            //Assert: FluentAssertions + Mocking:
            response.Should().Be(person_response_expected);
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

        //no need repomocking:
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
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

        //with repomocking:
        [Fact]
        public async Task GetPersonByPersonID_WithPersonID_ToBeSuccessfull()
        {
            Person person = _fixture.Build<Person>()
                                                .With(p => p.Email, "someone@example.com")
                                                .With(temp => temp.Country, null as Country)
                                                .Create();
            PersonResponse personResponse_expected =  person.ToPersonResponse();
            _personsRepositoryMock
                .Setup(t => t.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);
            //PersonResponse personResponse = await _personsService.AddPerson(personRequest);

            PersonResponse? response = await _personsService.GetPersonByPersonID(person.PersonID);
            //PersonResponse ? response = await _personsService.GetPersonByPersonID(personResponse.PersonID);

            //Assert with FluentAssertions package           
            //response.Should().Be(personResponse);
            response.Should().Be(personResponse_expected);
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

        //repomocking
        [Fact]
        public async Task GetAllPersons_EmptyList_Mocking()
        {
            _personsRepositoryMock.Setup(t => t.GetAllPersons())
                .ReturnsAsync(new List<Person>());
            List<PersonResponse> getAll = await _personsService.GetAllPersons();

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


        [Fact]
        public async Task GetAllPersons_AddFewPersons_ToBeSuccessfull()
        {
            List<Person> persons = new List<Person>() { 
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example2.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example3.com")
                .With(temp => temp.Country, null as Country)
                .Create()
        };

            //List<PersonAddRequest> personAddRequestsList = new List<PersonAddRequest>()
            //{
            //    personRequest1, personRequest2, personRequest3
            //};

            List<PersonResponse> personResponseList_expected = persons.Select(t => t.ToPersonResponse()).ToList();

            //List<PersonResponse> personResponseList = new List<PersonResponse>();

            //foreach (var req in personAddRequestsList)
            //{
            //    PersonResponse personResponse = await _personsService.AddPerson(req);
            //    personResponseList.Add(personResponse);
            //}

            _personsRepositoryMock.Setup(t => t.GetAllPersons()).ReturnsAsync(persons);
            List<PersonResponse> listFromDB = await _personsService.GetAllPersons();


            //print out output Expected
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse p in personResponseList_expected)
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
            //listFromDB.Should().BeEquivalentTo(personResponseList);

            listFromDB.Should().BeEquivalentTo(personResponseList_expected);

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

        //repomocking:
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
        {
            List<Person> persons = new List<Person>() {
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example2.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example3.com")
                .With(temp => temp.Country, null as Country)
                .Create()
        };

            List<PersonResponse> personResponseList_expected = persons.Select(t => t.ToPersonResponse()).ToList();
            List<PersonResponse> personResponseList = new List<PersonResponse>();

            //print out output Expected
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse p in personResponseList)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }
            //---------------------------------------------------------------
            _personsRepositoryMock
                .Setup(t => t.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);

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
            //listFromDB.Should().BeEquivalentTo(personResponseList);
            listFromDB.Should().BeEquivalentTo(personResponseList_expected);
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


        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
        {
            List<Person> persons = new List<Person>() {
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example2.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example3.com")
                .With(temp => temp.Country, null as Country)
                .Create()
        };

            List<PersonResponse> personResponseList_expected = persons.Select(t => t.ToPersonResponse()).ToList();
            List<PersonResponse> personResponseList = new List<PersonResponse>();

            //print out output Expected
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse p in personResponseList)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }
            //---------------------------------------------------------------
            _personsRepositoryMock
                .Setup(t => t.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);

            List<PersonResponse> listFromDB =
               await _personsService.GetFilteredPersons(nameof(PersonAddRequest.PersonName), "sa");

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
            //listFromDB.Should().BeEquivalentTo(personResponseList);
            listFromDB.Should().BeEquivalentTo(personResponseList_expected);
            //listFromDB.Should()
            //    .OnlyContain(temp => temp.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase));
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


        //repomock:
        [Fact]
        public async Task GetSortedPersons_byPersonName_ToBeSuccessfull()
        {
            List<Person> persons = new List<Person>() {
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example2.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            _fixture.Build<Person>()
                .With(p => p.Email, "person@example3.com")
                .With(temp => temp.Country, null as Country)
                .Create()
        };

            List<PersonResponse> personResponseList_expected = persons.Select(t => t.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(t => t.GetAllPersons()).ReturnsAsync(persons);

            //print out output Expected
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse p in personResponseList_expected)
            {
                _testOutputHelper.WriteLine(p.ToString());
            }

            //---------------------------------------------------------------
            List<PersonResponse> allPersons = await _personsService.GetAllPersons();

            //mivel a service-ben levo GetSortedPersons method nem hiv be repository-t igy ezt nem bantjuk:
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

        //repomock: itt nincs mit, csak a method kapott uj nevet. A folotte levo methodot lehet torolni.
        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = null;
             
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


        //repomock: itt nincs mit, csak a method kapott uj nevet. A folotte levo methodot lehet torolni.
        [Fact]
        public async Task UpdatePerson_InvalidPersonId_ToBeArgumentException()
        {
            PersonUpdateRequest? personUpdateRequest = _fixture.Build<PersonUpdateRequest>()
                .Create();
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


        //repomock: 
        [Fact]
        public async Task UpdatePerson_InvalidPersonName_ToBeArgumentException()
        {
            Person person = _fixture.Build<Person>()
                .With(p => p.PersonName, null as string)
                .With(p => p.Email, "person@example.com")
                .With(P => P.Country, null as Country)
                .With(p => p.Gender, "Male")
                .Create();
            PersonResponse personResponse = person.ToPersonResponse();

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

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

        //repomock: 
        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
        {
            Person person = _fixture.Build<Person>()
                .With(p => p.Email, "person@example.com")
                .With(P => P.Country,null as Country)
                .With(p => p.Gender, "Male")
                .Create();
            PersonResponse personResponse = person.ToPersonResponse();

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            _personsRepositoryMock
                .Setup(t => t.UpdatePerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            _personsRepositoryMock
                .Setup(t => t.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            PersonResponse updatedPersonResponseFromService = await _personsService.UpdatePerson(personUpdateRequest);           

            //Assert with FluentAssertions package          
            //updatedPersonResponseFromService.Should().Be(updatedPersonFromDB);
            updatedPersonResponseFromService.Should().Be(personResponse);
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
        //repomock: 
        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
        {
            Person person = _fixture.Build<Person>()
                .With(p => p.Email, "person@example.com")
                .With(P => P.Country, null as Country)
                .With(p => p.Gender, "Male")
                .Create();

            _personsRepositoryMock
                .Setup(t => t.DeletePersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            _personsRepositoryMock
                .Setup(t => t.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            bool isDeleted = await _personsService.DeletePerson(person.PersonID);

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
