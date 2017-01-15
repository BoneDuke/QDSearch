<%@ Application Language="C#" %>
<%@ Import Namespace="System.Threading.Tasks" %>
<%@ Import Namespace="QDSearch" %>
<%@ Import Namespace="QDSearch.Helpers" %>
<%@ Import Namespace="QDSearch.Repository.MtSearch" %>

<script RunAt="server">

    void Application_Start(object sender, EventArgs e)
    {
        // Code that runs on application startup
        if (Globals.Settings.Cache.Enabled)
        {
            try
            {
                var factory = new TaskFactory();
                factory.StartNew(GetFilterMethods.ClearOldDrectionCache);
                factory.StartNew(GetFilterMethods.ClearOldObjectCache);
                
                //GetFilterMethods.ClearOldDirectionCacheDelegate del1 = GetFilterMethods.ClearOldDrectionCache;
                //del1.BeginInvoke(null, null);

                //GetFilterMethods.ClearOldObjectCacheDelegate del2 = GetFilterMethods.ClearOldObjectCache;
                //del2.BeginInvoke(null, null);

                SqlCacheDependencyAdmin.EnableNotifications(Globals.Settings.MtMainDbConnectionString);
                SqlCacheDependencyAdmin.EnableTableForNotifications(Globals.Settings.MtMainDbConnectionString, CacheHelper.SqlCacheDependencyTablesMainDb);

                SqlCacheDependencyAdmin.EnableNotifications(Globals.Settings.MtSearchDbConnectionString);
                SqlCacheDependencyAdmin.EnableTableForNotifications(Globals.Settings.MtSearchDbConnectionString, CacheHelper.SqlCacheDependencyTablesSearchDb);
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Для базы данных на настроен SqlCacheDependency. Либо отключите cache в web.config, либо настройте публикацию изменений таблиц в SQL", ex);
            }

        }
    }

    void Application_End(object sender, EventArgs e)
    {
        //  Code that runs on application shutdown

    }

    void Application_Error(object sender, EventArgs e)
    {
        if (!Globals.Settings.ExceptionHandling.SendExceptionEmails)
            return;
        // Code that runs when an unhandled error occurs
        try
        {
            try
            {
                var lastError = HttpContext.Current.Server.GetLastError();
                ExceptionHandling.SendErrorMessage(lastError);
            }
            finally
            {
                // Обнуление ошибки на сервере
                Server.ClearError();
                // Перенаправление на статическую html страницу, сообщающую об ошибке
                // никаких данных об произошедшей ошибке ей не передается
                Response.Redirect("Error.html");
            }

        }
        catch
        {
            // если мы всёже приходим сюда - значит обработка исключения 
            // сама сгенерировала исключение, мы ничего не делаем, чтобы
            // не создать бесконечный цикл
            Response.Write("К сожалению произошла критическая ошибка. Нажмите кнопку 'Назад' в браузере и попробуйте ещё раз. ");
        }
    }

    void Session_Start(object sender, EventArgs e)
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e)
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }
       
</script>
