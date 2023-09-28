using System.Text;

namespace BlazmExtension.Extensions
{
    public static class StringExtensions
    {
        public static string GetComponentNameOnCursor(this string lineText, int cursorPosition)
        {
            if (string.IsNullOrWhiteSpace(lineText))
            {
                return null;
            }

            int initialCursorPos = cursorPosition;

            string componentName = null;

            // 1. Start from the cursor position and move to the right until we find a closing tag
            while (cursorPosition < lineText.Length && lineText[cursorPosition] != '<' && lineText[cursorPosition] != '>')
            {
                cursorPosition++;
            }

            if (cursorPosition < lineText.Length && lineText[cursorPosition] == '>')
            {
                // 2. Ok, the input has a closing tag to the right, lets go back to the original cursor position
                // and move to the left to see if we can find an opening tag
                cursorPosition = initialCursorPos;
                while (cursorPosition >= 0 && lineText[cursorPosition] != '<' && (lineText[cursorPosition] != '>' || cursorPosition == initialCursorPos))
                {
                    cursorPosition--;
                }

                if (cursorPosition >= 0 && lineText[cursorPosition] == '<')
                {
                    // Ok, we found an opening tag, lets move to the right and capture the component name
                    // NOTE: This could be an opening, closing or self-closing tag. It works the same
                    var sb = new StringBuilder();
                    while (cursorPosition < lineText.Length && lineText[cursorPosition] != ' ' && lineText[cursorPosition] != '>' && lineText[cursorPosition] != '\n')
                    {
                        if (lineText[cursorPosition] != '<' && lineText[cursorPosition] != '/')
                        {
                            sb.Append(lineText[cursorPosition]);
                        }
                        cursorPosition++;
                    }
                    componentName = sb.ToString();
                }
            }

            return componentName;
        }
    }
}
