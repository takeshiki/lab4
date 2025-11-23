using System.Collections.Generic;
using Energy_Project.Models;
using Energy_Project.Services;
using Energy_Project.Services.Interfaces;
using Moq;
using Xunit;

namespace SmartHomeTests
{
    public class EnergyMonitorServiceTests
    {
        private readonly Mock<IDeviceRepository> _deviceRepo = new();
        private readonly Mock<IEnergyPlanRepository> _planRepo = new();
        private readonly Mock<INotificationService> _notify = new();
        private readonly EnergyMonitorService _service;

        public EnergyMonitorServiceTests()
        {
            _service = new EnergyMonitorService(_deviceRepo.Object, _planRepo.Object, _notify.Object);
        }

        /// <summary>
        /// Calculates current usage for active devices using provided wattages.
        /// </summary>
        [Theory]
        [InlineData(1000, 500, 1.5)]
        [InlineData(0, 0, 0)]
        [InlineData(2000, 1000, 3.0)]
        public void CalculateCurrentUsageKwh_ReturnsExpectedSum(double first, double second, double expected)
        {
            _deviceRepo.Setup(r => r.GetAll()).Returns(new List<Device>
            {
                new() { PowerUsageWatts = first, IsOn = true },
                new() { PowerUsageWatts = second, IsOn = true }
            });

            var result = _service.CalculateCurrentUsageKwh();

            Assert.Equal(expected, result, 3);
            _deviceRepo.Verify(r => r.GetAll(), Times.Once());
        }

        /// <summary>
        /// Sends an overload alert when usage exceeds the daily limit.
        /// </summary>
        [Fact]
        public void CheckForOverload_SendsAlert_WhenUsageExceedsLimit()
        {
            _deviceRepo.Setup(r => r.GetAll()).Returns(new List<Device>
            {
                new() { PowerUsageWatts = 1500, IsOn = true },
                new() { PowerUsageWatts = 1000, IsOn = true }
            });
            _planRepo.Setup(r => r.GetCurrentPlan()).Returns(new EnergyPlan { DailyLimitKwh = 1.0 });

            _service.CheckForOverload();

            _notify.Verify(n => n.SendAlert(It.Is<string>(msg => msg.Contains("Overload") && msg.Contains("2.5"))), Times.Once());
            _planRepo.Verify(r => r.GetCurrentPlan(), Times.AtLeastOnce());
        }

        /// <summary>
        /// Does not send alerts when usage is within the plan limit.
        /// </summary>
        [Fact]
        public void CheckForOverload_DoesNotSendAlert_WhenWithinLimit()
        {
            _deviceRepo.Setup(r => r.GetAll()).Returns(new List<Device>
            {
                new() { PowerUsageWatts = 500, IsOn = true },
                new() { PowerUsageWatts = 500, IsOn = true }
            });
            _planRepo.Setup(r => r.GetCurrentPlan()).Returns(new EnergyPlan { DailyLimitKwh = 2.0 });

            _service.CheckForOverload();

            _notify.Verify(n => n.SendAlert(It.IsAny<string>()), Times.Never());
        }

        /// <summary>
        /// Updates the energy plan limit and persists the change.
        /// </summary>
        [Fact]
        public void UpdateEnergyLimit_UpdatesPlanAndPersists()
        {
            var plan = new EnergyPlan { DailyLimitKwh = 2.0 };
            _planRepo.Setup(r => r.GetCurrentPlan()).Returns(plan);

            _service.UpdateEnergyLimit(5.5);

            Assert.NotEqual(2.0, plan.DailyLimitKwh);
            Assert.Equal(5.5, plan.DailyLimitKwh);
            _planRepo.Verify(r => r.UpdatePlan(It.Is<EnergyPlan>(p => p.DailyLimitKwh == 5.5)), Times.Once());
        }
    }
}
