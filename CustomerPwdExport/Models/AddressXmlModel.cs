using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CustomerPwdExport.Models
{
    class AddressXmlModel
    {
        [XmlAttribute("address-id")]
        public string AddressID { get; set; }
        [XmlAttribute("preferred")]
        public bool Preferred { get; set; }

        public string salutation { get; set; }
        public string title { get; set; }

        [XmlElement("first-name")]
        public string firstName { get; set; }
        [XmlElement("second-name")]
        public string secondName { get; set; }
        [XmlElement("last-name")]
        public string lastName { get; set; }
    }
}
