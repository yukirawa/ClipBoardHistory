using System;

namespace myfarstAPP
{
    public class ClipboardItem
    {
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }

        public ClipboardItem(string text)
        {
            Text = text;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy/MM/dd HH:mm:ss}] {Text}";
        }
    }
}
