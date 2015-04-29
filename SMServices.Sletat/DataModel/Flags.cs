using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMServices.Sletat.DataModel
{
    /// <summary>
    /// Тип тура для слетать
    /// </summary>
    [Flags]
    public enum Flags : long
    {
        Recomended = 1,
        InstanConfirmation = 2,
        BestOffer = 4,
        EarlyBooking = 8,
        LateBooking = 16,
        Discount = 32,
        VipOffer = 64,
        CreditAvailable = 128,
        ExclusiveOffer = 256,
        Present = 512,
        Combined = 1024,
        ShopTour = 2048
    }
}
