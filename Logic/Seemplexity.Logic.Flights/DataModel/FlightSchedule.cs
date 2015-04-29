using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Seemplexity.Logic.Flights.DataModel
{
    /// <summary>
    /// Расписание перелетов
    /// </summary>
    [DataContract]
    public class FlightSchedule
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public FlightSchedule()
        {
            Schedule = new List<DatesMatches>();
        }

        /// <summary>
        /// Сопоставления рейсов туда и обратно на даты
        /// </summary>
        [DataMember]
        public List<DatesMatches> Schedule;

        public override string ToString()
        {
            var result = Schedule.Aggregate(String.Empty, (current, sched) => current + (sched.ToString() + "_"));
            return result;
        }
    }
}
