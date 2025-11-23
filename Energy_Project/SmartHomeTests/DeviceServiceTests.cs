using System;
using System.Collections.Generic;
using System.Linq;
using Energy_Project.Models;
using Energy_Project.Services;
using Energy_Project.Services.Interfaces;
using Moq;
using Xunit;

namespace SmartHomeTests
{
    public class DeviceServiceTests
    {
        private readonly Mock<IDeviceRepository> _deviceRepo = new();
        private readonly DeviceService _service;

        public DeviceServiceTests()
        {
            _service = new DeviceService(_deviceRepo.Object);
        }

        /// <summary>
        /// Turns the device on, returns true, and persists the change.
        /// </summary>
        [Fact]
        public void ToggleDevice_TurnsOn_WhenDeviceExists()
        {
            var device = new Device { Id = 1, IsOn = false };
            _deviceRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns(device);

            var result = _service.ToggleDevice(1, true);

            Assert.True(result);
            Assert.True(device.IsOn);
            Assert.NotNull(device);
            _deviceRepo.Verify(r => r.Update(It.Is<Device>(d => d.Id == 1 && d.IsOn)), Times.Exactly(1));
            _deviceRepo.Verify(r => r.GetById(1), Times.AtLeastOnce());
        }

        /// <summary>
        /// Throws an exception when the requested device does not exist.
        /// </summary>
        [Fact]
        public void ToggleDevice_Throws_WhenDeviceMissing()
        {
            _deviceRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns((Device?)null);

            Assert.Throws<ArgumentException>(() => _service.ToggleDevice(42, true));
            Assert.Null(_deviceRepo.Object.GetById(42));
            _deviceRepo.Verify(r => r.Update(It.IsAny<Device>()), Times.Never());
        }

        /// <summary>
        /// Turns an active device off and changes the state value.
        /// </summary>
        [Fact]
        public void ToggleDevice_TurnsOff_WhenDeviceWasOn()
        {
            var device = new Device { Id = 2, IsOn = true };
            _deviceRepo.Setup(r => r.GetById(device.Id)).Returns(device);

            var result = _service.ToggleDevice(device.Id, false);

            Assert.False(result);
            Assert.NotEqual(true, result);
            _deviceRepo.Verify(r => r.Update(It.Is<Device>(d => d.Id == device.Id && d.IsOn == false)), Times.Once());
        }

        /// <summary>
        /// Returns only devices that are active.
        /// </summary>
        [Fact]
        public void GetActiveDevices_ReturnsOnlyActiveItems()
        {
            var devices = new List<Device>
            {
                new() { Id = 1, Name = "Lamp", IsOn = true },
                new() { Id = 2, Name = "Heater", IsOn = false },
                new() { Id = 3, Name = "Fan", IsOn = true }
            };
            _deviceRepo.Setup(r => r.GetAll()).Returns(devices);

            var result = _service.GetActiveDevices().ToList();

            Assert.NotEmpty(result);
            Assert.Contains(result, d => d.Id == 1 && d.IsOn);
            Assert.Contains(result, d => d.Id == 3 && d.IsOn);
            _deviceRepo.Verify(r => r.GetAll(), Times.Once());
        }

        /// <summary>
        /// Returns an empty collection when no devices are active.
        /// </summary>
        [Fact]
        public void GetActiveDevices_ReturnsEmpty_WhenNoneOn()
        {
            _deviceRepo.Setup(r => r.GetAll()).Returns(new List<Device>
            {
                new() { Id = 4, IsOn = false }
            });

            var result = _service.GetActiveDevices();

            Assert.Empty(result);
            _deviceRepo.Verify(r => r.GetAll(), Times.Once());
        }
    }
}
