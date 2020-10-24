using System;
using System.Globalization;
using System.Xml;

namespace MTIPriceSalesPush.Verifone
{
  public static class XmlNodeExtensions
  {
    public static string GetAttributeString(this XmlNode node, string attributeName, string defaultValue = "")
    {
      if (node == null || node.Attributes == null) return defaultValue;
      var attribute = node.Attributes[attributeName];
      return (attribute == null) ? defaultValue : attribute.Value;
    }

    public static DateTimeOffset GetAttributeDateTimeOffset(this XmlNode node, string attributeName)
    {
      var value = GetAttributeString(node, attributeName);
      DateTimeOffset result;
      return (DateTimeOffset.TryParse(value, out result)) ? result : DateTimeOffset.MinValue;
    }

    public static bool GetAttributeBool(this XmlNode node, string attributeName, bool defaultValue = false)
    {
      var value = GetAttributeString(node, attributeName, defaultValue.ToString(CultureInfo.InvariantCulture));
      bool result;
      return (bool.TryParse(value, out result)) ? result : defaultValue;
    }

    public static int GetAttributeInt(this XmlNode node, string attributeName, int defaultValue = 0)
    {
      var value = GetAttributeString(node, attributeName, defaultValue.ToString(CultureInfo.InvariantCulture));
      int result;
      return (int.TryParse(value, out result)) ? result : defaultValue;
    }

    public static double GetAttributeDouble(this XmlNode node, string attributeName, double defaultValue = 0)
    {
      var value = GetAttributeString(node, attributeName, defaultValue.ToString(CultureInfo.InvariantCulture));
      double result;
      return (double.TryParse(value, out result)) ? result : defaultValue;
    }

    public static TEnum GetAttributeEnum<TEnum>(this XmlNode node, string attributeName, TEnum defaultValue)
      where TEnum : struct
    {
      TEnum result;
      var text = GetAttributeString(node, attributeName);
      return Enum.TryParse(text, true, out result) ? result : defaultValue;
    }
  }
}