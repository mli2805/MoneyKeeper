using System;
using System.Globalization;
using System.Xml.Serialization;
using Caliburn.Micro;

namespace Keeper.DomainModel
{
  [Serializable] // для binary formatter
  public class CurrencyRate : PropertyChangedBase
  {
    //    Все курсы валют хранятся относительно USD (дата - валюта - курс к доллару)
    public DateTime BankDay { get; set; }
    public CurrencyCodes Currency { get; set; }
    public Double Rate { get; set; }
  }

  /*
   * для XML сериализатора нужно чтобы класс был Public и был конструктор без параметров
   * (если нет ни одного конструктора - то без параметров создается компилятором)
   * 
   * чтобы поле было атрибутом , а не элементом - [XmlAttribute]
   * 
   * public поля для показа на форме не надо хранить - [XmlIgnore]
   * public без set - не сериализует, public с private set - runtime error при сериализации
   */
}
