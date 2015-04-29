// скрипт выполняющийся при каждой закгрузки страницы
// инициализирует переменные и события а так же любые другие действия которые необходимо выполнить на страницы после ее загрузки.
function startupToursFilter() {
    // добавляет класс для выбранных типов туров, для решения задачи одинакового кросс браузерного отображения 
    // выбранных типов туров
    $(".TFS_TourTypesWrapper li").has("input:checked").addClass("OptionChecked");
    $(".TFS_TourTypesWrapper li").has("input:not(:checked)").removeClass("OptionChecked");

    // добавляет классы необходимые для кроссбраузерного оформления списка ночей
    $(".TFS_NightsWrapper .OptionsList label:first-of-type").addClass("FirstOption");
    $(".TFS_NightsWrapper .OptionsList label:last-of-type").addClass("LastOption");
    $(".TFS_NightsWrapper .OptionsList input:checked").next("label").addClass("OptionChecked");
    $(".TFS_NightsWrapper .OptionsList input:not(:checked)").next("label").removeClass("OptionChecked");

    // выделение туров содержащие указанную фразу
    $(".TSF_TourListWrapper option:contains('. ХИТ! ')").css("color", "red");

    // фильтрация ввода. Не даем вводить ничего кроме цифр
    $("#TbMaxPrice, .TFS_PersonsWrapper").keypress(function (e) {
        e = e || event;
        if (e.ctrlKey || e.altKey || e.metaKey) return true;
        var chr = getChar(e);
        if (chr == null) return true;
        if (chr < '0' || chr > '9')
            return false;
        return true;
    });

    // проверяем корректность количества введеных взрослых
    //$("#TbAdultsFilter").blur(function () { return isAdultsNumberValid(); });
    // при потере фокуса на детях проверям что количество детей указано
    var tbChilds = $("#TbChildsFilter");
    //tbChilds.blur(function () { return isChildsNumberValid(); });

    // проверяем корректность заполнения информации о детях, возраст и количество,
    // настраиваем поля для ввода информации о детях
    tbChilds.keyup(function () {
        var tbChlds = $("#TbChildsFilter");
        var extChlDiv = $("#ExtraChildrenDiv");
        var secondChlSpan = $("#SecondChlSpan");
        var tbFirstChildAge = $("#TbFirstChildAge");
        var tbSecondChildAge = $("#TbSecondChildAge");

        // если детей нет скрываем поля с возрастом и обнуляем ранее заполненные значения возраста
        if (!tbChlds.val() || tbChlds.val() < 1) {
            secondChlSpan.fadeOut(); //style.visibility = "hidden";
            extChlDiv.slideUp();//style.visibility = "hidden";
            tbFirstChildAge.val(null);
            tbSecondChildAge.val(null);
        }

        // если дети есть открываем поля для заполнения возраста первого ребенка
        if (tbChlds.val() && tbChlds.val() > 0) {
            extChlDiv.slideDown(); //style.visibility = "visible";
        }

        // если детей два и более открываем форму для заполнения второго ребенка
        if (tbChlds.val() && tbChlds.val() > 1) {
            secondChlSpan.fadeIn(); //style.visibility = "visible";
        } else {
            secondChlSpan.fadeOut(); //style.visibility = "hidden";
            tbSecondChildAge.val(null);
        }
    });

    // обрабатываем событие смены текущей валюты
    var rblRates = $("#RblRates");
    if (rblRates) {
        // устанавливаем отображение выбранной валюты для поля максимальной цены
        if (!$(rblRates).data("InitMaxPriceRateLabel")) { // чтобы одно и тоже событие не обрабатывать дважды
            rblRates.change(InitMaxPriceRateLabel);
            $(rblRates).data("InitMaxPriceRateLabel", true);
        }
        // конфертируем цены туров в выбранную валюту
        if (!$(rblRates).data("ConvertToursPrice")) { // чтобы одно и тоже событие не обрабатывать дважды
            rblRates.change(ConvertToursPrice);
            $(rblRates).data("ConvertToursPrice", true);
        }
    }
    // инициализируем текущую валюту при первой загрузе
    InitMaxPriceRateLabel();
    ConvertToursPrice();

    // перед началом подбора туров проверяем корректность введенной информации и 
    // формируем строку запроса для текущих параметров поиска
    var btnSearch = $("#BtnSearch");
    if (btnSearch) {
        btnSearch.click(function(e) {
             return ValidateFilters();
        });
        btnSearch.click(function(e) { return AddQueryStringParametrs(); });
    }
}

