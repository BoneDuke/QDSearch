namespace QDSearch.DataModel
{
    /// <summary>
    /// Перечисление таблиц базы avalon с соответствиями им внутренних ключей
    /// </summary>
    public enum MegatecTables
    {
        /// <summary>
        /// Таблица неизвестна
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Доп. описание 1
        /// </summary>
        AddDescriptions1 = 1,
        /// <summary>
        /// Доп. описание 2
        /// </summary>
        AddDescriptions2 = 2,
        /// <summary>
        /// Самолеты
        /// </summary>
        Aircrafts = 3,
        /// <summary>
        /// Авиакомпании
        /// </summary>
        Airlines = 4,
        /// <summary>
        /// Аэропорты
        /// </summary>
        Airports = 5,
        /// <summary>
        /// Расписания
        /// </summary>
        AirSeasons = 6,
        /// <summary>
        /// Города
        /// </summary>
        Cities = 7,
        /// <summary>
        /// Чартеры
        /// </summary>
        Charters = 8,
        /// <summary>
        /// Страны
        /// </summary>
        Countries = 9,
        /// <summary>
        /// Экскурсии
        /// </summary>
        Excursions = 10,
        /// <summary>
        /// Истоия
        /// </summary>
        Histories = 11,
        /// <summary>
        /// Отели
        /// </summary>
        Hotels = 12,
        /// <summary>
        /// Таблица HotelRooms
        /// </summary>
        HotelRooms = 13,
        /// <summary>
        /// Размещения в отелях
        /// </summary>
        AccmdMenTypes = 14,
        /// <summary>
        /// Категории номеров
        /// </summary>
        RoomsCategories = 15,
        /// <summary>
        /// Типы комнат
        /// </summary>
        Rooms = 16,
        /// <summary>
        /// Партнеры
        /// </summary>
        Partners = 17,
        /// <summary>
        /// Питания
        /// </summary>
        Pansions = 18,
        /// <summary>
        /// Валюты
        /// </summary>
        Rates = 19,
        /// <summary>
        /// Справочник классов услуг
        /// </summary>
        Services = 20,
        /// <summary>
        /// Code разных классов услуг
        /// </summary>
        ServiceLists = 21,
        /// <summary>
        /// Классы перелетов
        /// </summary>
        AirServices = 22,
        /// <summary>
        /// Справочник трансферов
        /// </summary>
        Transferts = 23,
        /// <summary>
        /// Справочник транспорта
        /// </summary>
        Transports = 24,
        /// <summary>
        /// Справочник курортов
        /// </summary>
        Resorts = 25,
        /// <summary>
        /// ????
        /// </summary>
        DiscountClients = 26,
        /// <summary>
        /// Постоянные клиенты
        /// </summary>
        Clients = 27,
        /// <summary>
        /// ????
        /// </summary>
        Cards = 28,
        /// <summary>
        /// Список источников рекламы
        /// </summary>
        Advertisements = 29,
        /// <summary>
        /// ????
        /// </summary>
        ExchangeRates = 30,
        /// <summary>
        /// ????
        /// </summary>
        Orders = 31,
        /// <summary>
        /// Варианты платежей
        /// </summary>
        PaymentKinds = 32,
        /// <summary>
        /// Типы платежей
        /// </summary>
        PaymentTypes = 33,
        /// <summary>
        /// Описания
        /// </summary>
        Descriptions = 34,
        /// <summary>
        /// Типы описаний
        /// </summary>
        DescTypes = 35,
        /// <summary>
        /// Таблица tp_tours
        /// </summary>
        PriceTours = 36,
        /// <summary>
        /// Таблица tp_lists
        /// </summary>
        TurLists = 37,
        /// <summary>
        /// ???
        /// </summary>
        CostsInsertNumber = 38,
        /// <summary>
        /// ???
        /// </summary>
        PRConsolidation = 39,
        /// <summary>
        /// ???
        /// </summary>
        IlIncPartners = 40,
        /// <summary>
        /// Департаменты в справочнике партнеров
        /// </summary>
        PrtDeps = 41,
        /// <summary>
        /// ???
        /// </summary>
        PrtWarns = 42,
        /// <summary>
        /// Представители партнеров
        /// </summary>
        DupUsers = 43,
        /// <summary>
        /// ????
        /// </summary>
        OrderStatuses = 44,
        /// <summary>
        /// Типы туров
        /// </summary>
        TipTurs = 45,
        /// <summary>
        /// ????
        /// </summary>
        Ships = 46,
        /// <summary>
        /// ???
        /// </summary>
        Cabines = 47,
        /// <summary>
        /// Справочник статусов услуг
        /// </summary>
        Controls = 48,
        /// <summary>
        /// Путевки
        /// </summary>
        Dogovors = 49,
        /// <summary>
        /// Валюты в страховом блоке
        /// </summary>
        InsRates = 50,
        /// <summary>
        /// Регионы в страховом блоке
        /// </summary>
        InsRegions = 51,
        /// <summary>
        /// Варианты услуг в страховом блоке
        /// </summary>
        InsVariants = 52,
        /// <summary>
        /// ???
        /// </summary>
        InsRestrictedRegionCases = 53,
        /// <summary>
        /// Страховые агенты в страховом блоке
        /// </summary>
        InsAgents = 54,
        /// <summary>
        /// Пользователи МТ
        /// </summary>
        Users = 55,
        /// <summary>
        /// ???
        /// </summary>
        KindOfPays = 56,
        /// <summary>
        /// ???
        /// </summary>
        PartnerDepartments = 57,
        /// <summary>
        /// Счета
        /// </summary>
        Accounts = 58,
        /// <summary>
        /// Справочник типов записей в историю
        /// </summary>
        ObjectAliases = 59,
        /// <summary>
        /// Услуги в путевке в МТ
        /// </summary>
        DogovorLists = 60,
        /// <summary>
        /// Справочник туристов
        /// </summary>
        Turists = 61,
        /// <summary>
        /// Расширение информации п услугам для визы
        /// </summary>
        VisaTuristServices = 62,
        /// <summary>
        /// Справочник платежей
        /// </summary>
        Payments = 63,
        /// <summary>
        /// Расширения платежей
        /// </summary>
        PaymentDetails = 64,
        /// <summary>
        /// Связка туристов с усдугами в путевке в МТ
        /// </summary>
        TuristServices = 65,
        /// <summary>
        /// Сообщения внутри МТ
        /// </summary>
        Messages = 66,
        /// <summary>
        /// Категории отелей
        /// </summary>
        CategoriesOfHotels = 67,
        /// <summary>
        /// Привязка документов к услугам визы
        /// </summary>
        VisaServiceToDocs = 68,
        /// <summary>
        /// ???
        /// </summary>
        ServiceDefinitions = 69,
        /// <summary>
        /// Услуги в конструкторе туров
        /// </summary>
        TurService = 70,
        /// <summary>
        /// Связь между квотами и каждым днем услуги в путевке
        /// </summary>
        ServiceByDate = 71,
        /// <summary>
        /// Реальные кросс-курсы
        /// </summary>
        RealCourses = 72,
        /// <summary>
        /// Планируемые кросс-курсы
        /// </summary>
        Courses = 73,
        /// <summary>
        /// Таблица сопоставлений ключей внешних систем с ключами внутренней системы
        /// </summary>
        Mappings = 74,
        /// <summary>
        /// Таблица с настройками
        /// </summary>
        SystemSettings = 75,
        /// <summary>
        /// Правила простановки стоиомости в нац. валюте
        /// </summary>
        NationalCurrencyReservationStatuses = 76,
        /// <summary>
        /// Типы спец. предложений
        /// </summary>
        SPOTypes = 77,
        /// <summary>
        /// Справочник анкет
        /// </summary>
        Questionnaire = 78,
        /// <summary>
        /// Справочник шаблонов полей
        /// </summary>
        QuestionnaireFieldTemplate = 79,
        /// <summary>
        /// Справочник полей анкет
        /// </summary>
        QuestionnaireField = 80, 
        /// <summary>
        /// Справочник вариантов ответов в анкетах
        /// </summary>
        QuestionnaireFieldCase = 81,
        /// <summary>
        /// Справочник ответов на анкеты
        /// </summary>
        QuestionnaireTouristCase = 82,
        /// <summary>
        /// Документы на визу
        /// </summary>
        VisaDocuments = 83,
        /// <summary>
        /// Содержимое документов на визу
        /// </summary>
        VisaDocumentContents = 84,
        /// <summary>
        /// Названия файлов, внесеных в систему
        /// </summary>
        FileHeaders = 85,
        /// <summary>
        /// Содержимое файлов, внесенных в систему
        /// </summary>
        FileRepos = 86,
        /// <summary>
        ///     Поставщик (так же связь с таблицей партнеров (Partners), но только для поставщиков услуг)
        /// </summary>
        PartnerSupplier = 101
    }
}
