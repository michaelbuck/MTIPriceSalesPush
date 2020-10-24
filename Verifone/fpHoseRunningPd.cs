using System;
using System.Xml.Serialization;

namespace MTIPriceSalesPush.Verifone
{

  public class PosTotal
  {
    public string Name { get; set; }
    public int ProdId { get; set; }
    public decimal Amount { get; set; }
    public decimal Volume { get; set; }    
  }
  /// <remarks/>
  [Serializable(), System.ComponentModel.DesignerCategory("code"),
   XmlType(AnonymousType = true, Namespace = "urn:vfi-sapphire:pd.2002-05-21"),
   XmlRoot(Namespace = "urn:vfi-sapphire:pd.2002-05-21", IsNullable = false)]
  public class fpHoseRunningPd
  {
    /// <remarks />
    [XmlElement(Namespace = "urn:vfi-sapphire:vs.2001-10-01")]
    public period period { get; set; }

    /// <remarks />
    [XmlElement(Namespace = "urn:vfi-sapphire:vs.2001-10-01")]
    public string site { get; set; }

    /// <remarks />
    [XmlElement(Namespace = "")]
    public totals totals { get; set; }

    /// <remarks />
    [XmlElement("byFuelingPosition", Namespace = "")]
    public byFuelingPosition[] byFuelingPosition { get; set; }
  }

  /// <remarks />
  [XmlType(AnonymousType = true, Namespace = "urn:vfi-sapphire:vs.2001-10-01")]
  [XmlRoot(Namespace = "urn:vfi-sapphire:vs.2001-10-01", IsNullable = false)]
  public class period
  {
    /// <remarks />
    [XmlAttribute]
    public string name { get; set; }

    /// <remarks />
    [XmlAttribute]
    public DateTime periodBeginDate { get; set; }

    /// <remarks />
    [XmlAttribute]
    public int periodSeqNum { get; set; }

    /// <remarks />
    [XmlAttribute]
    public string periodType { get; set; }

    /// <remarks />
    [XmlAttribute]
    public int sysid { get; set; }
  }

  /// <remarks />
  [XmlType(AnonymousType = true)]
  [XmlRoot(Namespace = "", IsNullable = false)]
  public class totals
  {
    /// <remarks />
    [XmlElement("byFuelProduct")]
    public totalsByFuelProduct[] byFuelProduct { get; set; }

    /// <remarks />
    public totalsTotal total { get; set; }
  }

  /// <remarks />
  [XmlType(AnonymousType = true)]
  public class totalsByFuelProduct
  {
    /// <remarks />
    [XmlElement(Namespace = "urn:vfi-sapphire:fuel.2001-10-01")]
    public fuelProdBase fuelProdBase { get; set; }

    /// <remarks />
    public totalsByFuelProductFuelInfo fuelInfo { get; set; }
  }

  /// <remarks />
  [XmlType(AnonymousType = true, Namespace = "urn:vfi-sapphire:fuel.2001-10-01")]
  [XmlRoot(Namespace = "urn:vfi-sapphire:fuel.2001-10-01", IsNullable = false)]
  public class fuelProdBase
  {
    /// <remarks />
    [XmlElement(Namespace = "")]
    public string name { get; set; }

    /// <remarks />
    [XmlAttribute]
    public int number { get; set; }

    /// <remarks />
    [XmlAttribute]
    public int sysid { get; set; }
  }

  /// <remarks />
  [XmlType(AnonymousType = true)]
  public class totalsByFuelProductFuelInfo
  {
    /// <remarks />
    public uint count { get; set; }

    /// <remarks />
    public decimal amount { get; set; }

    /// <remarks />
    public decimal volume { get; set; }
  }

  /// <remarks />
  [XmlType(AnonymousType = true)]
  public class totalsTotal
  {
    /// <remarks />
    public uint count { get; set; }

    /// <remarks />
    public decimal amount { get; set; }

    /// <remarks />
    public decimal volume { get; set; }
  }

  /// <remarks />
  [XmlType(AnonymousType = true)]
  [XmlRoot(Namespace = "", IsNullable = false)]
  public class byFuelingPosition
  {
    /// <remarks />
    public int fuelingPosition { get; set; }

    /// <remarks />
    [XmlElement("byFuelProduct")]
    public byFuelingPositionByFuelProduct[] byFuelProduct { get; set; }

    /// <remarks />
    public byFuelingPositionTotal total { get; set; }
  }

  /// <remarks />
  [XmlType(AnonymousType = true)]
  public class byFuelingPositionByFuelProduct
  {
    /// <remarks />
    [XmlElement(Namespace = "urn:vfi-sapphire:fuel.2001-10-01")]
    public fuelProdBase fuelProdBase { get; set; }

    /// <remarks />
    public byFuelingPositionByFuelProductFuelInfo fuelInfo { get; set; }
  }

  /// <remarks />
  [XmlType(AnonymousType = true)]
  public class byFuelingPositionByFuelProductFuelInfo
  {
    /// <remarks />
    public int count { get; set; }

    /// <remarks />
    public decimal amount { get; set; }

    /// <remarks />
    public decimal volume { get; set; }
  }

  /// <remarks />
  [XmlType(AnonymousType = true)]
  public class byFuelingPositionTotal
  {
    /// <remarks />
    public int count { get; set; }

    /// <remarks />
    public decimal amount { get; set; }

    /// <remarks />
    public decimal volume { get; set; }
  }

}
