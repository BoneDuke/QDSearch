using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository;
using QDSearch.Repository.MtSearch;

namespace QDSearch.General.Tests
{
    [TestClass]
    public class QDFilterTests
    {
        [ClassInitialize]
        public static void InitPlainQuotas(TestContext context)
        {
            var dir = Directory.GetCurrentDirectory();
            var directoryInfo = new DirectoryInfo(dir).Parent;
            if (directoryInfo != null)
            {
                var directory = directoryInfo.Parent;
                if (directory != null)
                {
                    var xmlClass = File.ReadAllText(directory.FullName + @"\TestData\﻿GetPlainQuotasObjects_Hotel_19730.txt");
                    CacheHelper.AddCacheData(String.Format("{0}_{1}_{2}_{3}", "GetPlainQuotasObjects", CacheHelper.QuotaHash, (int)ServiceClass.Hotel, 19730), XmlSerializer.Deserialize<List<QuotaPlain>>(xmlClass), 0);
                }
            }
            var smallPlaces = new Dictionary<uint, QuotaSmallServiceParams>
            {
                {
                    1, new QuotaSmallServiceParams()
                    {
                        AndParam = false,
                        PercentParam = 10.0,
                        PlaceParam = 2
                    }
                },
                {
                    3, new QuotaSmallServiceParams()
                    {
                        AndParam = true,
                        PercentParam = null,
                        PlaceParam = 1
                    }
                }
            };
            CacheHelper.AddCacheData("ServiceSmallParams", smallPlaces, 0);
        }

        [TestMethod]
        public void CheckHotelQuotes_AvailiableQuotaState()
        {
            using (var searchDc = new MtSearchDbDataContext())
            {
                string hash;

                const ServiceClass serviceClass = ServiceClass.Hotel;
                const int code = 19730;
                const int subCode1 = 2;
                int? subCode2 = 32;
                const int partnerKey = 6732;
                var serviceDateFrom = new DateTime(2014, 04, 27);
                var serviceDateTo = new DateTime(2014, 04, 29);
                const int requestedPlaces = 2;

                var result = searchDc.CheckServiceQuota(serviceClass, code, subCode1,
                            subCode2, partnerKey, serviceDateFrom,
                            serviceDateTo, requestedPlaces, out hash);

                Assert.AreEqual(result, new QuotaStatePlaces
                {
                    IsCheckInQuota = false,
                    Places = 4,
                    QuotaState = QuotesStates.Availiable
                });
            }
        }

        [TestMethod]
        public void CheckHotelQuotes_SmallQuotaState()
        {
            using (var searchDc = new MtSearchDbDataContext())
            {
                string hash;

                const ServiceClass serviceClass = ServiceClass.Hotel;
                const int code = 19730;
                const int subCode1 = 2;
                int? subCode2 = 32;
                const int partnerKey = 6732;
                var serviceDateFrom = new DateTime(2014, 04, 27);
                var serviceDateTo = new DateTime(2014, 05, 03);
                const int requestedPlaces = 2;

                var result = searchDc.CheckServiceQuota(serviceClass, code, subCode1,
                            subCode2, partnerKey, serviceDateFrom,
                            serviceDateTo, requestedPlaces, out hash);

                Assert.AreEqual(result, new QuotaStatePlaces
                {
                    IsCheckInQuota = false,
                    Places = 1,
                    QuotaState = QuotesStates.Small
                });
            }
        }

        [TestMethod]
        public void CheckHotelQuotes_RequestQuotaState()
        {
            using (var searchDc = new MtSearchDbDataContext())
            {
                string hash;

                const ServiceClass serviceClass = ServiceClass.Hotel;
                const int code = 19730;
                const int subCode1 = 2;
                int? subCode2 = 400;
                const int partnerKey = 6732;
                var serviceDateFrom = new DateTime(2014, 04, 27);
                var serviceDateTo = new DateTime(2014, 04, 29);
                const int requestedPlaces = 2;

                var result = searchDc.CheckServiceQuota(serviceClass, code, subCode1,
                            subCode2, partnerKey, serviceDateFrom,
                            serviceDateTo, requestedPlaces, out hash);

                Assert.AreEqual(result, new QuotaStatePlaces
                {
                    IsCheckInQuota = false,
                    Places = 0,
                    QuotaState = QuotesStates.Request
                });
            }
        }

        [TestMethod]
        public void FlightState_No()
        {
            CacheHelper.AddCacheData("GetCharterCityDirection_Charter_1_3812", new Tuple<int, int>(227, 332), 0);
            CacheHelper.AddCacheData(StopAviaExtension.TableName, new List<StopAvia>(), 0);
            //CacheHelper.AddCacheData("GetPlainQuotasObjects_Quota_1_3812_뱢媘원̸ⷪ뉇靊", new List<QuotaPlain>()
            //GetPlainQuotasObjects_Quota_1_3812
            CacheHelper.AddCacheData("GetPlainQuotasObjects_1_3812_뱢媘원̸ⷪ뉇靊", new List<QuotaPlain>()
            {
                new QuotaPlain
                {
                    Busy = 180,
                    CheckInPlaces = 180,
                    CheckInPlacesBusy = 0,
                    Date = new DateTime(2014, 04, 27),
                    Duration = String.Empty,
                    IsAllotmentAndCommitment = false,
                    PartnerKey = 7859,
                    Places = 180,
                    QdId = 31136542,
                    QoId = 112967,
                    Release = null,
                    SsId = 20152512,
                    SsQdId = 31136542,
                    SubCode1 = 98,
                    SubCode2 = -1,
                    Type = QuotaType.Commitment
                }
            }, 0);

            int? subCode2 = null;
            using (var searchDc = new MtSearchDbDataContext())
            {
                string hash;

                const ServiceClass serviceClass = ServiceClass.Flight;
                const int code = 3812;
                const int subCode1 = 89;
                const int partnerKey = 7859;
                var serviceDateFrom = new DateTime(2014, 04, 27);
                var serviceDateTo = new DateTime(2014, 05, 22);
                const int requestedPlaces = 2;

                var result = searchDc.CheckServiceQuota(serviceClass, code, subCode1, subCode2, partnerKey, serviceDateFrom,
                    serviceDateTo, requestedPlaces, out hash);

                Assert.AreEqual(result.QuotaState, QuotesStates.No);
                Assert.AreEqual(result.Places, (uint)0);

                Assert.AreEqual(result, new QuotaStatePlaces()
                {
                    IsCheckInQuota = false,
                    Places = 0,
                    QuotaState = QuotesStates.No
                });
            }

            
        }
    }
}
