using System.Web.Mvc;

namespace Sdl.Dxa.Integration.Form.Models
{
    public class FormEntity
    {
        
        public string FormId { get; }
        
        public FormEntity(string namespaceId, string objectType, FormCollection formData, string objectKey = null)
        {
            
            // TODO: Expand expressions in the form fields
            
        }

        public void Create()
        {
            
        }

        public void Update()
        {
            /*
             *  
             */
        }
        
        /// <summary>
        /// Get Form ID
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        private string GetFormId(FormCollection formData)
        {
            return formData.Get("___formId");
        }
    }
}