namespace money.Tests;

using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;

[TestFixture]
public class CurrencyTests
{
    private CultureInfo _culture;

    [SetUp]
    public void SetUp() => _culture = Thread.CurrentThread.CurrentCulture;

    [TearDown]
    public void TearDown() => Thread.CurrentThread.CurrentCulture = _culture;

    [Test]
    public void Can_create_currency_using_culture_info()
    {
        CurrencyInfo currencyInfo = new CultureInfo("fr-FR");
        Assert.That(currencyInfo, Is.Not.Null);
    }

    [Test]
    public void Can_create_currency_using_currency_code()
    {
        CurrencyInfo currencyInfo = Currency.NZD;
        Assert.That(currencyInfo, Is.Not.Null);
    }

    [Test]
    public void Can_create_currency_using_current_culture()
    {
        CurrencyInfo currencyInfo = CultureInfo.CurrentCulture;
        Assert.That(currencyInfo, Is.Not.Null);
    }

    [Test]
    public void Can_create_currency_using_region_info()
    {
        CurrencyInfo currencyInfo = new RegionInfo("CA");
        Assert.That(currencyInfo, Is.Not.Null);
    }

    [Test]
    public void Currency_creation_creates_meaningful_display_cultures()
    {
        // If I'm from France, and I reference Canadian Dollars,
        // then the default culture for CAD should be fr-CA
        Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
        CurrencyInfo currencyInfo = Currency.CAD;
        Assert.That(new CultureInfo("fr-CA"), Is.EqualTo(currencyInfo.DisplayCulture));

        // If I'm from England, and I reference Canadian Dollars,
        // then the default culture for CAD should be en-CA
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        currencyInfo = Currency.CAD;
        Assert.That(new CultureInfo("en-CA"), Is.EqualTo(currencyInfo.DisplayCulture));

        // If I'm from Germany, and I reference Canadian Dollars,
        // then the default culture for CAD should be Canadian
        Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
        currencyInfo = Currency.CAD;
        Assert.That(currencyInfo.DisplayCulture, Is.EqualTo(new CultureInfo("en-CA")));

        // ... and it should not display as if it were in de currency!
        Money money = new(Currency.CAD, 1000);
        Assert.That(money.DisplayNative(), Is.EqualTo("$1,000.00"));

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
        money = new Money(1000);
        var german = new CultureInfo("de-DE");
        Console.WriteLine(money.DisplayIn(german));  // Output: $1,000.00
    }

    [Test]
    public void Currency_creation_creates_meaningful_native_regions()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
        CurrencyInfo currencyInfo = Currency.EUR;
        Assert.That(new RegionInfo("FR"), Is.EqualTo(currencyInfo.NativeRegion));

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
        currencyInfo = Currency.CAD;
        Assert.That(new RegionInfo("CA"), Is.EqualTo(currencyInfo.NativeRegion));
    }
}