using System.Text;

namespace IngameScript
{
    partial class Program
    {
        public class StringBuilderExtended
        {
            readonly StringBuilder sb;

            public StringBuilderExtended(int size) { sb = new StringBuilder(size); }

            public StringBuilder GetSB() { return sb; }

            public string GetString() { return sb.ToString(); }

            public override string ToString() { return sb.ToString(); }

            public void Trim()
            {
                int wsFromBegin = 0;
                int wsFromEnd = 0;

                for (int i = sb.Length - 1; i >= 0; i--)
                {
                    if (char.IsWhiteSpace(sb[i]))
                    {
                        wsFromEnd++;
                    }
                    else
                    {
                        break;
                    }
                }

                for (int i = 0; i < sb.Length; i++)
                {
                    if (char.IsWhiteSpace(sb[i]))
                    {
                        wsFromBegin++;
                    }
                    else
                    {
                        break;
                    }
                }

                // All-whitespace content: both counters hold the full length,
                // so removing twice would underflow. Clear and leave early.
                if (wsFromBegin >= sb.Length)
                {
                    sb.Clear();
                    return;
                }

                sb.Remove(0, wsFromBegin);
                sb.Remove(sb.Length - wsFromEnd, wsFromEnd);
            }

            public void Substring(StringBuilderExtended targetSBX, int startindex, int lenght = -1)
            {
                Substring(targetSBX.sb, startindex, lenght);
            }

            public void Substring(StringBuilder targetSB, int startindex, int lenght = -1)
            {
                targetSB.Clear();

                if (startindex < 0 || startindex >= sb.Length)
                {
                    return;
                }

                // endindex is inclusive, so a length of n ends at startindex + n - 1
                int endindex = lenght > 0 ? startindex + lenght - 1 : sb.Length - 1;

                // Never read past the end of the buffer
                if (endindex > sb.Length - 1)
                {
                    endindex = sb.Length - 1;
                }

                for (int i = startindex; i <= endindex; i++)
                {
                    targetSB.Append(sb[i]);
                }
            }

            public bool IsEmpty() { return sb.Length == 0; }

            public bool Equal(string text) { if (sb.Length == text.Length && Contains(text)) return true; return false; }

            public void Insert(int pos, string text) { sb.Insert(pos, text); }

            public void Append(StringBuilderExtended sbx) { if (sbx == null) return; sb.Append(sbx.sb); }

            public void Append(string text) { sb.Append(text); }

            public void Append(char c) { sb.Append(c); }

            public void AppendLF(string text) { Append(text + "\n"); }

            public void AppendLF(StringBuilderExtended sbx) { if (sbx == null) return; Append(sbx); Append('\n'); }

            public void AppendLFifNotEmpty(string text) { if (text.Length > 0) AppendLF(text); }

            public void AppendLFifNotEmpty(StringBuilderExtended sbx) { if (sbx != null && !sbx.IsEmpty()) AppendLF(sbx); }

            public void Clear() { sb.Clear(); }


            public void SetText(string text1, string text2 = "", string text3 = "", string text4 = "", string text5 = "")
            {
                Clear();
                Append(text1);
                if (text2 != "") Append(text2);
                if (text3 != "") Append(text3);
                if (text4 != "") Append(text4);
                if (text5 != "") Append(text5);
            }

            public void SetText(StringBuilderExtended sbx) { Clear(); Append(sbx); }

            public void SetText(char c) { Clear(); Append(c); }

            public void ToLower() { for (int i = 0; i < sb.Length; i++) sb[i] = char.ToLower(sb[i]); }

            public bool Contains(string s) { return IndexOf(s) != -1; }

            public int IndexOf(string s)
            {
                if (s.Length == 0)
                {
                    return 0;
                }

                if (s.Length > sb.Length)
                {
                    return -1;
                }

                int lastStart = sb.Length - s.Length;

                for (int start = 0; start <= lastStart; start++)
                {
                    int offset = 0;

                    while (offset < s.Length && sb[start + offset] == s[offset])
                    {
                        offset++;
                    }

                    if (offset == s.Length)
                    {
                        return start;
                    }
                }

                return -1;
            }
        }
    }
}