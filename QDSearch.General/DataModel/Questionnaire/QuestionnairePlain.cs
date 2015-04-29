using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel.Questionnaire
{
    public class Question
    {
        public int Key { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string DefaulValue { get; set; }
        public string Comment { get; set; }
        public string Example { get; set; }
        public FormatType Format { get; set; }
        public IEnumerable<QuestionCase> Cases { get; set; }
    }
}
