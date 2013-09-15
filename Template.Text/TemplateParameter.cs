using System;

namespace Template.Text
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true)]
    public class TemplateParameter : Attribute
    {
        public string Name;
        public string Property;

        public TemplateParameter ()
        {
        }

        public TemplateParameter (string name)
        {
            this.Name = name;
        }
    }
}

