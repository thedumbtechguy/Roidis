using System;

namespace Roidis.Attribute
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RoidPrefixAttribute : System.Attribute
    {
        public readonly string Prefix;

        public RoidPrefixAttribute(string Prefix)
        {
            if (string.IsNullOrWhiteSpace(Prefix))
                throw new ArgumentNullException(nameof(Prefix));

            this.Prefix = Prefix;
        }
    }
}