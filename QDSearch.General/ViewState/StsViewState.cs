using System;

namespace QDSearch.ViewState
{
    /// <summary>
    /// Класс обертка над данными ViewState
    /// </summary>
    public class StsViewState
    {
        /// <summary>
        /// Id конкретного экземпляра ViewState служит для связи ViewState и конкретного экземпляра страницы.
        /// </summary>
        public Guid? Id { get; set; }
        /// <summary>
        /// Сам ViewState
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// Это время по прошествию которого при отсутсвии обращений к конкретному экзмепляру ViewState система сочтет его недействительным и удалит
        /// </summary>
        public uint Timeout { get; set; }
    }
}
