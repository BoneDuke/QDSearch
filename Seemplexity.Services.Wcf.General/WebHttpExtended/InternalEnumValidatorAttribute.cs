using System;
using System.Configuration;

namespace Seemplexity.Services.Wcf.WebHttpExtended
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class InternalEnumValidatorAttribute : ConfigurationValidatorAttribute
    {
        Type _enumHelperType;

        public InternalEnumValidatorAttribute(Type enumHelperType)
        {
            EnumHelperType = enumHelperType;
        }

        public Type EnumHelperType
        {
            get { return _enumHelperType; }
            set { _enumHelperType = value; }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get { return new InternalEnumValidator(_enumHelperType); }
        }
    }
}