using System.IO;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.IO;

[TestFixture]
public class InLineScriptFileTest
{
    [Test]
    public void OpenRead()
    {
        const string Content = "some text";

        var stream = new InLineScriptFile("name", Content).OpenRead();
        using (stream)
        {
            stream.ShouldNotBeNull();

            new StreamReader(stream).ReadToEnd().ShouldBe(Content);
        }
    }

    [Test]
    public void GetParent()
    {
        new InLineScriptFile("name", string.Empty).GetParent().ShouldBeNull();
    }
}