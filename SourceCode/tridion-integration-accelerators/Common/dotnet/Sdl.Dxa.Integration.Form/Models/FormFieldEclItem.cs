using System;
using Sdl.Dxa.Integration.Client.Models;
using Sdl.Web.Common.Models;

namespace Sdl.Dxa.Integration.Form.Models
{
    public class FormFieldEclItem : EclItem
    {
        private string _namespaceId;
        private string _entityType;
        private string _parentId;
        private EclUri _eclUri;
        
        public string FieldName
        {
            get { return this.FileName; }
        }

        public string NamespaceId
        {
            get
            {
                if (_namespaceId == null)
                {
                    _namespaceId = EclUriData.NamespaceId;
                }

                return _namespaceId;
            }
        }

        public string EntityType
        {
            get
            {
                if (_entityType == null)
                {
                    // TODO: Read from config here
                    if (EclUriData.Id.Contains("~"))
                    {
                        _entityType = EclUriData.Id.Split(new char[] {'~'}, StringSplitOptions.RemoveEmptyEntries)[0];
                      
                    }
                } 
                return _entityType;
            }
        }

        private EclUri EclUriData
        {
            get
            {
                if (_eclUri == null)
                {
                    _eclUri = new EclUri(EclUri);
                }

                return _eclUri;
            }
        }
    }
}