function isAdultsNumberValid() {
    if ($(".TFS_PersonsWrapper:visible").length < 1)
        return true;
    var tbAdults = $("#TbAdultsFilter");
    if (!tbAdults.val() || tbAdults.val() == 0) {
        alert("Количество взрослых в номере должно быть больше 0");
        tbAdults.focus();
        return false;
    }
    return true;
}
function isChildsNumberValid() {
    if ($(".TFS_PersonsWrapper:visible").length < 1)
        return true;
    var tbChlds = $("#TbChildsFilter");
    if (!tbChlds.val()) {
        alert("Укажите пожалуйста количество детей. 0 - если нет.");
        tbChlds.focus();
        return false;
    }
    return true;
}

// Нам нужно проверять символы при вводе, поэтому, будем использовать событие keypress.
// Алгоритм такой: получаем символ и проверяем, является ли он цифрой. Если не является, то отменяем действие по умолчанию.
// Кроме того, игнорируем специальные символы и нажатия со включенным Ctrl/Alt.
function getChar(event) {
    if (event.which == null) {
        if (event.keyCode < 32) return null;
        return String.fromCharCode(event.keyCode); // IE
    }

    if (event.which != 0 && event.charCode != 0) {
        if (event.which < 32) return null;
        return String.fromCharCode(event.which); // остальные
    }

    return null; // специальная клавиша
}

// устанавливает отображение текущей валюты для максимальной цены за тур
function InitMaxPriceRateLabel() {
    var strSelector = "#RblRates *[for='" + $("#RblRates *:checked").attr("id") + "']";
    $("#RateTitle").text($(strSelector).text());
}

// переключает отображение цены за тур в выбранной валюте
function ConvertToursPrice() {
    var strSelector = "#RblRates *[for='" + $("#RblRates *:checked").attr("id") + "']";
    var selectedRate = $(strSelector).text();
    strSelector = "#tblFindedTours tbody .CellPrice span.PriceWrapper[priceRate!='" + selectedRate + "']";
    $(strSelector).hide();
    strSelector = "#tblFindedTours tbody .CellPrice span.PriceWrapper[priceRate='" + selectedRate + "']";
    $(strSelector).show();
}

