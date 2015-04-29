function ScrollToElement(elementId) {
    // прокрутка на элемент с указанным id
    // взято из инета - кроссбраузерно
    if ($(elementId)[0])
        $('html, body').animate({ scrollTop: $(elementId).offset().top }, 1000);
}

