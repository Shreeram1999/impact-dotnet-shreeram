using System.Text;
using System.Text.Json;

// This is the interface OUR application code actually wants to program
// against: "give me a report generator that accepts JSON". Nothing in the
// rest of our app should need to know that, underneath, the real work is
// being done by an XML-only third-party class.
public interface IJsonReportGenerator
{
    string GenerateReport(string json);
}

// The Adapter itself: it implements the interface OUR code expects
// (IJsonReportGenerator), but internally it "wraps" the third-party class
// and translates between the two data formats. This is the classic Adapter
// shape: "make this thing I can't change look like the thing I actually need".
public class XmlReportAdapter : IJsonReportGenerator
{
    private readonly ThirdPartyXmlReportGenerator thirdPartyGenerator;

    public XmlReportAdapter(ThirdPartyXmlReportGenerator thirdPartyGenerator)
    {
        this.thirdPartyGenerator = thirdPartyGenerator;
    }

    public string GenerateReport(string json)
    {
        var xml = ConvertJsonToXml(json);
        return thirdPartyGenerator.GenerateFromXml(xml);
    }

    // A small, simple JSON -> XML converter. It only needs to handle a
    // flat JSON object (like {"Title":"...","Total":"..."}), which is all
    // this demo needs - a real adapter would handle more JSON shapes, but
    // the pattern itself (translate between two formats) is the same idea
    // no matter how complex the conversion gets.
    private static string ConvertJsonToXml(string json)
    {
        using var document = JsonDocument.Parse(json);
        var builder = new StringBuilder();
        builder.AppendLine("<Report>");

        foreach (var property in document.RootElement.EnumerateObject())
            builder.AppendLine($"  <{property.Name}>{property.Value}</{property.Name}>");

        builder.AppendLine("</Report>");
        return builder.ToString();
    }
}
