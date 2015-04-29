namespace QDSearch.DataModel
{
    /// <summary>
    /// Тип запроса для составления запроса к БД на поиск.
    /// Для того, чтобы найти данные в базе нам нужно обратиться к трем таблицам
    /// </summary>
    public enum PagingCommandType
    {
        /// <summary>
        /// mwPriceDataTable - тут хранятся все данные по многоотельным турам. Данные из запроса попадают в темповую таблицу
        /// </summary>
        MultiHotelsTable,
        /// <summary>
        /// Таблица с данными по конкретному направлению
        /// </summary>
        TableXx,
        /// <summary>
        /// Темповая таблица с данными из таблицы mwPriceDataTable
        /// </summary>
        TableTemp
    }
}
