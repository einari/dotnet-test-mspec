using System.Xml.Linq;
using System.Xml.XPath;

namespace Machine.Specifications.Runner.DotNet.Controller.Model
{
    public class SpecificationInfo
    {
        public string Leader { get; set; }
        public string Name { get; set; }
        public string ContainingType { get; set; }
        public string FieldName { get; set; }
        public string CapturedOutput { get; set; }

        public SpecificationInfo()
        {
        }

        public SpecificationInfo(string leader, string name, string containingType, string fieldName)
        {
            this.Leader = leader;
            this.Name = name;
            this.ContainingType = containingType;
            this.FieldName = fieldName;
        }

        public static SpecificationInfo Parse(string specificationInfoXml)
        {
            var document = XDocument.Parse(specificationInfoXml);
            var specificationInfoElement = document.XPathSelectElement("/specificationinfo");
            return GetFrom(specificationInfoElement);
        }

        public static SpecificationInfo GetFrom(XElement element)
        {
            var leader = element.SafeGet<string>("./leader");
            var name = element.SafeGet<string>("./name");
            var containingType = element.SafeGet<string>("./containingtype");
            var fieldName = element.SafeGet<string>("./fieldname");
            var capturedOutput = element.SafeGet<string>("./capturedoutput");

            return new SpecificationInfo(leader, name, containingType, fieldName)
                       {
                           CapturedOutput = capturedOutput,
                       };

        }
    }
}