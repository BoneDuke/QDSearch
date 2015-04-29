using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Атрибуты онлайн-пользователей
    /// </summary>
    [Flags]
    public enum DupUserAttributes : int
    {
        /// <summary>
        /// Атрибут отсутствует
        /// </summary>
        None = 0,
        /// <summary>
        /// 1 - означает, что пароль зашифрован алгоритмом TripleDes, 0 - захэширован алгоритом MD5
        /// </summary>
        Converted = 1,
        /// <summary>
        /// Назначение наизвестно
        /// </summary>
        PersonalSubscribtion = 2,
        /// <summary>
        /// Назначение неизвестно
        /// </summary>
        FrameUser = 4,
        /// <summary>
        /// Участник бонусной программы
        /// </summary>
        BonusProgramParticipant = 8,
        /// <summary>
        /// Согласен получать уведомления (SMS и E-mail)
        /// </summary>
        EnableNotifications = 16,
        /// <summary>
        /// Флаг разрешает пользователю просматривать заказы всех сетей, в которых участвует его агентство
        /// </summary>
        CanSeeOrderdsOfNetworks = 32
    }
}
