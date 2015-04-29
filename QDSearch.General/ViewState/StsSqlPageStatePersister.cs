using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using QDSearch.Configuration;

namespace QDSearch.ViewState
{
    /// <summary>
    /// Класс для работы с ViewState
    /// </summary>
    public class StsSqlPageStatePersister : HiddenFieldPageStatePersister
    {
        private readonly ViewStateElement _viewStateElement = Globals.Settings.ViewState;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="page">СТраница</param>
        public StsSqlPageStatePersister(Page page) : base(page) { }

        /// <summary>
        /// Метод загружающий ViewState из базы данных и размещающий его на странице. 
        /// </summary>
        public override void Load()
        {

            // Load Whatever is there in the __VIEWSTATE hidden variables.
            base.Load();

            // Если включен постринченый режим применения проверяем помечена страница или нет
            try
            {
                if (_viewStateElement.ApplayByCustomPageOnly && !(Page is IStsStatePersistedPage))
                    return;

                string currentViewState = ViewState.ToString();

                // Проверяем что ViewState это GUID, для этого достаточно проверить,
                // что количество знаков равно 36
                if (currentViewState.Length == 36)
                {
                    StsViewState vs = null;

                    if (_viewStateElement.UseCacheForSqlMode)
                    {
                        //string cacheKey = VIEWSTATE_CACHEKEY_PREFIX + base.ViewState;
                        //try { vs = GetFromCache(cacheKey); }
                        //catch { }
                        throw new NotImplementedException("Механизм кэширования PageState еще не реализован.");
                    }

                    if (vs == null)
                        vs = LoadFromSql(currentViewState);

                    if (_viewStateElement.UseCompression)
                        throw new NotImplementedException("Механизм сжатия PageState еще не реализован.");

                    Deserialize(vs.Data);
                }
            }
            catch (Exception e)
            {
                if (!Globals.Settings.ExceptionHandling.SendExceptionEmails)
                    throw;

                QDSearch.Helpers.ExceptionHandling.SendErrorMessage(
                    new ApplicationException(
                        "Ошибка загрузки состояния страницы, пользователь перенаправлен на начальную страницу.", e));
                //QDSearch.Helpers.Web.ShowMessageWithRedirect(Page, @"К сожалению, страница устарела. Для продолжения работы страница будет перезагружена.", Page.ResolveClientUrl(@"~/"));
                Page.Response.Redirect(Page.ResolveClientUrl(@"~/"), true);
            }
        }

        /// <summary>
        /// Метод сохраняющий ViewState в базу данных и размещающий его ключ на странице. 
        /// </summary>
        public override void Save()
        {
            // Если включен постринченый режим применения проверяем помечена страница или нет
            if (_viewStateElement.ApplayByCustomPageOnly && !(Page is IStsStatePersistedPage))
            {
                base.Save();
                return;
            }

            var vs = new StsViewState
                                  {
                                      Id = Guid.NewGuid(),
                                      Data = Serialize(),
                                      Timeout = _viewStateElement.Timeout
                                  };

            SaveToSql(vs);

            if (_viewStateElement.UseCacheForSqlMode)
                //SaveToCache(vs, VIEWSTATE_CACHEKEY_PREFIX + vs.Id);
                throw new NotImplementedException("Механизм кэширования PageState еще не реализован.");


            // Заменяем реальный ViewState на GUID сохраненного
            ViewState = vs.Id;
            ControlState = "";

            // Базовый класс размещает подмененный ViewState на странице
            base.Save();
        }

