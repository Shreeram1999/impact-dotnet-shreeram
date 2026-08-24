// Task 3.9 - the Adapter pattern.
//
// Pretend this class comes from a third-party library we downloaded - we
// are NOT allowed to edit it (that's the whole point of the scenario: it's
// "third party"). It only knows how to generate a report from XML text.
// Our own application, meanwhile, works with JSON everywhere else. An
// Adapter's job is to sit in between and translate, so the rest of our
// code never has to deal with this XML-only class directly.
public class ThirdPartyXmlReportGenerator
{
    public string GenerateFromXml(string xml)
    {
        return $"[ThirdPartyXmlReportGenerator] Generated a report from:\n{xml}";
    }
}
