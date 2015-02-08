using Excess.Core;
using Excess.Web.Entities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Excess.Web.Controllers
{
    public class XSController : ExcessControllerBase
    {
        public XSController(ITranslationService translator, IDSLService dsl)
        {
            _translator = translator;
            _dsl        = dsl;
        }

        public ActionResult GetSamples()
        {
            var samples = from sample in _db.Samples
                          select new
                          {
                              id   = sample.ID,
                              desc = sample.Name
                          };

            return Json(samples, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSample(int id)
        {
            var content = from   sample in _db.Samples
                          where  sample.ID == id
                          select sample.Contents;

            return Content(content.First());
        }

        [ValidateInput(false)]
        public ActionResult Translate(string text)
        {
            var result = _translator.translate(text); 
            return Content(result);
        }

        public ActionResult GetKeywords()
        {
            IDSLFactory factory = _dsl.factory();

            StringBuilder result = new StringBuilder();
            foreach (string kw in factory.supported())
            {
                result.Append(" ");
                result.Append(kw);
            }

            return Content(result.ToString());
        }

        public ActionResult GetSampleProjects()
        {
            ProjectRepository repo = new ProjectRepository();
            return Json(repo.GetSampleProjects(), JsonRequestBehavior.AllowGet);
        }
        
        private ExcessDbContext     _db = new ExcessDbContext();
        private ITranslationService _translator;
        private IDSLService         _dsl;
    }
}