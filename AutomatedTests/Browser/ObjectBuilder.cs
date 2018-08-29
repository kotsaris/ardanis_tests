namespace AutomatedTests.Browser
{
    public class ObjectBuilder
    {
        public ObjectBuilder(string page, string name, string path)
        {
            Page = page;
            Name = name;
            Path = path;
        }

        public string Page { get; private set; }
        public string Name { get; private set; }
        public string Path { get; private set; }
    }
}