        /// <summary>
        /// Загрузка ViewState из БД
        /// </summary>
        /// <param name="vsId">Идентификатор состояния страницы</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        protected StsViewState LoadFromSql(string vsId)
        {
            if (string.IsNullOrEmpty(vsId))
                return null;

            using (var connection = new SqlConnection(_viewStateElement.PageStateDbConnectionString))
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                var vs = new StsViewState();;
                try
                {
                    using (var command = new SqlCommand("", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "dbo.SelectPageStateItem";
                        command.Parameters.AddWithValue("@pageStateId", vsId);

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                vs.Id = (Guid)dr["PageStateId"];
                                vs.Data = dr["PageStateItem"].ToString();
                                vs.Timeout = Convert.ToUInt32(dr["TimeoutMin"]);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new ApplicationException(String.Format(@"Ошибка загрузки состояния страницы для Id={0}, Data={1}, Timeout={2}, __pingStamp={3}",
                        vs.Id, vs.Data, vs.Timeout, Page.Request.Params.AllKeys.Contains("__pingStamp") ? Page.Request["__pingStamp"].ToString(CultureInfo.InvariantCulture) : "не найден", e));
                }
                return vs;
            }
        }

        /// <summary>
        /// Сохраняет ViewState в БД
        /// </summary>
        /// <param name="vs"></param>
        protected void SaveToSql(StsViewState vs)
        {
            if (vs != null && !string.IsNullOrEmpty(vs.Data))
            {
                using (var connection = new SqlConnection(_viewStateElement.PageStateDbConnectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.StoredProcedure;
                        if (vs.Data.Count() > 4000)
                        {
                            command.CommandText = @"dbo.InsertPageStateItemLong";
                            command.Parameters.Add(new SqlParameter("@pageStateItemLong", SqlDbType.NVarChar) { Value = vs.Data });
                        }
                        else
                        {
                            command.CommandText = @"dbo.InsertPageStateItemShort";
                            command.Parameters.Add(new SqlParameter("@pageStateItemShort", SqlDbType.NVarChar) { Value = vs.Data });
                        }

                        command.Parameters.Add(new SqlParameter("@pageStateId", SqlDbType.UniqueIdentifier) { Value = vs.Id });
                        command.Parameters.Add(new SqlParameter("@timeoutMin", SqlDbType.Int) { Value = vs.Timeout });

                        command.Connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Сериализация ViewState
        /// </summary>
        /// <returns></returns>
        protected string Serialize()
        {
            var vsPair = new Pair(ViewState, ControlState);
            return StateFormatter.Serialize(vsPair);
        }

        /// <summary>
        /// Десериализация ViewState
        /// </summary>
        /// <param name="serializedViewState"></param>
        protected void Deserialize(string serializedViewState)
        {
            object vsPair = StateFormatter.Deserialize(serializedViewState);
            if (vsPair != null && (vsPair is Pair))
            {
                var myPair = vsPair as Pair;
                ViewState = myPair.First;
                ControlState = myPair.Second;
            }
            else
                ViewState = vsPair;
        }

        /// <summary>
        /// Выполняет сброс счетчика времени до окончания срока действия для PageState и сессии.
        /// </summary>
        /// <param name="stateGuid">Уникальный идентификтор PageState для которой нужно выполнить сброс.</param>
        internal static bool ResetPageStateTimeout(Guid stateGuid)
        {
            bool result;
            using (var connection = new SqlConnection(Globals.Settings.ViewState.PageStateDbConnectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = @"dbo.ResetPageStateTimeout";
                    command.Parameters.Add(new SqlParameter("@pageStateId", SqlDbType.UniqueIdentifier) { Value = stateGuid });

                    command.Connection.Open();
                    result = (bool)command.ExecuteScalar();
                }
            }
            return result;
        }

        #region Futures...
        //public static void SaveToSession(CustomViewState vs, string cacheKey)
        //{
        //    if (vs != null && !string.IsNullOrEmpty(vs.Id) && !string.IsNullOrEmpty(vs.Data))
        //    {
        //        HttpContext.Current.Session.Add(cacheKey, vs);
        //    }
        //}

        //public static CustomViewState GetFromSession(string cacheKey)
        //{
        //    if (string.IsNullOrEmpty(cacheKey))
        //    {
        //        return null;
        //    }

        //    return (CustomViewState)HttpContext.Current.Session[cacheKey];
        //}


        //public static void SaveToCache(StsViewState vs, string cacheKey)
        //{
        //    if (vs != null && !string.IsNullOrEmpty(vs.Id) && !string.IsNullOrEmpty(vs.Data))
        //    {
        //        //var cachingManager = new CachingManager();
        //        //cachingManager.Add(cacheKey, vs);
        //        HttpContext.Current.Cache[cacheKey] = vs;
        //    }
        //}

        //public static StsViewState GetFromCache(string cacheKey)
        //{
        //    if (string.IsNullOrEmpty(cacheKey))
        //    {
        //        return null;
        //    }

        //    //var cachingManager = new CachingManager();
        //    return HttpContext.Current.Cache[cacheKey] as StsViewState;
        //}
        #endregion
    }
}