// добовляет необходимые параметры в строку запроса на стороне клиента
function AddQueryStringParametrs() {
    // добовляем в строку запроса параметры едущих туристов, кол-во взрослых, детей и возраста
    var strParametrs = "";
    if ($(".TFS_PersonsWrapper:visible").length > 0) {
        strParametrs = "&adults=" + $("#TbAdultsFilter").val();
        var childs = $("#TbChildsFilter").val();
        if (childs && childs > 0) {
            strParametrs += "&childs=" + childs;
            var age = $("#TbFirstChildAge").val();
            if (age && age > 0) {
                strParametrs += "&firstChildAge=" + age;
                if (childs > 1) {
                    age = $("#TbSecondChildAge").val();
                    if (age)
                        strParametrs += "&secondChildAge=" + age;
                }
            }
        }
    }

    // добавляем выбранные значения квот
    var strQuotaParametrs = "";
    if ($("#ChRoomsEnabled:checked").length > 0)
        strQuotaParametrs += "Availiable";
    if ($("#ChRoomsNo:checked").length > 0)
        strQuotaParametrs += ",%20No";
    if ($("#ChRoomsRQ:checked").length > 0)
        strQuotaParametrs += ",%20Request";
    if (strQuotaParametrs && strQuotaParametrs.length > 0) {
        if (strQuotaParametrs.indexOf(",%20") == 0)
            strQuotaParametrs = strQuotaParametrs.substr(4);
        strParametrs += "&hotelQuotaMask=" + strQuotaParametrs;
    }
    strQuotaParametrs = "";
    if ($("#ChAviaAvailiable:checked").length > 0)
        strQuotaParametrs += "Availiable";
    if ($("#ChAviaNo:checked").length > 0)
        strQuotaParametrs += ",%20No";
    if ($("#ChAviaRQ:checked").length > 0)
        strQuotaParametrs += ",%20Request";
    if (strQuotaParametrs && strQuotaParametrs.length > 0) {
        if (strQuotaParametrs.indexOf(",%20") == 0)
            strQuotaParametrs = strQuotaParametrs.substr(4);
        strParametrs += "&aviaQuotaMask=" + strQuotaParametrs;
    }

    // масимальную цену
    var strTmp = $("#TbMaxPrice").val();
    if (strTmp)
        strParametrs += "&priceLimit=" + strTmp;

    // выбранную валюту
    strTmp = $("#RblRates input:checked").val();
    if (strTmp)
        strParametrs += "&currency=" + strTmp;
    // приклеиваем выбранные параметры к другим в строке запроса
    if (strParametrs.length > 0) {
        $("form")[0].action += strParametrs;
    }
}

// проверяет корректность заполнения фильтров
function ValidateFilters() {
    if (!isAdultsNumberValid() || !isChildsNumberValid())
        return false;

    // проверяем что количество детей и возраст в указаны правильно.
    // Если дети указаны то для каждого из них должно быть заполнено значение возраста
    // в случае ошибки выдаем соотвествующее сообщение.
    var childWrapper = $("#ExtraChildrenDiv:visible")[0];
    if (!childWrapper)
        return true;
    var childNumber = $("#TbChildsFilter").val();
    var childAge = $("#TbFirstChildAge").val();
    if (childNumber > 0 && (childAge == "" || childAge < 0)) {
        alert("Вы не указали возраст первого ребенка. Подбор туров невозможен.");
        $("#TbFirstChildAge")[0].focus();
        return false;
    }
    childAge = $("#TbSecondChildAge").val();
    if (childNumber > 1 && (childAge == "" || childAge < 0)) {
        alert("Вы не указали возраст второго ребенка. Подбор туров невозможен.");
        $("#TbSecondChildAge")[0].focus();
        return false;
    }

    return true;
}

function showTourDescWnd(tourKey) {
    if (!tourKey)
        return;

    var wndUrl = window.location.protocol + "//" + window.location.host + '/windows/TourDescriptionWnd.aspx?tour=' + tourKey;
    var oWnd = window.radopen(wndUrl, null);
    //oWnd.set_autoSizeBehaviors(Telerik.Web.UI.WindowAutoSizeBehaviors.Width + Telerik.Web.UI.WindowAutoSizeBehaviors.Height);
    //oWnd.autoSize();
    oWnd.set_autoSize(false);
    oWnd.set_minHeight(300);
    oWnd.set_minWidth(500);
    oWnd.setSize(300, 500);
    oWnd.set_iconUrl('./styles/images/info-16x16.png');
    oWnd.set_showContentDuringLoad(true);
    oWnd.set_behaviors(window.Telerik.Web.UI.WindowBehaviors.Close + window.Telerik.Web.UI.WindowBehaviors.Reload + window.Telerik.Web.UI.WindowBehaviors.Move + window.Telerik.Web.UI.WindowBehaviors.Resize + window.Telerik.Web.UI.WindowBehaviors.Maximize);
    oWnd.set_enableShadow(true);
    oWnd.set_keepInScreenBounds(true);
    oWnd.set_visibleStatusbar(false);
    oWnd.set_visibleTitlebar(true);
    oWnd.set_modal(true);
    oWnd.center();
}