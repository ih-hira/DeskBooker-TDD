using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequestProcessor _processor;
        private readonly DeskBookingRequest _request;
        private readonly List<Desk> _availableDesks;
        private readonly Mock<IDeskBookingRepository> _deskBookingRepositoryMock;
        private readonly Mock<IDeskRepository> _deskRepositoryMock;

        public DeskBookingRequestProcessorTests()
        {

            _request = new DeskBookingRequest
            {
                FirstName = "Imtiyaz",
                LastName = "Hossain",
                Email = "ih_hira@outlook.com",
                Date = new DateTime(2020, 1, 28)
            };
            _availableDesks = new List<Desk> { new Desk { Id = 101} };

            _deskBookingRepositoryMock = new Mock<IDeskBookingRepository>();

            _deskRepositoryMock = new Mock<IDeskRepository>();

            _deskRepositoryMock.Setup(x => x.GetAvailableDesks(_request.Date))
                .Returns(_availableDesks);

            _processor = new DeskBookingRequestProcessor(_deskBookingRepositoryMock.Object, _deskRepositoryMock.Object);
        }
        [Fact]
        public void ShouldReturnDeskBookingResultWithRequestValues()
        {
            //Act
            DeskBookingResult result = _processor.BookDesk(_request);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(_request.FirstName, result.FirstName);
        }

        [Fact]
        public void ShouldThrowExceptionIfRequestIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _processor.BookDesk(null));
            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        public void ShouldSaveDeskBooking()
        {
            //Arrange
            DeskBooking savedDeskBooking = null;

            /*It.IsAny<T> is checking that the parameter is of type T, it can be any 
            instance of type T. It's basically saying, I don't care what you pass 
            in here as long as it is type of T.*/

            _deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                .Callback<DeskBooking>(deskBooking =>
                {
                    savedDeskBooking = deskBooking;
                });

            //Act
            _processor.BookDesk(_request);

            //Assert
            _deskBookingRepositoryMock.Verify(x => x.Save(It.Is<DeskBooking>(x => x.FirstName == _request.FirstName)), Times.Once);
            Assert.NotNull(savedDeskBooking);
            //Assert.Equal(_request.FirstName, savedDeskBooking.FirstName);
            //Assert.Equal(_request.LastName, savedDeskBooking.LastName);
            //Assert.Equal(_request.Email, savedDeskBooking.Email);
            //Assert.Equal(_request.Date, savedDeskBooking.Date);
            Assert.Equal(_availableDesks.First().Id, savedDeskBooking.DeskId);
        }

        [Fact]
        public void ShouldNotSaveDeskBookingIfNoDeskIsAvailable()
        {
            _availableDesks.Clear();

            _processor.BookDesk(_request);

            //Assert
            _deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Never);
        }
    }
}
