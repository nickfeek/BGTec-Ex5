using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using AnprFileService.Data;
using AnprFileService.Models;


namespace AnprFileService.Tests
{
    [TestClass]
    public class WorkerTests
    {
        [TestMethod]
        public void ProcessFile_ValidFile_RecordSavedToDatabase()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            var dataRepositoryMock = new Mock<IDataRepository>();

            // Set up mock behavior for file record not existing
            dataRepositoryMock.Setup(repo => repo.FileRecordExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            // Set up mock behavior for saving file record to the database
            dataRepositoryMock.Setup(repo => repo.SaveFileRecordAsync(It.IsAny<FileRecord>())).Returns(Task.CompletedTask);

            // Set up the service provider
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(provider => provider.GetService(typeof(IDataRepository))).Returns(dataRepositoryMock.Object);
            serviceProviderMock.Setup(provider => provider.GetService(typeof(ILogger<Worker>))).Returns(loggerMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceProviderMock.Setup(provider => provider.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);

            var worker = new Worker(loggerMock.Object, serviceProviderMock.Object);

            // Act
            worker.ProcessFile("C:\\Work\\Projects\\C#\\blands\\Ex5\\AnprFileService.Tests\\Testfile\\2712130774.lpr");

            // Assert
            dataRepositoryMock.Verify(repo => repo.SaveFileRecordAsync(It.IsAny<FileRecord>()), Times.Once);
        }
    }
}
