using BlazmExtension.Extensions;

namespace BlazmExtension.Tests
{
    [TestClass]
    public class ExtensionTests
    {
        [DataRow(null, 0, null)]
        [DataRow("", 0, null)] 
        [DataRow(" ", 0, null)]
        [DataRow("<p></p><em></em>", 16, null)] // very end of string
        [DataRow("<p></p><em></em>", 15, "em")]
        [DataRow("<p></p><em></em>", 14, "em")]
        [DataRow("<p></p><em></em>", 13, "em")]
        [DataRow("<p></p><em></em>", 12, "em")] 
        [DataRow("<p></p><em></em>", 11, null)] // before "</em>"
        [DataRow("<p></p><em></em>", 10, "em")]
        [DataRow("<p></p><em></em>", 9, "em")]
        [DataRow("<p></p><em></em>", 8, "em")]
        [DataRow("<p></p><em></em>", 7, null)] // before "<em>"
        [DataRow("<p></p><em></em>", 6, "p")]
        [DataRow("<p></p><em></em>", 5, "p")]  
        [DataRow("<p></p><em></em>", 4, "p")]
        [DataRow("<p></p><em></em>", 3, null)] // before "</p>"
        [DataRow("<p></p><em></em>", 2, "p")]
        [DataRow("<p></p><em></em>", 1, "p")]
        [DataRow("<p></p><em></em>", 0, null)] // very start of string

        [DataRow("<p><em>stuff</em></p>", 21, null)] // very end of string
        [DataRow("<p><em>stuff</em></p>", 20, "p")]
        [DataRow("<p><em>stuff</em></p>", 18, "p")]
        [DataRow("<p><em>stuff</em></p>", 17, null)] // after "</em>" before "</p>"
        [DataRow("<p><em>stuff</em></p>", 16, "em")]
        [DataRow("<p><em>stuff</em></p>", 13, "em")]
        [DataRow("<p><em>stuff</em></p>", 12, null)] // before "</em>
        [DataRow("<p><em>stuff</em></p>", 7, null)] // after "<em>"
        [DataRow("<p><em>stuff</em></p>", 6, "em")]
        [DataRow("<p><em>stuff</em></p>", 5, "em")]
        [DataRow("<p><em>stuff</em></p>", 4, "em")]
        [DataRow("<p><em>stuff</em></p>", 3, null)] // before "<p>" and "<em>"
        [DataRow("<p><em>stuff</em></p>", 1, "p")]
        [DataRow("<p><em>stuff</em></p>", 2, "p")]
        [DataRow("<p><em>stuff</em></p>", 0,  null)] // before '<'
        [TestMethod]
        public void GetComponentNameOnCursor(string lineText, int cursorPosition, string expectedComponentName)
        {
            var componentName = lineText.GetComponentNameOnCursor(cursorPosition);
            Assert.AreEqual(expectedComponentName, componentName);
        }


    }
}