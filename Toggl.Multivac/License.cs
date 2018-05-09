namespace Toggl.Multivac
{
    public struct License
    {
        public string Subject { get; set; }

        public string Text { get; set; }

        public License(string subject, string text)
        {
            Subject = subject;
            Text = text;
        }
    }
}
