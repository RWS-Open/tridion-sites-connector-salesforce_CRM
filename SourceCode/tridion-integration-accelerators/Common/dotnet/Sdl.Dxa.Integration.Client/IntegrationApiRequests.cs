namespace Sdl.Dxa.Integration.Client
{
    public class IntegrationApiRequests
    {
  
        // Namespace Query
        //
        public const string NamespaceQuery =
            @"{
                externalNamespaces {
                  namespace
                  entityTypes
                }
              }";
        
        // Get Entity 
        //
        public const string GetEntity =
            @"{
                externalItem(
                    identity: @identity
                ) { 
                parentIdentity {
                  namespace
                  id
                  type
                  localeId
                  structureType
                }
                identity {
                  namespace
                  id
                  type
                  localeId
                  structureType
                }
                @fragment
              }
            }";
        
        // Query Entities
        //
        public const string QueryEntities =
          @"{
              externalItems(
                namespace: ""@namespace"",
                filter: {
                   searchText: ""@searchText"",
                   type: ""@entityType"",
                   context: @context
                } 
              ) {
                edges {
                  node {
                    parentIdentity {
                      namespace
                      id
                      type
                      localeId
                      structureType
                    }
                    identity {
                      namespace
                      id
                      type
                      localeId
                      structureType
                    }
                    @fragment
                  }
                }
              }
          }";
        
        // Create Entity
        //
        public const string CreateEntity =
            @"mutation {
              externalItem: create@type(
                parentIdentity: @parentIdentity
                input: @input
              ) {
                identity {
                  namespace
                  id
                  type
                  localeId
                  structureType
                }
                parentIdentity {
                  namespace
                  id
                  type
                  localeId
                  structureType
                }
                @fragment
              }
            }";
        
        
        // Update Entity
        //
        public const string UpdateEntity =
          @"mutation {
              externalItem: update@type(
                identity: @identity
                input: @input
              ) {
                identity {
                  namespace
                  id
                  type
                  localeId
                  structureType
                }
                parentIdentity {
                  namespace
                  id
                  type
                  localeId
                  structureType
                }
                @fragment
              }
            }";
        
        // Delete Entity
        //
        public const string DeleteEntity =
          @"mutation {
              externalIdentity: delete@type(
                identity: @identity
              ) {                
                id               
              }
            }";

    }
}