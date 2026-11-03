using System;
using System.Collections.Generic;
using System.Globalization;
using MiniJSON.Teak;

[TeakTestFixture]
public class TeakMiniJsonTests {
    static readonly string[] Cultures = {"en-US", "de-DE"};

    static void InCulture(string name, Action body) {
        var original = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = new CultureInfo(name);
            body();
        } finally {
            CultureInfo.CurrentCulture = original;
        }
    }

    static object ParseValue(string number) {
        var dict = (Dictionary<string, object>) Json.Deserialize("{\"v\":" + number + "}");
        return dict["v"];
    }

    [TeakTest]
    public void FractionParsesAsDouble() {
        foreach (var culture in Cultures) {
            InCulture(culture, () => {
                TeakAssert.AreEqual(0.1d, ParseValue("0.1"), culture);
                TeakAssert.AreEqual(-2.5d, ParseValue("-2.5"), culture);
            });
        }
    }

    [TeakTest]
    public void WholeNumberParsesAsLong() {
        foreach (var culture in Cultures) {
            InCulture(culture, () => {
                TeakAssert.AreEqual(1L, ParseValue("1"), culture);
                TeakAssert.AreEqual(-7L, ParseValue("-7"), culture);
            });
        }
    }

    [TeakTest]
    public void ExponentParsesAsDouble() {
        foreach (var culture in Cultures) {
            InCulture(culture, () => {
                TeakAssert.AreEqual(1000d, ParseValue("1e3"), culture);
                TeakAssert.AreEqual(1000d, ParseValue("1E3"), culture);
                TeakAssert.AreEqual(0.01d, ParseValue("1e-2"), culture);
            });
        }
    }

    [TeakTest]
    public void SerializesDoublesWithInvariantSeparator() {
        foreach (var culture in Cultures) {
            InCulture(culture, () => {
                TeakAssert.AreEqual("{\"v\":0.1}", Json.Serialize(new Dictionary<string, object> {{"v", 0.1d}}), culture);
                TeakAssert.AreEqual("{\"v\":-2.5}", Json.Serialize(new Dictionary<string, object> {{"v", -2.5f}}), culture);
            });
        }
    }

    [TeakTest]
    public void DoubleRoundTripsExactly() {
        foreach (var culture in Cultures) {
            InCulture(culture, () => {
                var value = 0.1d + 0.2d;
                var json = Json.Serialize(new Dictionary<string, object> {{"v", value}});
                TeakAssert.AreEqual(value, ((Dictionary<string, object>) Json.Deserialize(json))["v"], culture);
            });
        }
    }
}
