using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Xml;
using QDSearch.Caching;
using QDSearch.Extensions;
using QDSearch.Properties;
using QDSearch.Repository.MtMain;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;

namespace QDSearch.Helpers
{
    /// <summary>
    /// содержит ряд методов облегчающих работу с кешем
    /// </summary>
    public static class CacheHelper
    {

        private static XmlDictionaryReaderQuotas xmlQuotas;

        static CacheHelper()
        {
            xmlQuotas = new XmlDictionaryReaderQuotas
            {
                MaxStringContentLength = 0x100000
            };
        }

        /// <summary>
        /// Список префиксов зависимостей кэшей. Создаем эти зависимости при добавлении новых значений в кэш, если они еще не были созданы до этого
        /// </summary>
        private static List<string> tagCachesPrefixes = new List<string>()
        {
            CharterDirectionHash + "_",
            CharterHash + "_",
            CostHash + "_",
            AirSeasonHash + "_",
            QuotaHash + "_",
            DirectionHash + "_",
            RootHash
        };

        /// <summary>
        /// Префикс кэша для квот, цен и расписаний по направлению перелетов
        /// </summary>
        public const string CharterDirectionHash = "ChD";
        /// <summary>
        /// Префикс кэша для квот, цен и расписаний по перелету
        /// </summary>
        public const string CharterHash = "Charter";
        /// <summary>
        /// Префикс кэша для цен
        /// </summary>
        public const string CostHash = "Cost";
        /// <summary>
        /// Префикс кэша для расписаний
        /// </summary>
        public const string AirSeasonHash = "AirSeason";
        /// <summary>
        /// Префикс кэша для квот
        /// </summary>
        public const string QuotaHash = "Quota";
        /// <summary>
        /// Префикс кэша для таблицы изменений объектов в БД Мегатек
        /// </summary>
        public const string ObjectCacheChangesHash = "ObjectCacheChanges";
        /// <summary>
        /// Префик кэша для таблицы выставления и снятия туров в интернет
        /// </summary>
        public const string MwReplQueueHash = "MwReplQueue";
        /// <summary>
        /// Префикс кэша по направлению
        /// </summary>
        public const string DirectionHash = "Direction";
        /// <summary>
        /// Префикс корневого элемента
        /// </summary>
        public const string RootHash = "Root";

        private static readonly Hashtable MemCache = new Hashtable();

        /// <summary>
        /// Список таблиц, по которым будем отслеживать изменения в кэше (основная база)
        /// </summary>
        public static string[] SqlCacheDependencyTablesMainDb =
        {
            DupUsersExtension.TableName,
            AirlinesExtension.TableName,
            HotelCategoriesExtension.TableName,
            DescriptionsExtension.TableName,
            TurServicesExtension.TableName,
            QuestionnaireTouristCasesExtension.TableName//,
            //TouristsExtension.TableName
        };

        /// <summary>
        /// Список таблиц, по которым будем отслеживать изменения в кэше (поисковая база)
        /// </summary>
        public static string[] SqlCacheDependencyTablesSearchDb =
        {
            TurListsExtension.TableName,
            CountriesExtension.TableName,
            CitiesExtension.TableName,
            HotelRoomsExtension.TableName,
            HotelsExtension.TableName,
            RatesExtension.TableName,
            RealCoursesExtension.TableName,
            ResortsExtension.TableName,
            ServicesExtension.TableName,
            PansionsExtension.TableName,
            AccomodationsExtension.TableName,
            RoomsExtension.TableName,
            RoomCategoriesExtension.TableName,
            AirSeasonExtension.TableName,
            CostsExtension.TableName,
            CharterExtension.TableName,
            StopAviaExtension.TableName,
            AirServicesExtension.TableName,

            //таблицы квот
            //QuotasExtension.TableName,
            QuotaObjectsExtension.TableName
            //StopSalesExtension.TableName
        };

        /// <summary>
        /// Глобальный кэш приложения
        /// </summary>
        public static Cache Cache
        {
            get
            {
                return HttpRuntime.Cache;
            }
        }

