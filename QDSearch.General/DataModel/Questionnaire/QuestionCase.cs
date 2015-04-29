using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel.Questionnaire
{
    public class QuestionCase
    {
        public int Key { get; set; }
        public string Value { get; set; }
        public bool IsDefault { get; set; }
        public int Order { get; set; }
    }
}
