using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WildWildWifi.Service
{
    [XmlRoot(ElementName = "SSID", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
    public class SSID
    {
        [XmlElement(ElementName = "hex", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public string Hex { get; set; }
        [XmlElement(ElementName = "name", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "SSIDConfig", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
    public class SSIDConfig
    {
        [XmlElement(ElementName = "SSID", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public SSID SSID { get; set; }
    }

    [XmlRoot(ElementName = "authEncryption", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
    public class AuthEncryption
    {
        [XmlElement(ElementName = "authentication", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public string Authentication { get; set; }
        [XmlElement(ElementName = "encryption", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public string Encryption { get; set; }
        [XmlElement(ElementName = "useOneX", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public bool UseOneX { get; set; }
    }

    [XmlRoot(ElementName = "sharedKey", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
    public class SharedKey
    {
        [XmlElement(ElementName = "keyType", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public string KeyType { get; set; }
        [XmlElement(ElementName = "protected", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public bool Protected { get; set; }
        [XmlElement(ElementName = "keyMaterial", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public string KeyMaterial { get; set; }
    }

    [XmlRoot(ElementName = "security", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
    public class Security
    {
        [XmlElement(ElementName = "authEncryption", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public AuthEncryption AuthEncryption { get; set; }
        [XmlElement(ElementName = "sharedKey", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public SharedKey SharedKey { get; set; }
    }

    [XmlRoot(ElementName = "MSM", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
    public class MSM
    {
        [XmlElement(ElementName = "security", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public Security Security { get; set; }
    }

    [XmlRoot(ElementName = "MacRandomization", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v3")]
    public class MacRandomization
    {
        [XmlElement(ElementName = "enableRandomization", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v3")]
        public bool EnableRandomization { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "WLANProfile", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
    public class WLANProfile
    {
        [XmlElement(ElementName = "name", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public string Name { get; set; }
        [XmlElement(ElementName = "SSIDConfig", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public SSIDConfig SSIDConfig { get; set; }
        [XmlElement(ElementName = "connectionType", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public string ConnectionType { get; set; }
        [XmlElement(ElementName = "connectionMode", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public string ConnectionMode { get; set; }
        [XmlElement(ElementName = "MSM", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v1")]
        public MSM MSM { get; set; }
        [XmlElement(ElementName = "MacRandomization", Namespace = "http://www.microsoft.com/networking/WLAN/profile/v3")]
        public MacRandomization MacRandomization { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }


        public static WLANProfile Default(string ssid)
        {
            return new WLANProfile()
            {
                ConnectionMode = "auto",
                ConnectionType = "ESS",
                MacRandomization = new MacRandomization() { EnableRandomization = false },
                Name = ssid,
                MSM = new MSM()
                {
                    Security = new Security()
                    {
                        AuthEncryption = new AuthEncryption()
                        {
                            Authentication = "WPA2PSK",
                            Encryption = "AES",
                            UseOneX = false
                        },
                        SharedKey = new SharedKey()
                        {
                            Protected = false,
                            KeyType = "passPhrase",
                            KeyMaterial = "default"
                        }
                    }
                },
                SSIDConfig = new SSIDConfig()
                {
                    SSID = new SSID()
                    {
                        Name = ssid,
                        Hex = BitConverter.ToString(Encoding.Default.GetBytes(ssid)).Replace("-", "")
                    }
                }
            };
        }
    }
}