        public static bool IsCacheKeyExists(string key)
        {
            var result = false;
            object res;
            switch (Globals.CacheMode)
            {
                case StorageModes.AspNet:
                    res = HttpRuntime.Cache[key];
                    if (res != null)
                        result = true;
                    break;
                case StorageModes.Memory:
                    res = MemCache[key];
                    if (res != null)
                        result = true;
                    break;
                case StorageModes.Redis:
                    result = IsCacheKeyExistsRedis(key);
                    break;
            }
            return result;
        }

        /// <summary>
        /// Возвращает элемент кэша
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <returns></returns>
        public static T GetCacheItem<T>(string key)
        {
            var result = default(T);
            object res;
            switch (Globals.CacheMode)
            {
                case StorageModes.AspNet:
                    res = HttpRuntime.Cache[key];
                    if (res != null)
                        result = (T) res;
                    break;
                case StorageModes.Memory:
                    res = MemCache[key];
                    if (res != null)
                        result = (T)res;
                    break;
                case StorageModes.Redis:
                    result = GetItemFromRedis<T>(key);
                    break;
            }
            return result;
        }

        ///// <summary>
        ///// Возвращает элемент кэша
        ///// </summary>
        ///// <param name="key">Ключ кэша</param>
        ///// <returns></returns>
        //public static object GetCacheItem(string key)
        //{
        //    object res = null;
        //    switch (Globals.CacheMode)
        //    {
        //        case CacheModes.AspNet:
        //            res = HttpRuntime.Cache[key];
        //            break;
        //        case CacheModes.Memory:
        //            res = MemCache[key];
        //            break;
        //    }
        //    return res;
        //}

        private static T GetItemFromRedis<T>(string key)
        {
            T result;
            using (var client = new RedisClient())
            {
                var bytesValue = client.Get(key);
                if (bytesValue != null)
                {
                    using (var xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(bytesValue, xmlQuotas))
                    {
                        result = (T) (new DataContractSerializer(typeof (T))).ReadObject(xmlDictionaryReader);
                    }
                }
                else
                    result = default(T);

                //result = bytesValue != null
                //    ? ServiceStack.Text.XmlSerializer.DeserializeFromString<T>(Encoding.UTF8.GetString(bytesValue))
                //    : default(T);
            }
            return result;
        }

        private static bool IsCacheKeyExistsRedis(string key)
        {
            bool result;
            using (var client = new RedisClient())
            {
                result = client.ContainsKey(key);
            }
            return result;
        }


        /// <summary>
        /// Формирует ключ кеша для указанного метода и значений его аргументов.
        /// В этом варианте argValues сворачиваются в хеш.
        /// </summary>
        /// <param name="argValues">значения аргуметов метода. Важно чтобы значения были переданы в метод в том же порядке как они следует в методе</param>
        /// <returns>Строка ключа кеша, где аргументы свернуты в хеш</returns>
        public static string GetCacheKeyHashed(string[] argValues)
        {
            string argValuesString = String.Join("_", argValues);

            byte[] data = Encoding.Unicode.GetBytes(argValuesString);

            using (SHA1 sha = new SHA1CryptoServiceProvider())
            {
                argValuesString = Encoding.Unicode.GetString(sha.ComputeHash(data));
            }
            return argValuesString;
        }

        /// <summary>
        /// Удаляет данные из кэша по вхождению строки в ключ
        /// </summary>
        public static void RemoveCacheDataByKeyPart()
        {
            ReInitCacheDependencies(new []{ RootHash }, false);

            //var itemsToRemove = new List<string>();

            //var enumerator = Cache.GetEnumerator();
            //while (enumerator.MoveNext())
            //{
            //    if ((enumerator.Key.ToString().ToLower().Contains(keyPart.ToLower()) && !itemsToRemove.Contains(enumerator.Key.ToString())) || keyPart == String.Empty)
            //    {
            //        itemsToRemove.Add(enumerator.Key.ToString());
            //    }
            //}



            //foreach (var itemToRemove in itemsToRemove)
            //{
            //    switch (Globals.CacheMode)
            //    {
            //        case StorageModes.AspNet:
            //            Cache.Remove(itemToRemove);
            //            break;
            //        case StorageModes.Memory:
            //            MemCache.Remove(itemToRemove);
            //            break;
            //    }
            //}
        }

