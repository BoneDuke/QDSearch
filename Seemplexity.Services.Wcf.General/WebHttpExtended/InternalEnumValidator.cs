using System;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;

namespace Seemplexity.Services.Wcf.WebHttpExtended
{
    internal class InternalEnumValidator : ConfigurationValidatorBase
    {
        readonly MethodInfo _isDefined;

        public InternalEnumValidator(Type enumHelperType)
        {
            _isDefined = enumHelperType.GetMethod("IsDefined", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public override bool CanValidate(Type type)
        {
            return (_isDefined != null);
        }

        public override void Validate(object value)
        {
            var retVal = (bool)_isDefined.Invoke(null, new[] { value });

            if (retVal) return;

            var isDefinedParameters = _isDefined.GetParameters();
            throw new InvalidEnumArgumentException("value", (int)value, isDefinedParameters[0].ParameterType);
        }
    }
}