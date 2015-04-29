using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using Seemplexity.Logic.Basket;

namespace TestMvcApplication.Controllers
{
    public class DefaultController : Controller
    {
        //
        // GET: /Default/

        public ActionResult Index()
        {
            using (var mainDc = new MtMainDbDataContext())
            {
                using (var searchDc = new MtSearchDbDataContext())
                {
                    var priceInfo = mainDc.GetPriceInfo(searchDc, -1929461652);
                }
                
            }
            return View();
        }

    }
}