        /// <summary>
        /// Добавляет данные в кэш
        /// </summary>
        /// <param name="key">Название ключа кэша</param>
        /// <param name="data">Данные, которые надо положить в кэш</param>
        /// <param name="dependencyTableName">Название таблицы зависимости в SQL</param>
        public static void AddCacheData(string key, object data, string dependencyTableName)
        {
            if (Globals.Settings.Cache.Enabled)
            {
                AddCacheData(key, data, SqlCacheDependencyTablesMainDb.Contains(dependencyTableName)
                            ? new SqlCacheDependency(Globals.Settings.MtMainDbConnectionStringName, dependencyTableName)
                            : new SqlCacheDependency(Globals.Settings.MtSearchDbConnectionStringName, dependencyTableName), 0, (uint)Globals.Settings.Cache.ExpirationMode,
                            CacheItemPriority.Default, null);
            }
            
        }

        /// <summary>
        /// Метод универсальный - как для создания зависимостей, так и для их реинициализации
        /// </summary>
        /// <param name="dependencies">Список зависимостей</param>
        /// <param name="isTagMode">true - реинициализация элемнтов кэша, false - добавление элемента, если его раньше не было (подходит для первичной инициализации)</param>
        public static void ReInitCacheDependencies(IEnumerable<string> dependencies, bool isTagMode)
        {
            if (dependencies == null || !dependencies.Any())
                return;

            var data = DateTime.UtcNow.Ticks;
            foreach (var key in dependencies)
            {
                AddCacheData(key, data, null, 0, (uint)Globals.Settings.Cache.ExpirationMode, CacheItemPriority.Default, null, isTagMode);
            }
        }

        /// <summary>
        /// Добавляет данные в кэш
        /// </summary>
        /// <param name="key">Название ключа кэша</param>
        /// <param name="data">Данные, которые надо положить в кэш</param>
        /// <param name="dependencies">Зависимости</param>
        /// <param name="cacheDuration">Продолжительность жизни кэша</param>
        public static void AddCacheData(string key, object data, List<string> dependencies, uint cacheDuration)
        {
            if (Globals.Settings.Cache.Enabled)
            {
                if (dependencies != null && dependencies.Any())
                {
                    dependencies.Add(RootHash);
                    ReInitCacheDependencies(dependencies.Where(d => tagCachesPrefixes.Any(d.StartsWith)).ToList(), true);
                    AddCacheData(key, data, new CacheDependency(null, dependencies.ToArray()), cacheDuration, (uint)Globals.Settings.Cache.ExpirationMode, CacheItemPriority.Default, null);
                }
                else
                {
                    AddCacheData(key, data, new CacheDependency(null, new string[0]), cacheDuration, (uint)Globals.Settings.Cache.ExpirationMode, CacheItemPriority.Default, null);
                }
            }
        }

        ///// <summary>
        ///// Добавляет данные в кэш
        ///// </summary>
        ///// <param name="key">Название ключа кэша</param>
        ///// <param name="data">Данные, которые надо положить в кэш</param>
        ///// <param name="cacheDuration">Длительность хранения</param>
        ///// <param name="priority">Приоритет кэша</param>
        ///// <param name="onRemoved">Событие при удалении данных из кэша</param>
        //public static void AddCacheData(string key, object data, uint cacheDuration, CacheItemPriority priority, CacheItemRemovedCallback onRemoved)
        //{
        //    AddCacheData(key, data, null, cacheDuration, 0, priority, onRemoved);
        //}

