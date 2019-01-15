﻿namespace QueryTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.OpenApi.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Query;
    using VDS.RDF.Parsing.Validation;
    using VDS.RDF.Query;

    [TestClass]
    [TestCategory("Sparql")]
    public class SparqlTests
    {
        private static IEnumerable<object[]> Endpoints
        {
            get
            {
                return Resources.OpenApiDocument.Paths.Select(endpoint => new object[] { endpoint.Key });
            }
        }

        [TestMethod]
        [DynamicData("Endpoints")]
        public void ValidateSparql(string endpointName)
        {
            var key = endpointName
                .Remove(endpointName.Length - 5, 5)
                .Remove(0, 1);
            var endpoint = Resources.OpenApiDocument.Paths[endpointName];
            var endpointType = Resources.GetXType<EndpointType>(endpoint);

            if (endpointType == EndpointType.HardCoded)
            {
                return;
            }

            var queryString = Resources.GetSparql(key);
            var query = new SparqlParameterizedString(queryString);

            query.SetUri("schemaUri", new Uri("http://example.com"));
            var parameters = Resources.GetSparqlParameters(endpoint).ToArray();
            if (parameters.Any())
            {
                var values = parameters.ToDictionary(
                    parameter => parameter.Name,
                    parameter =>
                    {
                        switch (Resources.GetXType<ParameterType>(parameter))
                        {
                            case ParameterType.Uri:
                                return "http://example.com";
                            default:
                                return "EXAMPLE";
                        }
                    });

                QueryController.SetParameters(query, parameters, values);
            }

            var validator = new SparqlQueryValidator();
            var result = validator.Validate(query.ToString());

            Assert.IsTrue(result.IsValid, result.Message);
        }
    }
}