using System;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using Contoso.LegacyBank.Portal.Domain;
using Contoso.LegacyBank.Portal.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Contoso.LegacyBank.Portal.Tests
{
    [TestClass]
    public sealed class GatewayContractTests
    {
        [TestMethod]
        public void WcfDtosUseAccountsContractNamespace()
        {
            var attribute = (DataContractAttribute)Attribute.GetCustomAttribute(
                typeof(CustomerDto), typeof(DataContractAttribute));

            Assert.IsNotNull(attribute);
            Assert.AreEqual("urn:contoso:legacy-bank:accounts:v1", attribute.Namespace);
        }

        [TestMethod]
        public void StatementResponseMapsWebApiCamelCasePayload()
        {
            var id = Guid.NewGuid();
            var json = "{\"jobId\":\"" + id + "\",\"status\":\"completed\"," +
                       "\"pdfPath\":\"C:\\\\Statements\\\\statement.pdf\"," +
                       "\"updatedUtc\":\"2026-09-15T21:00:00Z\"}";

            var response = new JavaScriptSerializer().Deserialize<StatementResult>(json);

            Assert.AreEqual(id, response.JobId);
            Assert.AreEqual("completed", response.Status);
            Assert.AreEqual(@"C:\Statements\statement.pdf", response.PdfUrl);
            Assert.AreEqual(id.ToString(), response.StatementId);
        }
    }
}
