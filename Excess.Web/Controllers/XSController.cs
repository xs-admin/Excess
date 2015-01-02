using Excess.Core;
using Excess.Web.Entities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Excess.Web.Controllers
{
    public class XSController : ExcessControllerBase
    {
        public XSController(ITranslationService translator)
        {
            _translator = translator;
        }

        public ActionResult GetSamples()
        {
            var samples = db.Samples;
            return Json(samples, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Translate(string text)
        {
            var result = _translator.translate(text); 
            return Content(result);
        }

        private ExcessDbContext     db = new ExcessDbContext();
        private ITranslationService _translator;
    }
}