        /// <summary>
        /// Вставка объекта в объект Cache с политиками сроков действия и приоритетов, а также с делегатом,
        /// которого можно использовать для уведомления приложения при удалении вставленного элемента из Cache. 
        /// </summary>
        /// <param name="key">Ключ кэша, используемый для ссылки на объект.</param>
        /// <param name="data">Объект для вставки в кэш.</param>
        /// <param name="dependency">Переменная зависимостей кэша</param>
        /// <param name="cacheDuration">Время по истечению которого объект будет удален из кэша</param>
        /// <param name="slidingExpiration">Интервал в секундах между временем последнего обращения к вставленному объекту и временем истечения срока действия этого объекта.
        /// Если это значение равно 20 минутам, срок действия объекта истечет, и он будет удален из кэша через 20 минут после последнего обращения к этому объекту.
        /// Если используется скользящий срок действия, параметр cacheDuration должен быть равен 0.</param>
        /// <param name="priority">Цена объекта относительно других элементов, сохраненных в кэше, выраженная перечислением CacheItemPriority. 
        /// Это значение используется в кэше при исключении объектов. Объекты с более низкой ценой удаляются из кэша раньше, чем объекты с более высокой ценой.</param>
        /// <param name="onRemoved">Цена объекта относительно других элементов, сохраненных в кэше, выраженная перечислением CacheItemPriority. 
        /// Это значение используется в кэше при исключении объектов. Объекты с более низкой ценой удаляются из кэша раньше, чем объекты с более высокой ценой.</param>
        /// <param name="isTagMode">Добавлять ли данные в кэш как Insert или как Add (http://habrahabr.ru/post/61617/)</param>
        public static void AddCacheData(string key, object data, CacheDependency dependency, uint cacheDuration, uint slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoved, bool isTagMode = false)
        {
            if (Globals.Settings.Cache.Enabled)
            {
                switch (Globals.CacheMode)
                {
                    case StorageModes.AspNet:
                        if (!isTagMode)
                        {
                            //перезаписывваем значение в любом случае
                            Cache.Insert(key, data, dependency,
                             (cacheDuration > 0 && slidingExpiration == 0) ? DateTime.Now.AddSeconds(cacheDuration) : Cache.NoAbsoluteExpiration,
                             slidingExpiration > 0 ? new TimeSpan(0, 0, (int)cacheDuration) : Cache.NoSlidingExpiration, priority, onRemoved);
                        }
                        else
                        {
                            Cache.Add(key, data, dependency,
                             (cacheDuration > 0 && slidingExpiration == 0) ? DateTime.Now.AddSeconds(cacheDuration) : Cache.NoAbsoluteExpiration,
                             slidingExpiration > 0 ? new TimeSpan(0, 0, (int)cacheDuration) : Cache.NoSlidingExpiration, priority, onRemoved);
                        }
                        break;
                    case StorageModes.Redis:
                        if (onRemoved == null)
                            onRemoved = OnRemovedRedisCacheValue;
                        InsertCacheIntoRedisDb(key, data, cacheDuration);
                        if (!isTagMode)
                        {
                            Cache.Insert(key, false, dependency,
                             (cacheDuration > 0 && slidingExpiration == 0) ? DateTime.Now.AddSeconds(cacheDuration) : Cache.NoAbsoluteExpiration,
                             slidingExpiration > 0 ? new TimeSpan(0, 0, (int)slidingExpiration) : Cache.NoSlidingExpiration, priority, onRemoved);
                        }
                        else
                        {
                            Cache.Add(key, false, dependency,
                             (cacheDuration > 0 && slidingExpiration == 0) ? DateTime.Now.AddSeconds(cacheDuration) : Cache.NoAbsoluteExpiration,
                             slidingExpiration > 0 ? new TimeSpan(0, 0, (int)slidingExpiration) : Cache.NoSlidingExpiration, priority, onRemoved);
                        }
                        break;
                    case StorageModes.Memory:
                        if (!MemCache.ContainsKey(key))
                            MemCache.Add(key, data);
                        else
                            MemCache[key] = data;
                        break;
                }
                
            }
        }

        private static void InsertCacheIntoRedisDb(string key, object data, uint cacheDuration)
        {
            using (var client = new RedisClient())
            {
                if (cacheDuration > 0)
                    client.SetEntry(key, ServiceStack.Text.XmlSerializer.SerializeToString(data), TimeSpan.FromSeconds(cacheDuration));
                else
                    client.SetEntry(key, ServiceStack.Text.XmlSerializer.SerializeToString(data));
            }
        }

        private static void OnRemovedRedisCacheValue(string key, object value, CacheItemRemovedReason reason)
        {
            using (var client = new RedisClient())
            {
                client.RemoveEntry(new[] {key});
            }
        }
    }
}
