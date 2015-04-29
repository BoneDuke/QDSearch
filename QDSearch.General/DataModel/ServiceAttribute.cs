using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Атрибуты услуг (в конструкторе туров)
    /// </summary>
    [Flags, TypeConverter(typeof(ServiceAttributesConverter))]
    public enum ServiceAttribute
    {
        /// <summary>
        /// Без атрибута
        /// </summary>
        None = 0,
        /// <summary>
        /// Услуга может быть удалена
        /// </summary>
        Delete = 1,
        /// <summary>
        /// В услуге может быть изменено значения поля Code (для проживания - отель, рейс - для перелетов)
        /// </summary>
        CodeEdit = 2,
        /// <summary>
        /// Доп. описания услуги
        /// </summary>
        SubCodeEdit = 4,
        /// <summary>
        /// Город услуги
        /// </summary>
        CityEdit = 8,
        /// <summary>
        /// Партнер по услуге
        /// </summary>
        PartnerEdit = 16,
        /// <summary>
        /// ???
        /// </summary>
        BadCheck = 32,
        /// <summary>
        /// ???
        /// </summary>
        Invisible = 64,
        /// <summary>
        /// Изменение продолжительности
        /// </summary>
        LongEdit = 128,
        /// <summary>
        /// Полность изменяемая услуга
        /// </summary>
        FullEdit = Delete | CodeEdit | SubCodeEdit | CityEdit | PartnerEdit | LongEdit,
        /// <summary>
        /// Основная услуга (при связывании)
        /// </summary>
        Host = 256,
        /// <summary>
        /// Для связывания
        /// </summary>
        HostChangeCode2 = Host | 512,
        /// <summary>
        /// Для связывания
        /// </summary>
        HostChangeCode1 = Host | 1024,
        /// <summary>
        /// Для связывания
        /// </summary>
        HostChangeCode = Host | 2048,
        /// <summary>
        /// Для связывания
        /// </summary>
        HostChangePartner = Host | 4096,
        /// <summary>
        /// Для связывания
        /// </summary>
        HostChangePacket = Host | 8192,
        /// <summary>
        /// Зависимая услуга
        /// </summary>
        Depended = 16384,
        /// <summary>
        /// Для связывания
        /// </summary>
        DependedCode2 = Depended | 512,
        /// <summary>
        /// Для связывания
        /// </summary>
        DependedCode1 = Depended | 1024,
        /// <summary>
        /// Для связывания
        /// </summary>
        DependedCode = Depended | 2048,
        /// <summary>
        /// Для связывания
        /// </summary>
        DependedCity = Depended | 4096,
        /// <summary>
        /// Для связывания
        /// </summary>
        DependedCountry = Depended | 8192,
        /// <summary>
        /// Для связывания
        /// </summary>
        FullDepended = Depended | DependedCode2 | DependedCode1 | DependedCode | DependedCity | DependedCountry,
        /// <summary>
        /// Не расчитываемая при расчете быстрым прайсом
        /// </summary>
        NotCalculate = 32768
    }

    /// <summary>
    /// Класс для преобразования атрибутов услуг
    /// </summary>
    public class ServiceAttributesConverter : TypeConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(int))
                return true;
            else if (sourceType == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(int))
                return true;
            else if (destinationType == typeof(string))
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(int))
            {
                return (ServiceAttribute)value;
            }
            else if (value.GetType() == typeof(string))
            {
                return (ServiceAttribute)int.Parse((string)value, culture.NumberFormat);
            }
            else
                return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(int))
                return (int)value;
            else if (destinationType == typeof(string))
                return ((int)value).ToString(culture.NumberFormat);
            else
                return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
