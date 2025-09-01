using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sdl.Dxa.Integration.Client.Models;
using Sdl.Dxa.Modules.Crm;
using Sdl.Tridion.Api.GraphQL.Client;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ConnectorFramework.Contracts;
using Tridion.Remoting.Contracts;
using Xunit;
using Xunit.Abstractions;

namespace Sdl.Dxa.Integration.Client.Tests
{
    public class TestIntegrationClient
    {
        private readonly ITestOutputHelper _output;
        private readonly IIntegrationApi _integrationApi;

        public TestIntegrationClient(ITestOutputHelper output)
        {
            _output = output;
            var endpoint = new Uri("http://localhost:8081/cd/api");
            var graphQlClient = new GraphQLClient(endpoint);
            _integrationApi = new IntegrationApi(graphQlClient);
        }

        [Fact]
        public void TestGetEntitySchema()
        {
            var entitySchema = _integrationApi.GetEntitySchema("Contact");
            _output.WriteLine($"Schema Name: {entitySchema.Name}, Description: {entitySchema.Description}");
            _output.WriteLine("Fields:");
            foreach (var field in entitySchema.Fields)
            {
                _output.WriteLine($"  Name: {field.Name} Type: {field.Type} Readonly: {field.Readonly} Is List: {field.List}");                
            }
        }
        
        [Fact]
        public void TestGetCrmEntity()
        {
            var entity = _integrationApi.GetEntity(new EntityIdentity("0034J0000092S5EQAU")
            {
                Type = "Contact",
                NamespaceId = "salesforce"

            });
            OutputEntity(entity);
        }

        [Fact]
        public void TestGetCommerceEntity()
        {
            var entity = _integrationApi.GetEntity(new EntityIdentity("300564392")
            {
                Type = "Product",
                NamespaceId = "hybris"

            });
            OutputEntity(entity);
        }
        
        [Fact]
        public void TestGetCommerceEntityUsingEclUri()
        {
            var entity = _integrationApi.GetEntity("ecl:7-hybris-300564392-Product-file");
            OutputEntity(entity);
        }

        [Fact]
        public void TestGetXillioEntityUsingEclUri()
        {
            var entity = _integrationApi.GetEntity("ecl:2-wordpress-!2F;posts!2F;43-XillioContent-file");
            OutputEntity(entity);
        }

        [Fact]
        public void TestCreateCrmEntity()
        {
            DynamicEntity entity = new DynamicEntity();
            entity.SetField("firstName", "Arne");
            entity.SetField("lastName", "Bengt");
            entity.SetField("email", "arne@bengt.com");
            var createdId = _integrationApi.CreateEntity(RootEntity.CreateRootEntityIdentity("salesforce", null),
                "Contact", entity);
            
            _output.WriteLine("Created Identity:");
            OutputEntityIdentity(createdId);
        }
        
        [Fact]
        public void TestCreateCrmEntityWithResponse()
        {
            DynamicEntity entity = new DynamicEntity();
            entity.SetField("firstName", "Arne");
            entity.SetField("lastName", "Bengt");
            entity.SetField("email", "arne@bengt.com");
            var createdEntity = _integrationApi.CreateEntityWithResponse(RootEntity.CreateRootEntityIdentity("salesforce", null),
                "Contact", entity);
            
            _output.WriteLine("Created Entity:");
            OutputEntity(createdEntity);
        }
        
        [Fact]
        public void TestUpdateCrmEntityWithResponse()
        {
            DynamicEntity entity = new DynamicEntity();
            entity.SetField("phone", "+469299922");
            var updatedEntity = _integrationApi.UpdateEntityWithResponse(new EntityIdentity("0034J0000092S5EQAU")
            {
                Type = "Contact",
                NamespaceId = "salesforce"

            }, entity);
            
            _output.WriteLine("Updated Entity:");
            OutputEntity(updatedEntity);
        }

        [Fact]
        public void TestDeleteCrmEntity()
        {
            _integrationApi.DeleteEntity(new EntityIdentity("0034J0000092dISQAY")
            {
                Type = "Contact",
                NamespaceId = "salesforce"

            });
        }

        [Fact]
        public void TestQueryCrmEntities()
        {
            var result = _integrationApi.QueryEntities(new EntityFilter
            {
                SearchText = "tridion_bf59fb59-6d35-441f-9b29-430de6cbadae", // Search using tracking key
                EntityType = "Contact",
                Context = RootEntity.CreateRootEntityIdentity("salesforce", null)
            });
            
            _output.WriteLine("Matching contacts:");
            foreach (var entity in result.DynamicEntities)
            {
                _output.WriteLine("ID: " + entity.Identity.Id);
            }
        }

        [Fact]
        public void TestEncodingBinaryUrl()
        {
            var encodedUrl =
                "hybris!s!product!s!L3Jlc3QvdjIvbWVkaWFzL3NvbGFyLXBhbmVsLW1vdW50LWtpdC5qcGc__s__Y29udGV4dA%3D%3D_bWFzdGVyfGltYWdlc3w2NzMyMnxpbWFnZS9qcGVnfGltYWdlcy9oMTgvaDM4Lzg3OTcyMzYwNjgzODIuanBnfGU0OGYwNGRkMDViZTg1MjlkYTAwZjkxMzk1NDQxNTIwYmVjYWY2ZTY1ZmE2OGUzY2U1YWNjZDBiODE4ZjcyMWE";

            var downloadUrl = BinaryReferenceHelper.DecodeBinaryUrl(encodedUrl);
            _output.WriteLine("Download URL: " + downloadUrl);
        }

        private void OutputEntity(DynamicEntity entity)
        {
            _output.WriteLine("Entity:");
            _output.WriteLine("------------------------");
            if (entity.Identity != null)
            {
                _output.WriteLine("Identity:");
                OutputEntityIdentity(entity.Identity);
            }
            if (entity.ParentIdentity != null)
            {
                _output.WriteLine("Parent Identity:");
                OutputEntityIdentity(entity.ParentIdentity);
            }
            OutputValueObject(entity, 0);
        }

        private void OutputEntityIdentity(IEntityIdentity identity)
        {
            _output.WriteLine("  Namespace: " + identity.NamespaceId);
            _output.WriteLine("  Id: " + identity.Id);
            _output.WriteLine("  Type: " + identity.Type);
            if (identity.LocaleId != null)
            {
                _output.WriteLine("  LocaleId: " + identity.LocaleId);
            }
            _output.WriteLine("  StructureType: " + identity.StructureType);
        }

        private void OutputValueObject(IDynamicValueObject valueObject, int spaces)
        {
            foreach (var field in valueObject.GetFieldNames())
            {
                var fieldValue = valueObject.GetField(field);
                if (fieldValue is IDynamicValueObject nestedValueObject)
                {
                    _output.WriteLine(field.PadRight(spaces, ' ') + ": ");
                    OutputValueObject(nestedValueObject, spaces+2);
                }
                else if (fieldValue is List<NestedObject> list)
                {
                    _output.WriteLine(field.PadRight(spaces, ' ') + ": ");
                    _output.WriteLine("[".PadRight(spaces, ' '));
                    bool firstItem = true;
                    foreach (var ListItem in list)
                    {
                        if (!firstItem)
                        {
                            _output.WriteLine("");
                        }
                        else
                        {
                            firstItem = false;
                        }
                        OutputValueObject(ListItem, spaces+2);
                        
                    }
                    _output.WriteLine("]".PadRight(spaces, ' '));
                }
                else
                {
                    _output.WriteLine(field.PadRight(spaces, ' ') + ": " + valueObject.GetField(field));
                }
            } 
        }
    }
}