// скрипт выполняющийся при каждой закгрузки контрола
// инициализирует переменные и события а так же любые другие действия которые необходимо выполнить на страницы после ее загрузки.
function startupHotelsFilter() {
    var tbHotelNameFilter = $("#TbHotelNameFilter");
    // привязываем фильтрацию имен отелей по вводиму в textbox имени отеля
    // стоит обертка для исключения повтороной привязки.
    if (!$(tbHotelNameFilter).data("showHideHotelsByFilter")) {
        $(tbHotelNameFilter).keyup(showHideHotelsByFilter);
        $(tbHotelNameFilter).data("showHideHotelsByFilter", true);
    }
    showHideHotelsByFilter();

    showHideSelectedHotels();

    // находим обертку для отеля у которого нет цен
    var disabledHotelWrapper = $(".HotelsFilterWrapper .OptionsList span.aspNetDisabled");
    if (disabledHotelWrapper.length > 0) {
        // находим чекбос для этого отеля, он нужен чтобы определить размеры активной области слоя чтобы поймать событие клик на за дисейбленном элементе
        // это необходимо для вывода сообщения почему элемент неактивен
        var checkBox = $(".HotelsFilterWrapper .OptionsList span.aspNetDisabled input");

        // добовляем слой поверх chekbox перехвата клика
        disabledHotelWrapper.append("<div style='position:absolute;'></div>");
        // уточняем стили для обертки неактивного отеля, необходимо для правильного позиционирования
        disabledHotelWrapper.css({ "position": "relative" });
        // получаем перекрывающий слой
        var clickArea = $(disabledHotelWrapper.selector + " div");
        // устанавливаем его размеры по размерам перекрываемого chebox
        clickArea.css({ "top": checkBox.position().top, "left": checkBox.position().left, "width": checkBox.outerWidth(true) + "px", "height": checkBox.outerHeight(true) + "px", "background-color": "#000000", "opacity": "0" });
        // вешаем на него обработчик события клик
        clickArea.click(function (e) {
            alert("Извините, этот отель нельзя выбрать, т.к. на указанную дату и продолжительность на данный отель нет цен. Если вас интересует именно этот отель, попробуйте сменить дату и продолжительность. Спасибо.");
            return false;
        });
    }
}

// скрывает отели взависмости от имени отеля введенного в форме поиска
function showHideHotelsByFilter() {
    var seachedName = $("#TbHotelNameFilter").val().toLowerCase();
    var strSelector = ".HotelsFilterWrapper .OptionsList label";

    $(strSelector).each(function (i) {
        if (seachedName == '' || ($(this).text().toLowerCase().indexOf(seachedName) + 1)) {
            var parentSpan = $(this).parent();
            if (!parentSpan.parent().hasClass("OptionsList"))
                parentSpan = null;

            if (parentSpan != null && parentSpan.length > 0) {
                parentSpan.show();
                parentSpan.next('br').first().show();
            } else {
                $(this).show();
                $(this).next('br').first().show();
                $(this).prev('input').first().show();
            }

        } else {
            var parentSpan = $(this).parent();
            if (!parentSpan.parent().hasClass("OptionsList"))
                parentSpan = null;

            if (parentSpan != null && parentSpan.length > 0) {
                parentSpan.hide();
                parentSpan.next('br').first().hide();
            } else {
                $(this).hide();
                $(this).next('br').first().hide();
                $(this).prev('input').first().hide();
            }

        }
    });
}

// метод реализуется синхронизация выбораных отелей в общем списке со списком выбранных отелей и действий производимых над ними.
// так же при удалении отелей из списка выбранных снимает с него выделение в общем списке,
function showHideSelectedHotels() {
    var targetEl = $(".HotelsFilterWrapper .SelectedOptionsList");
    $(targetEl).empty();

    $(".HotelsFilterWrapper .OptionsList *:checked").each(function (i) {
        $(this).clone(false).removeAttr("onclick").removeAttr("id").show().appendTo(targetEl).change(function (e) {
            var strSelector = ".HotelsFilterWrapper .OptionsList *:checked[value='" + $(this).val() + "']";
            $(strSelector).click();
        });
        $(this).next("label").first().clone(false).removeAttr("for").show().appendTo(targetEl);
        $(targetEl).append("<br />");
    });
}