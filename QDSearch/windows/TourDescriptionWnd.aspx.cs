using System;
using System.Linq;
using QDSearch.Repository.MtSearch;

namespace windows
{
    public partial class TourDescriptionWnd : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var qs = new QueryStringParametrs(Request);
            if(qs.IsEmpty || !qs.IsParametrsValid || qs.TourKeys == null || !qs.TourKeys.Any())
            {
                LtContent.Text =
                    @"<span style='color:red;'>ОШИБКА - загрузки данных по туру!</span><br/><span>Идентификатор тура не распознан. Поробуйте еще раз.<br/>Если ошибка повторится сообщите об этом системному администратору.<br/>Спасибо!</span>";
                return;
            }

            using (var dc = new MtSearchDbDataContext())
            {
                LtContent.Text = dc.GetTourDescription(qs.TourKeys.First());
                if (String.IsNullOrWhiteSpace(LtContent.Text))
                    LtContent.Text = @"<span>Для запрошенного тура отсутсвует информация.</span>";
            }
        }
    }
}